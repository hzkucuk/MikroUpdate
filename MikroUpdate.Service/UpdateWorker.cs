using System.IO.Pipes;

using MikroUpdate.Service.Services;
using MikroUpdate.Shared;
using MikroUpdate.Shared.Messages;
using MikroUpdate.Shared.Models;

namespace MikroUpdate.Service;

/// <summary>
/// Ana Worker servisi.
/// Named Pipe sunucusu ile tray uygulamasından komut alır ve güncelleme işlemlerini yönetir.
/// Periyodik versiyon kontrolü yapar.
/// </summary>
public sealed class UpdateWorker : BackgroundService
{
    private readonly ILogger<UpdateWorker> _logger;
    private readonly ConfigService _configService = new();
    private readonly VersionService _versionService = new();
    private readonly UpdateService _updateService = new();
    private UpdateConfig _config = new();
    private ServiceStatus _currentStatus = ServiceStatus.Idle;
    private string _statusMessage = "Servis başlatıldı.";
    private string? _lastServerVersion;
    private string? _lastLocalVersion;
    private bool _updateRequired;

    public UpdateWorker(ILogger<UpdateWorker> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _config = _configService.Load();
        _logger.LogInformation(
            "MikroUpdate Service başlatıldı. Ürün: {Product}, Sunucu: {Server}",
            _config.ProductName,
            _config.ServerSharePath);

        // Pipe sunucusu ve periyodik kontrol paralel çalışır
        Task pipeTask = RunPipeServerAsync(stoppingToken);
        Task timerTask = RunPeriodicCheckAsync(stoppingToken);

        await Task.WhenAll(pipeTask, timerTask).ConfigureAwait(false);
    }

    #region Pipe Server

    /// <summary>
    /// Named Pipe sunucusu — tray uygulamasından gelen komutları dinler.
    /// </summary>
    private async Task RunPipeServerAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using NamedPipeServerStream pipeServer = new(
                    PipeConstants.PipeName,
                    PipeDirection.InOut,
                    NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous);

                await pipeServer.WaitForConnectionAsync(stoppingToken).ConfigureAwait(false);
                _logger.LogDebug("Pipe bağlantısı alındı.");

                await HandlePipeConnectionAsync(pipeServer, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Pipe sunucu hatası.");
                await Task.Delay(1000, stoppingToken).ConfigureAwait(false);
            }
        }
    }

    private async Task HandlePipeConnectionAsync(
        NamedPipeServerStream pipeServer,
        CancellationToken stoppingToken)
    {
        try
        {
            // Komutu oku (length-prefixed)
            ServiceCommand? command = await PipeProtocol.ReadMessageAsync<ServiceCommand>(
                pipeServer, stoppingToken).ConfigureAwait(false);

            if (command is null)
            {
                return;
            }

            _logger.LogInformation("Komut alındı: {Command}", command.Command);

            // Komutu işle
            ServiceResponse response = command.Command switch
            {
                CommandType.CheckVersion => await HandleCheckVersionAsync(stoppingToken).ConfigureAwait(false),
                CommandType.RunUpdate => await HandleRunUpdateAsync(stoppingToken).ConfigureAwait(false),
                CommandType.GetStatus => BuildStatusResponse(),
                CommandType.ReloadConfig => HandleReloadConfig(),
                _ => new ServiceResponse { Success = false, Message = "Bilinmeyen komut." }
            };

            // Yanıtı yaz (length-prefixed)
            await PipeProtocol.WriteMessageAsync(pipeServer, response, stoppingToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Pipe komut işleme hatası.");
        }
    }

    #endregion

    #region Command Handlers

    private async Task<ServiceResponse> HandleCheckVersionAsync(CancellationToken stoppingToken)
    {
        _currentStatus = ServiceStatus.Checking;
        _statusMessage = "Versiyon kontrol ediliyor...";

        try
        {
            Version? serverVersion = _versionService.GetVersion(_config.ServerExePath);
            Version? localVersion = _versionService.GetVersion(_config.LocalExePath);

            _lastServerVersion = serverVersion?.ToString();
            _lastLocalVersion = localVersion?.ToString();
            _updateRequired = serverVersion is not null
                && (localVersion is null || localVersion < serverVersion);

            _currentStatus = ServiceStatus.Completed;

            if (_updateRequired)
            {
                _statusMessage = $"Güncelleme mevcut: {_lastLocalVersion ?? "Kurulu değil"} → {_lastServerVersion}";
                _logger.LogInformation("{Message}", _statusMessage);
            }
            else
            {
                _statusMessage = $"Terminal güncel: {_lastLocalVersion}";
                _logger.LogInformation("{Message}", _statusMessage);
            }

            return BuildStatusResponse();
        }
        catch (Exception ex)
        {
            _currentStatus = ServiceStatus.Error;
            _statusMessage = $"Versiyon kontrol hatası: {ex.Message}";
            _logger.LogError(ex, "Versiyon kontrol hatası.");

            return new ServiceResponse
            {
                Success = false,
                Status = ServiceStatus.Error,
                Message = _statusMessage
            };
        }
    }

    private async Task<ServiceResponse> HandleRunUpdateAsync(CancellationToken stoppingToken)
    {
        try
        {
            // 1. Versiyon kontrol
            ServiceResponse checkResponse = await HandleCheckVersionAsync(stoppingToken).ConfigureAwait(false);

            if (!checkResponse.UpdateRequired)
            {
                return new ServiceResponse
                {
                    Success = true,
                    Status = ServiceStatus.Completed,
                    Message = "Terminal zaten güncel.",
                    ServerVersion = _lastServerVersion,
                    LocalVersion = _lastLocalVersion
                };
            }

            // 2. Mikro sürecini kapat
            _currentStatus = ServiceStatus.Installing;
            _statusMessage = "Mikro süreci kapatılıyor...";
            int killed = _updateService.KillMikroProcess(_config.ExeFileName);
            _logger.LogInformation("{Killed} Mikro süreci kapatıldı.", killed);
            await Task.Delay(1500, stoppingToken).ConfigureAwait(false);

            // 3. Setup dosyasını kopyala
            _currentStatus = ServiceStatus.CopyingSetup;
            _statusMessage = "Setup dosyası kopyalanıyor...";
            string? setupPath = _updateService.CopySetupFromServer(_config.ServerSetupFilePath);

            if (string.IsNullOrEmpty(setupPath))
            {
                _currentStatus = ServiceStatus.Error;
                _statusMessage = "Setup dosyası sunucuda bulunamadı: " + _config.ServerSetupFilePath;
                _logger.LogError("{Message}", _statusMessage);

                return new ServiceResponse
                {
                    Success = false,
                    Status = ServiceStatus.Error,
                    Message = _statusMessage
                };
            }

            _logger.LogInformation("Setup kopyalandı: {Path}", setupPath);

            // 4. Sessiz kurulum
            _currentStatus = ServiceStatus.Installing;
            _statusMessage = "Kurulum yapılıyor...";
            int exitCode = await _updateService.RunSilentInstallAsync(
                setupPath, _config.LocalInstallPath, stoppingToken).ConfigureAwait(false);

            // 5. Temizlik
            UpdateService.CleanupTempFiles();

            // 6. Sonuç
            Version? newVersion = _versionService.GetVersion(_config.LocalExePath);
            _lastLocalVersion = newVersion?.ToString();
            _updateRequired = false;

            if (exitCode == 0)
            {
                _currentStatus = ServiceStatus.Completed;
                _statusMessage = $"Kurulum tamamlandı. Yeni versiyon: {_lastLocalVersion}";
                _logger.LogInformation("{Message}", _statusMessage);

                return new ServiceResponse
                {
                    Success = true,
                    Status = ServiceStatus.Completed,
                    Message = _statusMessage,
                    ServerVersion = _lastServerVersion,
                    LocalVersion = _lastLocalVersion
                };
            }
            else
            {
                _currentStatus = ServiceStatus.Error;
                _statusMessage = $"Kurulum hata kodu: {exitCode}";
                _logger.LogError("{Message}", _statusMessage);

                return new ServiceResponse
                {
                    Success = false,
                    Status = ServiceStatus.Error,
                    Message = _statusMessage
                };
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _currentStatus = ServiceStatus.Error;
            _statusMessage = $"Güncelleme hatası: {ex.Message}";
            _logger.LogError(ex, "Güncelleme hatası.");

            return new ServiceResponse
            {
                Success = false,
                Status = ServiceStatus.Error,
                Message = _statusMessage
            };
        }
    }

    private ServiceResponse HandleReloadConfig()
    {
        _config = _configService.Load();
        _logger.LogInformation(
            "Yapılandırma yeniden yüklendi. Ürün: {Product}", _config.ProductName);

        return new ServiceResponse
        {
            Success = true,
            Status = _currentStatus,
            Message = $"Yapılandırma yeniden yüklendi: {_config.ProductName}"
        };
    }

    private ServiceResponse BuildStatusResponse()
    {
        return new ServiceResponse
        {
            Success = true,
            Status = _currentStatus,
            Message = _statusMessage,
            ServerVersion = _lastServerVersion,
            LocalVersion = _lastLocalVersion,
            UpdateRequired = _updateRequired
        };
    }

    #endregion

    #region Periodic Check

    /// <summary>
    /// Periyodik versiyon kontrolü.
    /// </summary>
    private async Task RunPeriodicCheckAsync(CancellationToken stoppingToken)
    {
        // İlk kontrolde kısa bekle (servisin başlamasını bekle)
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken).ConfigureAwait(false);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogDebug("Periyodik versiyon kontrolü başlatılıyor...");
                await HandleCheckVersionAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Periyodik kontrol hatası.");
            }

            await Task.Delay(
                TimeSpan.FromMinutes(PipeConstants.CheckIntervalMinutes),
                stoppingToken).ConfigureAwait(false);
        }
    }

    #endregion
}

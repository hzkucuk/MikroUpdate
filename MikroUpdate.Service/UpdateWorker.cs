using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;

using MikroUpdate.Service.Services;
using MikroUpdate.Shared;
using MikroUpdate.Shared.Messages;
using MikroUpdate.Shared.Models;

namespace MikroUpdate.Service;

/// <summary>
/// Ana Worker servisi.
/// Named Pipe sunucusu ile tray uygulamasından komut alır ve güncelleme işlemlerini yönetir.
/// Periyodik versiyon kontrolü yapar. Çoklu modül desteği (Client, e-Defter, Beyanname).
/// </summary>
public sealed class UpdateWorker : BackgroundService
{
    private readonly ILogger<UpdateWorker> _logger;
    private readonly ConfigService _configService = new();
    private readonly VersionService _versionService = new();
    private readonly UpdateService _updateService = new();
    private OnlineVersionService? _onlineVersionService;
    private DownloadService? _downloadService;
    private UpdateConfig _config = new();
    private ServiceStatus _currentStatus = ServiceStatus.Idle;
    private string _statusMessage = "Servis başlatıldı.";
    private string? _lastServerVersion;
    private string? _lastLocalVersion;
    private bool _updateRequired;
    private List<ModuleVersionInfo> _moduleVersions = [];

    public UpdateWorker(ILogger<UpdateWorker> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    /// <summary>
    /// Yapılandırmaya göre HTTP servislerini (yeniden) oluşturur.
    /// Proxy/timeout ayarları config'den okunur.
    /// </summary>
    private void InitializeHttpServices()
    {
        // Mevcut servisleri temizle
        _onlineVersionService?.Dispose();
        _downloadService?.Dispose();

        string? proxy = string.IsNullOrWhiteSpace(_config.ProxyAddress) ? null : _config.ProxyAddress;
        int timeout = _config.HttpTimeoutSeconds;

        _onlineVersionService = new OnlineVersionService(_logger, proxy, timeout);
        _downloadService = new DownloadService(_logger, proxy, timeout);

        if (proxy is not null)
        {
            _logger.LogInformation("HTTP proxy yapılandırıldı: {Proxy}", proxy);
        }

        if (timeout > 0)
        {
            _logger.LogInformation("HTTP zaman aşımı: {Timeout} saniye", timeout);
        }
    }

    public override void Dispose()
    {
        _onlineVersionService?.Dispose();
        _downloadService?.Dispose();
        base.Dispose();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _config = _configService.Load();
        _config.EnsureModules();
        InitializeHttpServices();
        _logger.LogInformation(
            "MikroUpdate Service başlatıldı. Sürüm: {Version}, Ürün: {Product}, Modül: {Count}, Mod: {Mode}, Sunucu: {Server}",
            _config.MajorVersion,
            _config.ProductName,
            _config.Modules.Count,
            _config.UpdateMode,
            _config.UpdateMode == UpdateMode.Local ? _config.ServerSharePath : _config.CdnBaseUrl);

        // Self-update sonrası bekleyen tray app restart kontrolü
        await CheckPendingAppRestartAsync(stoppingToken).ConfigureAwait(false);

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
                await using NamedPipeServerStream pipeServer = CreatePipeServer();

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

    /// <summary>
    /// Named Pipe oluşturur. Önce PipeSecurity ACL ile dener (tüm kullanıcılar erişebilir),
    /// başarısız olursa temel constructor'a düşer (yalnızca admin/SYSTEM erişebilir).
    /// </summary>
    private NamedPipeServerStream CreatePipeServer()
    {
        try
        {
            // SYSTEM olarak çalışırken tüm kullanıcıların pipe'a bağlanabilmesini sağla
#pragma warning disable CA1416 // Windows-only servis — platform uyumluluğu garantili
            PipeSecurity pipeSecurity = new();
            pipeSecurity.AddAccessRule(new PipeAccessRule(
                new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                PipeAccessRights.ReadWrite,
                AccessControlType.Allow));

            NamedPipeServerStream server = NamedPipeServerStreamAcl.Create(
                PipeConstants.PipeName,
                PipeDirection.InOut,
                NamedPipeServerStream.MaxAllowedServerInstances,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous,
                inBufferSize: 0,
                outBufferSize: 0,
                pipeSecurity);
#pragma warning restore CA1416

            _logger.LogDebug("Pipe oluşturuldu (ACL: Everyone ReadWrite).");
            return server;
        }
        catch (Exception ex)
        {
            // PipeSecurity tipleri runtime'da yoksa veya ACL oluşturulamazsa
            // temel pipe ile devam et — en azından servis çalışır
            _logger.LogWarning(ex, "PipeSecurity ile pipe oluşturulamadı, temel pipe kullanılıyor.");

            return new NamedPipeServerStream(
                PipeConstants.PipeName,
                PipeDirection.InOut,
                NamedPipeServerStream.MaxAllowedServerInstances,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous);
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

            // DownloadUpdate özel: progress streaming gerektirir
            if (command.Command == CommandType.DownloadUpdate)
            {
                await HandleDownloadUpdateAsync(pipeServer, stoppingToken).ConfigureAwait(false);

                return;
            }

            // InstallSelfUpdate özel: installer'ı servis üzerinden çalıştırır (UAC'sız)
            if (command.Command == CommandType.InstallSelfUpdate)
            {
                await HandleInstallSelfUpdateAsync(pipeServer, command.Data, stoppingToken)
                    .ConfigureAwait(false);

                return;
            }

            // Diğer komutları işle (tek yanıt)
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
            // Güncelleme moduna göre versiyon kontrolü
            if (_config.UpdateMode == UpdateMode.Hybrid)
            {
                _logger.LogDebug("Hybrid versiyon kontrolü: önce yerel sunucu deneniyor...");
                _moduleVersions = _versionService.GetModuleVersions(_config);

                // Sunucu erişilemezse (tüm ServerVersion null) → CDN'e geç
                bool serverReachable = _moduleVersions.Exists(v => v.ServerVersion is not null);

                if (!serverReachable)
                {
                    _logger.LogInformation("Yerel sunucu erişilemedi, CDN probe'a geçiliyor...");

                    if (_onlineVersionService is null)
                    {
                        _logger.LogError("OnlineVersionService başlatılmamış — hybrid CDN fallback yapılamıyor.");

                        return new ServiceResponse
                        {
                            Success = false,
                            Status = ServiceStatus.Error,
                            Message = "Online servis başlatılmamış — CDN fallback yapılamıyor."
                        };
                    }

                    _moduleVersions = await _onlineVersionService
                        .GetOnlineModuleVersionsAsync(_config, stoppingToken).ConfigureAwait(false);
                }
            }
            else if (_config.UpdateMode == UpdateMode.Online)
            {
                _logger.LogDebug("Online versiyon kontrolü başlatılıyor...");

                if (_onlineVersionService is null)
                {
                    _logger.LogError("OnlineVersionService başlatılmamış — online kontrol yapılamıyor.");

                    return new ServiceResponse
                    {
                        Success = false,
                        Status = ServiceStatus.Error,
                        Message = "Online servis başlatılmamış."
                    };
                }

                _moduleVersions = await _onlineVersionService
                    .GetOnlineModuleVersionsAsync(_config, stoppingToken).ConfigureAwait(false);
            }
            else
            {
                _moduleVersions = _versionService.GetModuleVersions(_config);
            }

            _updateRequired = _moduleVersions.Exists(v => v.UpdateRequired);

            // Ana modül (Client) bilgilerini geriye uyumluluk için sakla
            ModuleVersionInfo? clientModule = _moduleVersions
                .Find(v => v.ModuleName.Equals("Client", StringComparison.OrdinalIgnoreCase));
            _lastServerVersion = clientModule?.ServerVersion;
            _lastLocalVersion = clientModule?.LocalVersion;

            _currentStatus = ServiceStatus.Completed;

            if (_updateRequired)
            {
                int count = _moduleVersions.Count(v => v.UpdateRequired);
                _statusMessage = $"{count} modülde güncelleme mevcut.";
                _logger.LogInformation("{Message}", _statusMessage);
            }
            else
            {
                _statusMessage = "Tüm modüller güncel.";
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
                    Message = "Tüm modüller güncel.",
                    ServerVersion = _lastServerVersion,
                    LocalVersion = _lastLocalVersion,
                    ModuleVersions = _moduleVersions
                };
            }

            // 2. Güncellenmesi gereken modüller
            List<UpdateModule> modulesToUpdate = _config.EnabledModules
                .Where(m => _moduleVersions.Exists(v =>
                    v.ModuleName == m.Name && v.UpdateRequired))
                .ToList();

            // 3. İlgili süreçleri kapat
            _currentStatus = ServiceStatus.Installing;
            _statusMessage = "Mikro süreçleri kapatılıyor...";
            HashSet<string> killedProcesses = [];

            foreach (UpdateModule module in modulesToUpdate)
            {
                if (killedProcesses.Add(module.ExeFileName))
                {
                    int killed = _updateService.KillMikroProcess(module.ExeFileName);
                    _logger.LogInformation("{ExeFile}: {Killed} süreç kapatıldı.", module.ExeFileName, killed);
                }
            }

            await Task.Delay(1500, stoppingToken).ConfigureAwait(false);

            // 4. Her modül için setup kopyala ve kur
            int successCount = 0;
            int failCount = 0;

            foreach (UpdateModule module in modulesToUpdate)
            {
                string serverSetupPath = Path.Combine(_config.SetupFilesPath, module.SetupFileName);

                _currentStatus = ServiceStatus.CopyingSetup;
                _statusMessage = $"{module.Name} setup kopyalanıyor...";
                string? setupPath = _updateService.CopySetupFromServer(serverSetupPath);

                if (string.IsNullOrEmpty(setupPath))
                {
                    _logger.LogError("{Module} setup bulunamadı: {Path}", module.Name, serverSetupPath);
                    failCount++;

                    continue;
                }

                _logger.LogInformation("{Module} setup kopyalandı: {Path}", module.Name, setupPath);

                _currentStatus = ServiceStatus.Installing;
                _statusMessage = $"{module.Name} kurulumu yapılıyor...";
                int exitCode = await _updateService.RunSilentInstallAsync(
                    setupPath, _config.LocalInstallPath, stoppingToken).ConfigureAwait(false);

                if (exitCode == 0)
                {
                    _logger.LogInformation("{Module} kurulumu başarılı.", module.Name);
                    successCount++;
                }
                else
                {
                    _logger.LogError("{Module} kurulum hata kodu: {ExitCode}", module.Name, exitCode);
                    failCount++;
                }
            }

            // 5. Temizlik
            UpdateService.CleanupTempFiles();

            // 6. Güncel versiyon bilgileri
            _moduleVersions = _versionService.GetModuleVersions(_config);
            _updateRequired = _moduleVersions.Exists(v => v.UpdateRequired);

            ModuleVersionInfo? clientModule = _moduleVersions
                .Find(v => v.ModuleName.Equals("Client", StringComparison.OrdinalIgnoreCase));
            _lastLocalVersion = clientModule?.LocalVersion;

            // 7. Sonuç
            if (failCount == 0)
            {
                _currentStatus = ServiceStatus.Completed;
                _statusMessage = $"{successCount} modül başarıyla güncellendi.";
                _logger.LogInformation("{Message}", _statusMessage);

                return new ServiceResponse
                {
                    Success = true,
                    Status = ServiceStatus.Completed,
                    Message = _statusMessage,
                    ServerVersion = _lastServerVersion,
                    LocalVersion = _lastLocalVersion,
                    ModuleVersions = _moduleVersions
                };
            }
            else
            {
                _currentStatus = ServiceStatus.Error;
                _statusMessage = $"{successCount} başarılı, {failCount} başarısız modül kurulumu.";
                _logger.LogError("{Message}", _statusMessage);

                return new ServiceResponse
                {
                    Success = false,
                    Status = ServiceStatus.Error,
                    Message = _statusMessage,
                    ServerVersion = _lastServerVersion,
                    LocalVersion = _lastLocalVersion,
                    ModuleVersions = _moduleVersions
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
        _config.EnsureModules();
        InitializeHttpServices();
        _logger.LogInformation(
            "Yapılandırma yeniden yüklendi. Ürün: {Product}, Sürüm: {Version}, Modül sayısı: {Count}",
            _config.ProductName, _config.MajorVersion, _config.Modules.Count);

        return new ServiceResponse
        {
            Success = true,
            Status = _currentStatus,
            Message = $"Yapılandırma yeniden yüklendi: {_config.MajorVersion} {_config.ProductName}"
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
            UpdateRequired = _updateRequired,
            ModuleVersions = _moduleVersions
        };
    }

    /// <summary>
    /// CDN'den güncelleme indirir ve kurar. Pipe üzerinden ilerleme bilgisi gönderir.
    /// Her ara mesaj IsProgressMessage=true ile gönderilir, son mesaj false ile.
    /// </summary>
    private async Task HandleDownloadUpdateAsync(
        NamedPipeServerStream pipeServer,
        CancellationToken stoppingToken)
    {
        try
        {
            // 1. Online versiyon kontrolü
            await SendProgressAsync(pipeServer, "CDN'de güncel versiyon aranıyor...", stoppingToken);

            ServiceResponse checkResponse = await HandleCheckVersionAsync(stoppingToken).ConfigureAwait(false);

            if (!checkResponse.UpdateRequired)
            {
                checkResponse.IsProgressMessage = false;
                await PipeProtocol.WriteMessageAsync(pipeServer, checkResponse, stoppingToken)
                    .ConfigureAwait(false);

                return;
            }

            if (_onlineVersionService is null)
            {
                await SendFinalResponseAsync(pipeServer, false, ServiceStatus.Error,
                    "Online versiyon servisi başlatılamamış.", stoppingToken);

                return;
            }

            string? cdnCode = _onlineVersionService.LatestCdnCode;

            if (string.IsNullOrEmpty(cdnCode))
            {
                await SendFinalResponseAsync(pipeServer, false, ServiceStatus.Error,
                    "CDN versiyon kodu tespit edilemedi.", stoppingToken);

                return;
            }

            _logger.LogInformation("CDN versiyon kodu: {CdnCode}", cdnCode);

            // 2. Güncellenmesi gereken modüller
            List<UpdateModule> modulesToUpdate = _config.EnabledModules
                .Where(m => _moduleVersions.Exists(v =>
                    v.ModuleName == m.Name && v.UpdateRequired))
                .ToList();

            // 3. İlgili süreçleri kapat
            await SendProgressAsync(pipeServer, "Mikro süreçleri kapatılıyor...", stoppingToken);
            HashSet<string> killedProcesses = [];

            foreach (UpdateModule module in modulesToUpdate)
            {
                if (killedProcesses.Add(module.ExeFileName))
                {
                    int killed = _updateService.KillMikroProcess(module.ExeFileName);
                    _logger.LogInformation("{ExeFile}: {Killed} süreç kapatıldı.", module.ExeFileName, killed);
                }
            }

            await Task.Delay(1500, stoppingToken).ConfigureAwait(false);

            // 4. Her modül için güncelleme al ve kur
            int successCount = 0;
            int failCount = 0;
            bool isHybrid = _config.UpdateMode == UpdateMode.Hybrid;

            foreach (UpdateModule module in modulesToUpdate)
            {
                string? setupPath = null;

                // Hybrid: önce yerel sunucudan kopyala
                if (isHybrid)
                {
                    string serverSetupPath = Path.Combine(_config.SetupFilesPath, module.SetupFileName);
                    await SendProgressAsync(pipeServer,
                        $"{module.Name} yerel sunucudan deneniyor...", stoppingToken);

                    setupPath = _updateService.CopySetupFromServer(serverSetupPath);

                    if (!string.IsNullOrEmpty(setupPath))
                    {
                        _logger.LogInformation("[Hybrid] {Module} yerel sunucudan kopyalandı.", module.Name);
                    }
                    else
                    {
                        _logger.LogInformation("[Hybrid] {Module} yerel sunucuda bulunamadı, CDN'e geçiliyor...", module.Name);
                    }
                }

                // Online veya Hybrid fallback: CDN'den indir
                if (string.IsNullOrEmpty(setupPath) && _downloadService is not null)
                {
                    _currentStatus = ServiceStatus.Downloading;
                    _statusMessage = $"{module.Name} CDN'den indiriliyor...";

                    setupPath = await _downloadService.DownloadSetupAsync(
                        _config,
                        module,
                        cdnCode,
                        onProgress: progress => SendProgressSync(pipeServer, progress, stoppingToken),
                        stoppingToken).ConfigureAwait(false);
                }

                if (string.IsNullOrEmpty(setupPath))
                {
                    string source = isHybrid ? "yerel sunucu ve CDN" : "CDN";
                    _logger.LogError("{Module} {Source}'den alınamadı.", module.Name, source);
                    await SendProgressAsync(pipeServer,
                        $"{module.Name} indirme başarısız ({source}).", stoppingToken);
                    failCount++;

                    continue;
                }

                // 4b. Sessiz kurulum
                _currentStatus = ServiceStatus.Installing;
                _statusMessage = $"{module.Name} kurulumu yapılıyor...";
                await SendProgressAsync(pipeServer, _statusMessage, stoppingToken);

                int exitCode = await _updateService.RunSilentInstallAsync(
                    setupPath, _config.LocalInstallPath, stoppingToken).ConfigureAwait(false);

                if (exitCode == 0)
                {
                    _logger.LogInformation("{Module} kurulumu başarılı.", module.Name);
                    await SendProgressAsync(pipeServer,
                        $"{module.Name} kurulumu tamamlandı.", stoppingToken);
                    successCount++;
                }
                else
                {
                    _logger.LogError("{Module} kurulum hata kodu: {ExitCode}", module.Name, exitCode);
                    failCount++;
                }
            }

            // 5. Temizlik
            DownloadService.CleanupTempFiles();

            // 6. Güncel versiyon bilgileri
            _moduleVersions = _versionService.GetModuleVersions(_config);
            _updateRequired = _moduleVersions.Exists(v => v.UpdateRequired);

            ModuleVersionInfo? clientModule = _moduleVersions
                .Find(v => v.ModuleName.Equals("Client", StringComparison.OrdinalIgnoreCase));
            _lastLocalVersion = clientModule?.LocalVersion;

            // 7. Son yanıt
            bool success = failCount == 0;
            string message = success
                ? $"{successCount} modül başarıyla güncellendi."
                : $"{successCount} başarılı, {failCount} başarısız modül kurulumu.";

            _currentStatus = success ? ServiceStatus.Completed : ServiceStatus.Error;
            _statusMessage = message;
            _logger.LogInformation("{Message}", _statusMessage);

            await SendFinalResponseAsync(pipeServer, success,
                _currentStatus, message, stoppingToken,
                moduleVersions: _moduleVersions);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _currentStatus = ServiceStatus.Error;
            _statusMessage = $"Online güncelleme hatası: {ex.Message}";
            _logger.LogError(ex, "Online güncelleme hatası.");

            await SendFinalResponseAsync(pipeServer, false,
                ServiceStatus.Error, _statusMessage, stoppingToken);
        }
    }

    /// <summary>
    /// Pipe üzerinden ara ilerleme mesajı gönderir (IsProgressMessage=true).
    /// </summary>
    private async Task SendProgressAsync(
        NamedPipeServerStream pipeServer,
        string statusText,
        CancellationToken cancellationToken)
    {
        ServiceResponse progress = new()
        {
            Success = true,
            Status = _currentStatus,
            Message = statusText,
            IsProgressMessage = true
        };

        await PipeProtocol.WriteMessageAsync(pipeServer, progress, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// DownloadService callback'inden pipe'a ilerleme bilgisi gönderir (senkron wrapper).
    /// </summary>
    private void SendProgressSync(
        NamedPipeServerStream pipeServer,
        DownloadProgressInfo downloadProgress,
        CancellationToken cancellationToken)
    {
        ServiceResponse progress = new()
        {
            Success = true,
            Status = ServiceStatus.Downloading,
            Message = downloadProgress.StatusText,
            IsProgressMessage = true,
            DownloadProgress = downloadProgress
        };

        // DownloadService callback'i senkron; pipe yazımını senkron bekle
        PipeProtocol.WriteMessageAsync(pipeServer, progress, cancellationToken)
            .GetAwaiter().GetResult();
    }

    /// <summary>
    /// Pipe üzerinden son (terminal) yanıt gönderir (IsProgressMessage=false).
    /// </summary>
    private async Task SendFinalResponseAsync(
        NamedPipeServerStream pipeServer,
        bool success,
        ServiceStatus status,
        string message,
        CancellationToken cancellationToken,
        List<ModuleVersionInfo>? moduleVersions = null)
    {
        ServiceResponse response = new()
        {
            Success = success,
            Status = status,
            Message = message,
            IsProgressMessage = false,
            ServerVersion = _lastServerVersion,
            LocalVersion = _lastLocalVersion,
            UpdateRequired = _updateRequired,
            ModuleVersions = moduleVersions ?? _moduleVersions
        };

        await PipeProtocol.WriteMessageAsync(pipeServer, response, cancellationToken)
            .ConfigureAwait(false);
    }

    #endregion

    #region Self-Update

    /// <summary>
    /// ProgramData altındaki restart flag dosyasının yolu.
    /// Self-update installer tamamlandıktan sonra tray app'i yeniden başlatmak için kullanılır.
    /// </summary>
    private static string RestartFlagPath =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "MikroUpdate", "pending_restart.flag");

    /// <summary>
    /// Tray app'in indirdiği self-update installer'ını SYSTEM yetkileriyle (UAC'sız) çalıştırır.
    /// Installer tamamlandıktan sonra tray app yeniden başlatılmak üzere flag dosyası yazılır.
    /// </summary>
    private async Task HandleInstallSelfUpdateAsync(
        NamedPipeServerStream pipeServer,
        string? installerPath,
        CancellationToken stoppingToken)
    {
        try
        {
            // 1. Installer yolunu doğrula
            if (string.IsNullOrWhiteSpace(installerPath) || !File.Exists(installerPath))
            {
                _logger.LogError("Self-update installer bulunamadı: {Path}", installerPath);

                await SendFinalResponseAsync(pipeServer, false, ServiceStatus.Error,
                    $"Installer dosyası bulunamadı: {installerPath}", stoppingToken);

                return;
            }

            _logger.LogInformation("Self-update installer başlatılıyor: {Path}", installerPath);

            // 2. Tray app'in yolunu belirle (servis exe'sinin yanındaki Win klasörü)
            string serviceDir = AppContext.BaseDirectory;
            string appDir = Path.GetFullPath(Path.Combine(serviceDir, "..", "Win"));
            string trayAppPath = Path.Combine(appDir, "MikroUpdate.exe");

            // 3. Restart flag dosyasını yaz (installer tamamlandıktan sonra kullanılacak)
            string flagDir = Path.GetDirectoryName(RestartFlagPath)!;

            if (!Directory.Exists(flagDir))
            {
                Directory.CreateDirectory(flagDir);
            }

            await File.WriteAllTextAsync(RestartFlagPath, trayAppPath, stoppingToken)
                .ConfigureAwait(false);

            _logger.LogInformation("Restart flag yazıldı: {FlagPath}", RestartFlagPath);

            // 4. Yanıtı gönder (pipe üzerinden tray app'e bildir — tray app kapanacak)
            await SendFinalResponseAsync(pipeServer, true, ServiceStatus.Installing,
                "Self-update başlatılıyor, uygulama yeniden başlatılacak.", stoppingToken);

            // 5. Tray app'in kapanmasını bekle (Session 0 izolasyonu nedeniyle
            //    Restart Manager cross-session app kapatamaz, tray app kendisi kapanıyor).
            //    File lock'ların serbest kalması için kısa bekleme.
            await Task.Delay(3000, stoppingToken).ConfigureAwait(false);

            // 6. Installer'ı başlat (/SILENT /SUPPRESSMSGBOXES /NOPOSTLAUNCH=1)
            //    SYSTEM olarak çalıştığımız için UAC gerekmez.
            //    /NOPOSTLAUNCH=1 ile ISS'nin [Run] bölümünde app başlatmasını engelliyoruz.
            using System.Diagnostics.Process? process = System.Diagnostics.Process.Start(
                new System.Diagnostics.ProcessStartInfo
                {
                    FileName = installerPath,
                    Arguments = "/SILENT /SUPPRESSMSGBOXES /NOPOSTLAUNCH=1",
                    UseShellExecute = false,
                    CreateNoWindow = true
                });

            if (process is null)
            {
                _logger.LogError("Self-update installer process başlatılamadı.");
                TryDeleteRestartFlag();

                return;
            }

            _logger.LogInformation("Installer PID: {PID}, bekleniyor...", process.Id);
            await process.WaitForExitAsync(stoppingToken).ConfigureAwait(false);

            if (process.ExitCode != 0)
            {
                _logger.LogError("Self-update installer hata ile sonlandı. ExitCode: {ExitCode}", process.ExitCode);
                TryDeleteRestartFlag();

                return;
            }

            _logger.LogInformation("Self-update installer başarıyla tamamlandı.");

            // 7. Tray app'i kullanıcı oturumunda yeniden başlat
            LaunchTrayAppInUserSession(trayAppPath);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Self-update hatası.");
            TryDeleteRestartFlag();

            // Hata durumunda client'a yanıt göndermeyi dene — yoksa tray app sonsuza kadar bekler
            try
            {
                if (pipeServer.IsConnected)
                {
                    await SendFinalResponseAsync(pipeServer, false, ServiceStatus.Error,
                        $"Self-update hatası: {ex.Message}", stoppingToken);
                }
            }
            catch
            {
                // Pipe zaten kopmuş olabilir, yutulabilir
            }
        }
    }

    /// <summary>
    /// Servis başlangıcında bekleyen restart flag'i kontrol eder.
    /// Installer tamamlandıktan sonra servis yeniden başlatıldıysa tray app'i kullanıcı oturumunda başlatır.
    /// Installer hâlâ çalışıyor olabileceği için gecikme ve yeniden deneme mekanizması içerir.
    /// </summary>
    private async Task CheckPendingAppRestartAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (!File.Exists(RestartFlagPath))
            {
                _logger.LogDebug("Bekleyen restart flag yok, devam ediliyor.");

                return;
            }

            string flagContent = File.ReadAllText(RestartFlagPath).Trim();
            _logger.LogInformation(
                "Bekleyen restart flag bulundu: {FlagPath}, İçerik: {Content}",
                RestartFlagPath, flagContent);

            // Installer hâlâ çalışıyor olabilir — sistemin oturmasını bekle
            _logger.LogInformation("Installer'ın tamamlanması bekleniyor (5 saniye)...");
            await Task.Delay(5000, cancellationToken).ConfigureAwait(false);

            // Retry mekanizması: kullanıcı oturumu token'ı henüz hazır olmayabilir
            const int maxRetries = 3;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                _logger.LogInformation(
                    "Tray app başlatma denemesi {Attempt}/{MaxRetries}",
                    attempt, maxRetries);

                bool launched = LaunchTrayAppInUserSession(flagContent);

                if (launched)
                {
                    _logger.LogInformation(
                        "Tray app başarıyla başlatıldı (deneme {Attempt}/{MaxRetries}).",
                        attempt, maxRetries);

                    return;
                }

                if (attempt < maxRetries)
                {
                    int delaySeconds = attempt * 5;
                    _logger.LogWarning(
                        "Tray app başlatılamadı (deneme {Attempt}/{MaxRetries}), {Delay} saniye sonra tekrar denenecek.",
                        attempt, maxRetries, delaySeconds);

                    await Task.Delay(delaySeconds * 1000, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    _logger.LogError(
                        "Tray app {MaxRetries} denemede de başlatılamadı.", maxRetries);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Tray app restart kontrolü iptal edildi.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bekleyen restart kontrolü hatası.");
        }
        finally
        {
            TryDeleteRestartFlag();
        }
    }

    /// <summary>
    /// Tray app'i aktif kullanıcı oturumunda başlatır.
    /// </summary>
    /// <param name="flagContent">Flag dosyasından okunan tray app yolu.</param>
    /// <returns>Başlatma başarılı ise true.</returns>
    private bool LaunchTrayAppInUserSession(string flagContent)
    {
        string trayAppPath = flagContent;

        if (string.IsNullOrWhiteSpace(trayAppPath) || !File.Exists(trayAppPath))
        {
            // Fallback: servis dizininden tahmin et
            string serviceDir = AppContext.BaseDirectory;
            trayAppPath = Path.GetFullPath(Path.Combine(serviceDir, "..", "Win", "MikroUpdate.exe"));
            _logger.LogInformation(
                "Flag içeriği geçersiz, fallback yol kullanılıyor: {Path}", trayAppPath);
        }

        if (!File.Exists(trayAppPath))
        {
            _logger.LogError("Tray app bulunamadı: {Path}", trayAppPath);

            return false;
        }

        _logger.LogInformation("Tray app başlatılıyor: {Path}", trayAppPath);

        return UserSessionLauncher.LaunchInUserSession(trayAppPath, _logger);
    }

    /// <summary>
    /// Restart flag dosyasını güvenli şekilde siler.
    /// </summary>
    private void TryDeleteRestartFlag()
    {
        try
        {
            if (File.Exists(RestartFlagPath))
            {
                File.Delete(RestartFlagPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Restart flag dosyası silinemedi: {Path}", RestartFlagPath);
        }
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
                TimeSpan.FromMinutes(_config.CheckIntervalMinutes),
                stoppingToken).ConfigureAwait(false);
        }
    }

    #endregion
}

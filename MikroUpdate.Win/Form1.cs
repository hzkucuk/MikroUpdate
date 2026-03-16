using MikroUpdate.Win.Models;
using MikroUpdate.Win.Services;

namespace MikroUpdate.Win;

public partial class Form1 : Form
{
    private readonly ConfigService _configService = new();
    private readonly VersionService _versionService = new();
    private readonly UpdateService _updateService = new();
    private readonly bool _autoMode;
    private UpdateConfig _config = new();
    private CancellationTokenSource? _cts;

    public Form1(bool autoMode = false)
    {
        InitializeComponent();
        _autoMode = autoMode;
        LoadConfig();
    }

    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);

        if (_autoMode)
        {
            try
            {
                await RunAutoModeAsync();
            }
            catch (Exception ex)
            {
                LogError($"Otomatik mod hatası: {ex.Message}");
            }
        }
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _updateService.Dispose();
        base.OnFormClosed(e);
    }

    private void LoadConfig()
    {
        try
        {
            _config = _configService.Load();
            LogInfo($"Ayarlar yüklendi: {_config.ProductName} | {_config.ServerSharePath}");
            LogInfo("Yapılandırma dosyası: " + ConfigService.GetConfigFilePath());
        }
        catch (Exception ex)
        {
            LogError($"Ayarlar yüklenirken hata: {ex.Message}");
        }
    }

    #region Button Event Handlers

    private void BtnSettings_Click(object? sender, EventArgs e)
    {
        using SettingsForm settingsForm = new()
        {
            Config = _config
        };

        if (settingsForm.ShowDialog(this) == DialogResult.OK)
        {
            try
            {
                _config = settingsForm.Config;
                _configService.Save(_config);
                LogSuccess($"Ayarlar kaydedildi: {_config.ProductName} | {_config.ServerSharePath}");
            }
            catch (Exception ex)
            {
                LogError($"Ayarlar kaydedilirken hata: {ex.Message}");
            }
        }
    }

    private async void BtnCheck_Click(object? sender, EventArgs e)
    {
        try
        {
            SetUIBusy(true);
            CheckVersions();
        }
        catch (Exception ex)
        {
            LogError($"Versiyon kontrol hatası: {ex.Message}");
        }
        finally
        {
            SetUIBusy(false);
        }
    }

    private async void BtnUpdate_Click(object? sender, EventArgs e)
    {
        try
        {
            SetUIBusy(true);
            _cts = new CancellationTokenSource();
            await RunUpdateAsync(_cts.Token);
        }
        catch (OperationCanceledException)
        {
            LogWarning("Güncelleme iptal edildi.");
        }
        catch (Exception ex)
        {
            LogError($"Güncelleme hatası: {ex.Message}");
        }
        finally
        {
            _cts?.Dispose();
            _cts = null;
            SetUIBusy(false);
        }
    }

    private void BtnLaunch_Click(object? sender, EventArgs e)
    {
        try
        {
            _updateService.LaunchMikro(_config.LocalExePath);
            LogSuccess($"{_config.ExeFileName} başlatıldı.");
        }
        catch (Exception ex)
        {
            LogError($"Mikro başlatılamadı: {ex.Message}");
        }
    }

    #endregion

    #region Version Check

    private void CheckVersions()
    {
        Version? localVersion = _versionService.GetVersion(_config.LocalExePath);
        Version? serverVersion = _versionService.GetVersion(_config.ServerExePath);

        _lblLocalVersion.Text = localVersion?.ToString() ?? "Kurulu değil";
        _lblServerVersion.Text = serverVersion?.ToString() ?? "Erişilemiyor";

        if (serverVersion is null)
        {
            SetStatus("Sunucu erişilemiyor", Color.Orange);
            LogWarning("Sunucu EXE dosyasına erişilemedi: " + _config.ServerExePath);
        }
        else if (localVersion is null)
        {
            SetStatus("Kurulum gerekli", Color.Red);
            LogWarning("Yerel Mikro kurulumu bulunamadı: " + _config.LocalExePath);
        }
        else if (localVersion < serverVersion)
        {
            SetStatus("Güncelleme mevcut!", Color.Red);
            LogWarning($"Güncelleme gerekli: {localVersion} → {serverVersion}");
        }
        else
        {
            SetStatus("Güncel", Color.LimeGreen);
            LogSuccess($"Terminal güncel: {localVersion}");
        }
    }

    private void SetStatus(string text, Color color)
    {
        _lblStatus.Text = text;
        _lblStatus.ForeColor = color;
    }

    #endregion

    #region Update Workflow

    private async Task RunUpdateAsync(CancellationToken cancellationToken)
    {
        // 1. Versiyon kontrol
        LogInfo("Versiyon kontrol ediliyor...");
        CheckVersions();

        Version? localVersion = _versionService.GetVersion(_config.LocalExePath);
        Version? serverVersion = _versionService.GetVersion(_config.ServerExePath);

        if (serverVersion is not null && localVersion is not null && localVersion >= serverVersion)
        {
            LogSuccess("Terminal zaten güncel, güncelleme gerekmiyor.");

            return;
        }

        // 2. Mikro sürecini kapat
        LogInfo($"{_config.ExeFileName} süreci kapatılıyor...");
        int killed = _updateService.KillMikroProcess(_config.ExeFileName);
        LogInfo(killed > 0 ? $"{killed} süreç kapatıldı." : "Çalışan süreç bulunamadı.");
        await Task.Delay(1500, cancellationToken);

        // 3. Setup dosyasını al
        string? setupPath = await GetSetupFileAsync(cancellationToken);

        if (string.IsNullOrEmpty(setupPath))
        {
            LogError("Setup dosyası bulunamadı ve CDN'den indirilemedi. Güncelleme iptal.");

            return;
        }

        // 4. Sessiz kurulum
        LogInfo($"Sessiz kurulum başlatılıyor: {Path.GetFileName(setupPath)}");
        LogInfo($"Hedef dizin: {_config.LocalInstallPath}");
        _prgProgress.Style = ProgressBarStyle.Marquee;

        int exitCode = await _updateService.RunSilentInstallAsync(
            setupPath, _config.LocalInstallPath, cancellationToken);

        _prgProgress.Style = ProgressBarStyle.Blocks;
        _prgProgress.Value = 100;

        if (exitCode == 0)
        {
            LogSuccess("Client kurulumu başarıyla tamamlandı.");
        }
        else
        {
            LogError($"Kurulum hata kodu ile tamamlandı: {exitCode}");
        }

        // 5. e-Defter kurulumu (opsiyonel)
        if (_config.IncludeEDefter)
        {
            await RunEDefterInstallAsync(cancellationToken);
        }

        // 6. Kurulum sonrası versiyon kontrol
        Version? newVersion = _versionService.GetVersion(_config.LocalExePath);

        if (newVersion is not null)
        {
            _lblLocalVersion.Text = newVersion.ToString();
            LogSuccess($"Yeni versiyon: {newVersion}");
        }

        // 7. Geçici dosyaları temizle
        UpdateService.CleanupTempFiles();
        LogInfo("Geçici dosyalar temizlendi.");

        // 8. Otomatik başlatma
        if (_config.AutoLaunchAfterUpdate && exitCode == 0)
        {
            LogInfo("Mikro başlatılıyor...");

            try
            {
                _updateService.LaunchMikro(_config.LocalExePath);
                LogSuccess($"{_config.ExeFileName} başlatıldı.");
            }
            catch (Exception ex)
            {
                LogError($"Mikro başlatılamadı: {ex.Message}");
            }
        }
    }

    private async Task<string?> GetSetupFileAsync(CancellationToken cancellationToken)
    {
        // Önce sunucu paylaşımını dene
        LogInfo("Setup dosyası sunucuda aranıyor: " + _config.ServerSetupFilePath);
        string? setupPath = _updateService.CopySetupFromServer(_config.ServerSetupFilePath);

        if (setupPath is not null)
        {
            LogSuccess("Setup dosyası sunucudan kopyalandı.");

            return setupPath;
        }

        // Sunucuda bulunamazsa CDN'den indir
        LogWarning("Sunucuda bulunamadı, CDN'den indiriliyor...");
        LogInfo("CDN URL: " + _config.CdnSetupUrl);

        try
        {
            _prgProgress.Value = 0;
            _prgProgress.Style = ProgressBarStyle.Blocks;

            Progress<int> progress = new(percent =>
            {
                if (IsHandleCreated)
                {
                    _prgProgress.Value = percent;
                }
            });

            setupPath = await _updateService.DownloadSetupFromCdnAsync(
                _config.CdnSetupUrl, progress, cancellationToken);

            LogSuccess("Setup dosyası CDN'den indirildi.");

            return setupPath;
        }
        catch (HttpRequestException ex)
        {
            LogError($"CDN indirme hatası: {ex.Message}");

            return null;
        }
    }

    private async Task RunEDefterInstallAsync(CancellationToken cancellationToken)
    {
        LogInfo("e-Defter kurulumu başlatılıyor...");

        string? eDefterPath = _updateService.CopySetupFromServer(_config.ServerEDefterSetupFilePath);

        if (eDefterPath is null)
        {
            LogWarning("e-Defter sunucuda bulunamadı, CDN'den indiriliyor...");

            try
            {
                Progress<int> progress = new(percent =>
                {
                    if (IsHandleCreated)
                    {
                        _prgProgress.Value = percent;
                    }
                });

                eDefterPath = await _updateService.DownloadSetupFromCdnAsync(
                    _config.CdnEDefterUrl, progress, cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                LogError($"e-Defter CDN indirme hatası: {ex.Message}");

                return;
            }
        }

        _prgProgress.Style = ProgressBarStyle.Marquee;
        int exitCode = await _updateService.RunSilentInstallAsync(
            eDefterPath, _config.LocalInstallPath, cancellationToken);

        _prgProgress.Style = ProgressBarStyle.Blocks;

        if (exitCode == 0)
        {
            LogSuccess("e-Defter kurulumu tamamlandı.");
        }
        else
        {
            LogError($"e-Defter kurulum hata kodu: {exitCode}");
        }
    }

    #endregion

    #region Auto Mode

    /// <summary>
    /// Otomatik mod: Versiyon kontrol et, gerekirse güncelle, Mikro'yu başlat.
    /// Kısayol: MikroUpdate.exe /auto
    /// </summary>
    private async Task RunAutoModeAsync()
    {
        LogInfo("═══ Otomatik mod başlatıldı ═══");

        _config = _configService.Load();

        LogInfo($"Ürün: {_config.ProductName} | EXE: {_config.ExeFileName}");
        LogInfo($"Sunucu: {_config.ServerSharePath}");
        LogInfo($"Terminal: {_config.LocalInstallPath}");

        bool updateNeeded = _versionService.IsUpdateRequired(_config);
        CheckVersions();

        if (!updateNeeded)
        {
            LogSuccess("Terminal güncel. Mikro başlatılıyor...");

            if (File.Exists(_config.LocalExePath))
            {
                _updateService.LaunchMikro(_config.LocalExePath);
            }
            else
            {
                LogWarning("Mikro EXE bulunamadı: " + _config.LocalExePath);
            }

            await Task.Delay(2000);
            Close();

            return;
        }

        LogWarning("Güncelleme gerekli, kurulum başlatılıyor...");
        SetStatus("Güncelleme yapılıyor...", Color.Orange);

        using CancellationTokenSource cts = new();
        await RunUpdateAsync(cts.Token);

        LogInfo("═══ Otomatik mod tamamlandı ═══");
        await Task.Delay(3000);
        Close();
    }

    #endregion

    #region UI Helpers

    private void SetUIBusy(bool busy)
    {
        _btnCheck.Enabled = !busy;
        _btnUpdate.Enabled = !busy;
        _btnSettings.Enabled = !busy;
        _btnLaunch.Enabled = !busy;

        if (!busy)
        {
            _prgProgress.Value = 0;
            _prgProgress.Style = ProgressBarStyle.Blocks;
        }
    }

    private void LogInfo(string message) => AppendLog(message, Color.White);
    private void LogSuccess(string message) => AppendLog(message, Color.LimeGreen);
    private void LogWarning(string message) => AppendLog(message, Color.Yellow);
    private void LogError(string message) => AppendLog(message, Color.OrangeRed);

    private void AppendLog(string message, Color color)
    {
        if (!IsHandleCreated)
        {
            return;
        }

        string timestamp = DateTime.Now.ToString("HH:mm:ss");
        _rtbLog.SelectionStart = _rtbLog.TextLength;
        _rtbLog.SelectionLength = 0;
        _rtbLog.SelectionColor = color;
        _rtbLog.AppendText($"[{timestamp}] {message}{Environment.NewLine}");
        _rtbLog.ScrollToCaret();
    }

    #endregion
}

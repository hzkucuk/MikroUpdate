using MikroUpdate.Shared.Messages;
using MikroUpdate.Shared.Models;
using MikroUpdate.Win.Services;

namespace MikroUpdate.Win;

public partial class Form1 : Form
{
    private readonly ConfigService _configService = new();
    private readonly VersionService _versionService = new();
    private readonly UpdateService _updateService = new();
    private readonly PipeClient _pipeClient = new();
    private readonly FileLogService _fileLog = new();
    private readonly bool _autoMode;
    private UpdateConfig _config = new();
    private CancellationTokenSource? _cts;
    private bool _forceExit;
    private bool _serviceAvailable;

    public Form1(bool autoMode = false)
    {
        InitializeComponent();
        _autoMode = autoMode;
        _pipeClient.OnError = message => _fileLog.Warning(message);
        LoadConfig();
    }

    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);

        try
        {
            _serviceAvailable = await _pipeClient.IsServiceRunningAsync();
            LogInfo(_serviceAvailable
                ? "MikroUpdate servisi algılandı — servis modu aktif."
                : "MikroUpdate servisi bulunamadı — doğrudan mod aktif.");

            if (_autoMode)
            {
                await RunAutoModeAsync();
            }
            else
            {
                await CheckVersionsAsync();
            }
        }
        catch (Exception ex)
        {
            LogError($"Başlatma hatası: {ex.Message}");
            _fileLog.Error("Uygulama başlatma hatası", ex);
            ShowTrayBalloon("Başlatma Hatası", ex.Message, ToolTipIcon.Error);
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        // Çarpı butonuna basıldığında tray'e küçült, çıkış menüsü ile kapat
        if (e.CloseReason == CloseReason.UserClosing && !_forceExit)
        {
            e.Cancel = true;
            MinimizeToTray();

            return;
        }

        _cts?.Cancel();
        _cts?.Dispose();
        _fileLog.Info("Uygulama kapatılıyor.");
        _fileLog.Dispose();
        _notifyIcon.Visible = false;
        base.OnFormClosing(e);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);

        if (WindowState == FormWindowState.Minimized)
        {
            MinimizeToTray();
        }
    }

    private void MinimizeToTray()
    {
        Hide();
        WindowState = FormWindowState.Normal;
        _notifyIcon.ShowBalloonTip(
            1000,
            "MikroUpdate",
            "Program arka planda çalışmaya devam ediyor.",
            ToolTipIcon.Info);
    }

    private void RestoreFromTray()
    {
        Show();
        WindowState = FormWindowState.Normal;
        Activate();
    }

    private void LoadConfig()
    {
        try
        {
            _config = _configService.Load();
            LogInfo($"Ayarlar yüklendi: {_config.ProductName} | {_config.ServerSharePath}");
            LogInfo("Yapılandırma dosyası: " + ConfigService.GetConfigFilePath());
            LogInfo("Log dizini: " + FileLogService.GetLogDirectory());
        }
        catch (Exception ex)
        {
            LogError($"Ayarlar yüklenirken hata: {ex.Message}");
            _fileLog.Error("Yapılandırma yükleme hatası", ex);
        }
    }

    #region Tray Menu Event Handlers

    private void TsmShow_Click(object? sender, EventArgs e)
    {
        RestoreFromTray();
    }

    private void TsmExit_Click(object? sender, EventArgs e)
    {
        _forceExit = true;
        Close();
    }

    #endregion

    #region Button Event Handlers

    private async void BtnSettings_Click(object? sender, EventArgs e)
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

                // Servis çalışıyorsa yapılandırmayı yeniden yüklemesini iste
                if (_serviceAvailable)
                {
                    ServiceResponse? response = await _pipeClient.SendCommandAsync(CommandType.ReloadConfig);

                    if (response?.Success == true)
                    {
                        LogInfo("Servis yapılandırmayı yeniden yükledi.");
                    }
                    else
                    {
                        LogWarning("Servis yapılandırma yeniden yüklemesine yanıt vermedi.");
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"Ayarlar kaydedilirken hata: {ex.Message}");
                _fileLog.Error("Ayar kaydetme hatası", ex);
                ShowTrayBalloon("Ayar Hatası", "Ayarlar kaydedilirken bir hata oluştu.", ToolTipIcon.Error);
            }
        }
    }

    private async void BtnCheck_Click(object? sender, EventArgs e)
    {
        try
        {
            SetUIBusy(true);
            await CheckVersionsAsync();
        }
        catch (Exception ex)
        {
            LogError($"Versiyon kontrol hatası: {ex.Message}");
            _fileLog.Error("Versiyon kontrol hatası", ex);
            ShowTrayBalloon("Kontrol Hatası", "Versiyon kontrolü sırasında hata oluştu.", ToolTipIcon.Error);
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
            _fileLog.Warning("Güncelleme kullanıcı tarafından iptal edildi.");
        }
        catch (Exception ex)
        {
            LogError($"Güncelleme hatası: {ex.Message}");
            _fileLog.Error("Güncelleme hatası", ex);
            ShowTrayBalloon("Güncelleme Hatası", "Güncelleme sırasında bir hata oluştu.", ToolTipIcon.Error);
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
        LaunchMikro();
    }

    #endregion

    #region Version Check

    /// <summary>
    /// Versiyon kontrolü — servis varsa pipe üzerinden, yoksa doğrudan dosya sistemi.
    /// </summary>
    private async Task CheckVersionsAsync(CancellationToken cancellationToken = default)
    {
        if (_serviceAvailable)
        {
            await CheckVersionsViaServiceAsync(cancellationToken);
        }
        else
        {
            CheckVersionsDirect();
        }
    }

    private async Task CheckVersionsViaServiceAsync(CancellationToken cancellationToken)
    {
        LogInfo("Servis üzerinden versiyon kontrol ediliyor...");

        ServiceResponse? response = await _pipeClient.SendCommandAsync(
            CommandType.CheckVersion, cancellationToken).ConfigureAwait(true);

        if (response is null)
        {
            LogWarning("Servis yanıt vermedi, doğrudan moda geçiliyor...");
            _fileLog.Warning("Servis pipe yanıt vermedi — doğrudan moda fallback.");
            _serviceAvailable = false;
            CheckVersionsDirect();

            return;
        }

        _lblLocalVersion.Text = response.LocalVersion ?? "Kurulu değil";
        _lblServerVersion.Text = response.ServerVersion ?? "Erişilemiyor";

        if (!response.Success)
        {
            SetStatus("Hata", Color.OrangeRed);
            LogError(response.Message ?? "Versiyon kontrol hatası.");
            ShowTrayBalloon("Kontrol Hatası", response.Message ?? "Versiyon kontrol hatası.", ToolTipIcon.Error);
        }
        else if (response.UpdateRequired)
        {
            SetStatus("Güncelleme mevcut!", Color.Red);
            LogWarning(response.Message ?? "Güncelleme gerekli.");
            ShowTrayBalloon("Güncelleme Mevcut", $"Yeni sürüm: {response.ServerVersion}", ToolTipIcon.Warning);
        }
        else
        {
            SetStatus("Güncel", Color.LimeGreen);
            LogSuccess(response.Message ?? "Terminal güncel.");
        }
    }

    private void CheckVersionsDirect()
    {
        Version? localVersion = null;
        Version? serverVersion = null;

        try
        {
            localVersion = _versionService.GetVersion(_config.LocalExePath);
        }
        catch (Exception ex)
        {
            LogError($"Yerel versiyon okunamadı: {ex.Message}");
            _fileLog.Error($"Yerel versiyon okuma hatası: {_config.LocalExePath}", ex);
        }

        try
        {
            serverVersion = _versionService.GetVersion(_config.ServerExePath);
        }
        catch (Exception ex)
        {
            LogError($"Sunucu versiyonu okunamadı: {ex.Message}");
            _fileLog.Error($"Sunucu versiyon okuma hatası: {_config.ServerExePath}", ex);
        }

        _lblLocalVersion.Text = localVersion?.ToString() ?? "Kurulu değil";
        _lblServerVersion.Text = serverVersion?.ToString() ?? "Erişilemiyor";

        if (serverVersion is null)
        {
            SetStatus("Sunucu erişilemiyor", Color.Orange);
            LogWarning("Sunucu EXE dosyasına erişilemedi: " + _config.ServerExePath);
            ShowTrayBalloon("Bağlantı Sorunu", "Sunucu EXE dosyasına erişilemiyor.", ToolTipIcon.Warning);
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
            ShowTrayBalloon("Güncelleme Mevcut", $"Yeni sürüm: {serverVersion}", ToolTipIcon.Warning);
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
        _notifyIcon.Text = $"MikroUpdate — {text}";
    }

    #endregion

    #region Update Workflow

    private async Task RunUpdateAsync(CancellationToken cancellationToken)
    {
        if (_serviceAvailable)
        {
            await RunUpdateViaServiceAsync(cancellationToken);
        }
        else
        {
            await RunUpdateDirectAsync(cancellationToken);
        }
    }

    private async Task RunUpdateViaServiceAsync(CancellationToken cancellationToken)
    {
        LogInfo("Servis üzerinden güncelleme başlatılıyor...");
        _prgProgress.Style = ProgressBarStyle.Marquee;

        ServiceResponse? response = await _pipeClient.SendCommandAsync(
            CommandType.RunUpdate, cancellationToken).ConfigureAwait(true);

        _prgProgress.Style = ProgressBarStyle.Blocks;

        if (response is null)
        {
            LogWarning("Servis yanıt vermedi, doğrudan moda geçiliyor...");
            _fileLog.Warning("Güncelleme sırasında servis pipe yanıt vermedi — doğrudan moda fallback.");
            _serviceAvailable = false;
            await RunUpdateDirectAsync(cancellationToken);

            return;
        }

        _lblLocalVersion.Text = response.LocalVersion ?? _lblLocalVersion.Text;
        _lblServerVersion.Text = response.ServerVersion ?? _lblServerVersion.Text;

        if (response.Success)
        {
            _prgProgress.Value = 100;
            SetStatus("Güncelleme tamamlandı", Color.LimeGreen);
            LogSuccess(response.Message ?? "Güncelleme başarıyla tamamlandı.");
            ShowTrayBalloon("Güncelleme Tamamlandı", $"Yeni sürüm: {response.LocalVersion}", ToolTipIcon.Info);

            if (_config.AutoLaunchAfterUpdate)
            {
                LaunchMikro();
            }
        }
        else
        {
            SetStatus("Hata", Color.OrangeRed);
            LogError(response.Message ?? "Güncelleme sırasında hata oluştu.");
            _fileLog.Error($"Servis güncelleme hatası: {response.Message}");
            ShowTrayBalloon("Güncelleme Hatası", response.Message ?? "Güncelleme başarısız.", ToolTipIcon.Error);
        }
    }

    private async Task RunUpdateDirectAsync(CancellationToken cancellationToken)
    {
        // 1. Versiyon kontrol
        LogInfo("Versiyon kontrol ediliyor (doğrudan mod)...");
        CheckVersionsDirect();

        Version? localVersion = null;
        Version? serverVersion = null;

        try
        {
            localVersion = _versionService.GetVersion(_config.LocalExePath);
            serverVersion = _versionService.GetVersion(_config.ServerExePath);
        }
        catch (Exception ex)
        {
            LogError($"Versiyon okuma hatası: {ex.Message}");
            _fileLog.Error("Doğrudan güncelleme versiyon okuma hatası", ex);
        }

        if (serverVersion is not null && localVersion is not null && localVersion >= serverVersion)
        {
            LogSuccess("Terminal zaten güncel, güncelleme gerekmiyor.");

            return;
        }

        // 2. Mikro sürecini kapat
        try
        {
            LogInfo($"{_config.ExeFileName} süreci kapatılıyor...");
            int killed = _updateService.KillMikroProcess(_config.ExeFileName);
            LogInfo(killed > 0 ? $"{killed} süreç kapatıldı." : "Çalışan süreç bulunamadı.");
        }
        catch (Exception ex)
        {
            LogError($"Süreç kapatma hatası: {ex.Message}");
            _fileLog.Error("Mikro süreç kapatma hatası", ex);
        }

        await Task.Delay(1500, cancellationToken);

        // 3. Setup dosyasını sunucudan al
        LogInfo("Setup dosyası sunucuda aranıyor: " + _config.ServerSetupFilePath);
        string? setupPath;

        try
        {
            setupPath = _updateService.CopySetupFromServer(_config.ServerSetupFilePath);
        }
        catch (Exception ex)
        {
            LogError($"Setup kopyalama hatası: {ex.Message}");
            _fileLog.Error("Setup dosyası sunucudan kopyalanamadı", ex);
            ShowTrayBalloon("Güncelleme Hatası", "Setup dosyası sunucudan kopyalanamadı.", ToolTipIcon.Error);

            return;
        }

        if (string.IsNullOrEmpty(setupPath))
        {
            LogError("Setup dosyası sunucuda bulunamadı: " + _config.ServerSetupFilePath);
            LogError("Güncelleme iptal edildi.");
            _fileLog.Error($"Setup dosyası bulunamadı: {_config.ServerSetupFilePath}");
            ShowTrayBalloon("Güncelleme Hatası", "Setup dosyası sunucuda bulunamadı.", ToolTipIcon.Error);

            return;
        }

        LogSuccess("Setup dosyası sunucudan kopyalandı.");

        // 4. Sessiz kurulum
        LogInfo($"Sessiz kurulum başlatılıyor: {Path.GetFileName(setupPath)}");
        LogInfo($"Hedef dizin: {_config.LocalInstallPath}");
        _prgProgress.Style = ProgressBarStyle.Marquee;

        int exitCode;

        try
        {
            exitCode = await _updateService.RunSilentInstallAsync(
                setupPath, _config.LocalInstallPath, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _prgProgress.Style = ProgressBarStyle.Blocks;
            LogError($"Kurulum çalıştırma hatası: {ex.Message}");
            _fileLog.Error("Sessiz kurulum çalıştırma hatası", ex);
            ShowTrayBalloon("Kurulum Hatası", "Kurulum işlemi başarısız oldu.", ToolTipIcon.Error);

            return;
        }

        _prgProgress.Style = ProgressBarStyle.Blocks;
        _prgProgress.Value = 100;

        if (exitCode == 0)
        {
            SetStatus("Güncelleme tamamlandı", Color.LimeGreen);
            LogSuccess("Client kurulumu başarıyla tamamlandı.");
            ShowTrayBalloon("Güncelleme Tamamlandı", "Kurulum başarıyla tamamlandı.", ToolTipIcon.Info);
        }
        else
        {
            SetStatus("Kurulum hatası", Color.OrangeRed);
            LogError($"Kurulum hata kodu ile tamamlandı: {exitCode}");
            _fileLog.Error($"Sessiz kurulum hata kodu: {exitCode}");
            ShowTrayBalloon("Kurulum Hatası", $"Kurulum hata kodu: {exitCode}", ToolTipIcon.Error);
        }

        // 5. Kurulum sonrası versiyon kontrol
        try
        {
            Version? newVersion = _versionService.GetVersion(_config.LocalExePath);

            if (newVersion is not null)
            {
                _lblLocalVersion.Text = newVersion.ToString();
                LogSuccess($"Yeni versiyon: {newVersion}");
            }
        }
        catch (Exception ex)
        {
            LogWarning($"Kurulum sonrası versiyon okunamadı: {ex.Message}");
            _fileLog.Error("Kurulum sonrası versiyon okuma hatası", ex);
        }

        // 6. Geçici dosyaları temizle
        try
        {
            UpdateService.CleanupTempFiles();
            LogInfo("Geçici dosyalar temizlendi.");
        }
        catch (Exception ex)
        {
            LogWarning($"Geçici dosya temizleme hatası: {ex.Message}");
            _fileLog.Warning($"Geçici dosya temizleme hatası: {ex.Message}");
        }

        // 7. Otomatik başlatma
        if (_config.AutoLaunchAfterUpdate && exitCode == 0)
        {
            LaunchMikro();
        }
    }

    private void LaunchMikro()
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
            _fileLog.Error("Mikro başlatma hatası", ex);
            ShowTrayBalloon("Başlatma Hatası", $"{_config.ExeFileName} başlatılamadı.", ToolTipIcon.Error);
        }
    }

    #endregion

    #region Auto Mode

    /// <summary>
    /// Otomatik mod: Versiyon kontrol et, gerekirse güncelle, Mikro'yu başlat.
    /// Servis varsa pipe üzerinden, yoksa doğrudan mod.
    /// Kısayol: MikroUpdate.exe /auto
    /// </summary>
    private async Task RunAutoModeAsync()
    {
        LogInfo("═══ Otomatik mod başlatıldı ═══");
        _fileLog.Info("═══ Otomatik mod başlatıldı ═══");

        try
        {
            _config = _configService.Load();
        }
        catch (Exception ex)
        {
            LogError($"Otomatik mod yapılandırma hatası: {ex.Message}");
            _fileLog.Error("Otomatik mod yapılandırma yükleme hatası", ex);
            ShowTrayBalloon("Otomatik Mod Hatası", "Yapılandırma yüklenemedi.", ToolTipIcon.Error);

            return;
        }

        LogInfo($"Ürün: {_config.ProductName} | EXE: {_config.ExeFileName}");
        LogInfo($"Sunucu: {_config.ServerSharePath}");
        LogInfo($"Terminal: {_config.LocalInstallPath}");
        LogInfo(_serviceAvailable ? "Mod: Servis" : "Mod: Doğrudan");

        try
        {
            await CheckVersionsAsync();
        }
        catch (Exception ex)
        {
            LogError($"Otomatik mod versiyon kontrol hatası: {ex.Message}");
            _fileLog.Error("Otomatik mod versiyon kontrol hatası", ex);
            ShowTrayBalloon("Kontrol Hatası", "Versiyon kontrolü başarısız.", ToolTipIcon.Error);
        }

        bool updateNeeded = _serviceAvailable
            ? _lblStatus.Text.Contains("mevcut", StringComparison.OrdinalIgnoreCase)
               || _lblStatus.Text.Contains("gerekli", StringComparison.OrdinalIgnoreCase)
            : _versionService.IsUpdateRequired(_config);

        if (!updateNeeded)
        {
            LogSuccess("Terminal güncel. Mikro başlatılıyor...");

            try
            {
                if (File.Exists(_config.LocalExePath))
                {
                    _updateService.LaunchMikro(_config.LocalExePath);
                }
                else
                {
                    LogWarning("Mikro EXE bulunamadı: " + _config.LocalExePath);
                    _fileLog.Warning($"Otomatik mod — Mikro EXE bulunamadı: {_config.LocalExePath}");
                }
            }
            catch (Exception ex)
            {
                LogError($"Otomatik mod Mikro başlatma hatası: {ex.Message}");
                _fileLog.Error("Otomatik mod Mikro başlatma hatası", ex);
            }

            await Task.Delay(2000);
            _forceExit = true;
            Close();

            return;
        }

        LogWarning("Güncelleme gerekli, kurulum başlatılıyor...");
        SetStatus("Güncelleme yapılıyor...", Color.Orange);

        try
        {
            using CancellationTokenSource cts = new();
            await RunUpdateAsync(cts.Token);
        }
        catch (Exception ex)
        {
            LogError($"Otomatik mod güncelleme hatası: {ex.Message}");
            _fileLog.Error("Otomatik mod güncelleme hatası", ex);
            ShowTrayBalloon("Güncelleme Hatası", "Otomatik güncelleme başarısız.", ToolTipIcon.Error);
        }

        LogInfo("═══ Otomatik mod tamamlandı ═══");
        _fileLog.Info("═══ Otomatik mod tamamlandı ═══");
        await Task.Delay(3000);
        _forceExit = true;
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
        _tsmCheck.Enabled = !busy;
        _tsmUpdate.Enabled = !busy;
        _tsmSettings.Enabled = !busy;

        if (!busy)
        {
            _prgProgress.Value = 0;
            _prgProgress.Style = ProgressBarStyle.Blocks;
        }
    }

    private void LogInfo(string message) => AppendLog(message, Color.White, LogLevel.INFO);
    private void LogSuccess(string message) => AppendLog(message, Color.LimeGreen, LogLevel.OK);
    private void LogWarning(string message) => AppendLog(message, Color.Yellow, LogLevel.WARN);
    private void LogError(string message) => AppendLog(message, Color.OrangeRed, LogLevel.ERROR);

    private void AppendLog(string message, Color color, LogLevel level)
    {
        // Dosya log'u her zaman yaz (handle durumu ne olursa olsun)
        _fileLog.Write(level, message);

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

    /// <summary>
    /// Windows bildirim alanında toast bildirimi gösterir.
    /// Durum değişiklikleri, güncelleme uyarıları ve hatalar için kullanılır.
    /// </summary>
    private void ShowTrayBalloon(string title, string text, ToolTipIcon icon)
    {
        _notifyIcon.ShowBalloonTip(3000, title, text, icon);
    }

    #endregion
}

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
            _config.EnsureModules();
            _lblConfigInfo.Text = $"{_config.MajorVersion} {_config.ProductName}  •  {_config.Modules.Count} modül";
            LogInfo($"Ayarlar yüklendi: {_config.MajorVersion} {_config.ProductName} | {_config.Modules.Count} modül");
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
                _config.EnsureModules();
                _configService.Save(_config);
                _lblConfigInfo.Text = $"{_config.MajorVersion} {_config.ProductName}  •  {_config.Modules.Count} modül";
                LogSuccess($"Ayarlar kaydedildi: {_config.MajorVersion} {_config.ProductName} | {_config.Modules.Count} modül");

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

                // Yeni ayarlarla otomatik versiyon kontrolü başlat
                LogInfo("Ayarlar değişti — versiyon kontrolü başlatılıyor...");
                await CheckVersionsAsync();
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

    private void BtnAbout_Click(object? sender, EventArgs e)
    {
        using AboutForm aboutForm = new();
        aboutForm.ShowDialog(this);
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

        if (!response.Success)
        {
            SetStatus("Hata", Color.OrangeRed);
            LogError(response.Message ?? "Versiyon kontrol hatası.");
            ShowTrayBalloon("Kontrol Hatası", response.Message ?? "Versiyon kontrol hatası.", ToolTipIcon.Error);

            return;
        }

        // Modül versiyon bilgilerini göster
        DisplayModuleVersions(response.ModuleVersions);

        if (response.UpdateRequired)
        {
            int count = response.ModuleVersions.Count(v => v.UpdateRequired);
            SetStatus($"{count} modülde güncelleme mevcut!", Color.Red);
            LogWarning($"{count} modülde güncelleme gerekli.");
            ShowTrayBalloon("Güncelleme Mevcut", $"{count} modülde güncelleme mevcut.", ToolTipIcon.Warning);
        }
        else if (response.ModuleVersions.Exists(v => v.ServerVersion is null))
        {
            SetStatus("Sunucu erişilemiyor", Color.Orange);
            LogWarning("Bazı modüllerin sunucu versiyonuna erişilemedi.");
            ShowTrayBalloon("Bağlantı Sorunu", "Sunucu versiyonları okunamadı.", ToolTipIcon.Warning);
        }
        else
        {
            SetStatus("Güncel", Color.LimeGreen);
            LogSuccess("Tüm modüller güncel.");
        }
    }

    private void CheckVersionsDirect()
    {
        List<ModuleVersionInfo> moduleVersions = [];

        foreach (UpdateModule module in _config.EnabledModules)
        {
            string localPath = Path.Combine(_config.LocalInstallPath, module.ExeFileName);
            string serverPath = Path.Combine(_config.ServerSharePath, module.ExeFileName);
            Version? localVersion = null;
            Version? serverVersion = null;

            try
            {
                localVersion = _versionService.GetVersion(localPath);
            }
            catch (Exception ex)
            {
                LogError($"[{module.Name}] Yerel versiyon okunamadı: {ex.Message}");
                _fileLog.Error($"Yerel versiyon okuma hatası: {localPath}", ex);
            }

            try
            {
                serverVersion = _versionService.GetVersion(serverPath);
            }
            catch (Exception ex)
            {
                LogError($"[{module.Name}] Sunucu versiyonu okunamadı: {ex.Message}");
                _fileLog.Error($"Sunucu versiyon okuma hatası: {serverPath}", ex);
            }

            bool updateRequired = serverVersion is not null
                && (localVersion is null || localVersion < serverVersion);

            moduleVersions.Add(new ModuleVersionInfo
            {
                ModuleName = module.Name,
                LocalVersion = localVersion?.ToString(),
                ServerVersion = serverVersion?.ToString(),
                UpdateRequired = updateRequired
            });
        }

        DisplayModuleVersions(moduleVersions);

        bool anyUpdate = moduleVersions.Exists(v => v.UpdateRequired);
        bool anyServerUnavailable = moduleVersions.Exists(v => v.ServerVersion is null);
        bool allLocal = moduleVersions.TrueForAll(v => v.LocalVersion is not null);

        if (anyServerUnavailable && !anyUpdate)
        {
            SetStatus("Sunucu erişilemiyor", Color.Orange);
            LogWarning("Bazı modüllerin sunucu EXE dosyasına erişilemedi.");
            ShowTrayBalloon("Bağlantı Sorunu", "Sunucu EXE dosyalarına erişilemiyor.", ToolTipIcon.Warning);
        }
        else if (!allLocal && !anyUpdate)
        {
            SetStatus("Kurulum gerekli", Color.Red);
            LogWarning("Bazı modüller yerel kurulumda bulunamadı.");
        }
        else if (anyUpdate)
        {
            int count = moduleVersions.Count(v => v.UpdateRequired);
            SetStatus($"{count} modülde güncelleme mevcut!", Color.Red);
            LogWarning($"{count} modülde güncelleme gerekli.");
            ShowTrayBalloon("Güncelleme Mevcut", $"{count} modülde güncelleme mevcut.", ToolTipIcon.Warning);
        }
        else
        {
            SetStatus("Güncel", Color.LimeGreen);
            LogSuccess("Tüm modüller güncel.");
        }
    }

    /// <summary>
    /// Modül versiyon bilgilerini DataGridView'da ve log'da gösterir.
    /// </summary>
    private void DisplayModuleVersions(List<ModuleVersionInfo> moduleVersions)
    {
        _dgvModules.Rows.Clear();

        foreach (ModuleVersionInfo info in moduleVersions)
        {
            string status = info.UpdateRequired ? "▲ Güncelle"
                : info.ServerVersion is null ? "— Erişilemiyor"
                : "✔ Güncel";

            int rowIndex = _dgvModules.Rows.Add(
                info.ModuleName,
                info.LocalVersion ?? "Kurulu değil",
                info.ServerVersion ?? "Erişilemiyor",
                status);

            DataGridViewRow row = _dgvModules.Rows[rowIndex];
            row.Cells[3].Style.ForeColor = info.UpdateRequired ? Color.Red
                : info.ServerVersion is null ? Color.Orange
                : Color.LimeGreen;

            LogInfo($"  {info.ModuleName}: {info.LocalVersion ?? "-"} → {info.ServerVersion ?? "-"} [{status}]");
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
            if (_config.UpdateMode is UpdateMode.Online or UpdateMode.Hybrid or UpdateMode.AI)
            {
                await RunOnlineUpdateViaServiceAsync(cancellationToken);
            }
            else
            {
                await RunUpdateViaServiceAsync(cancellationToken);
            }
        }
        else
        {
            if (_config.UpdateMode is UpdateMode.Online or UpdateMode.AI)
            {
                LogError("Online güncelleme için MikroUpdate servisi çalışıyor olmalıdır.");
                SetStatus("Servis gerekli", Color.OrangeRed);
                ShowTrayBalloon("Servis Gerekli",
                    "Online güncelleme modunda servis çalışıyor olmalıdır.", ToolTipIcon.Error);

                return;
            }

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

        // Modül versiyon bilgilerini göster
        if (response.ModuleVersions.Count > 0)
        {
            DisplayModuleVersions(response.ModuleVersions);
        }

        if (response.Success)
        {
            _prgProgress.Value = 100;
            SetStatus("Güncelleme tamamlandı", Color.LimeGreen);
            LogSuccess(response.Message ?? "Güncelleme başarıyla tamamlandı.");
            ShowTrayBalloon("Güncelleme Tamamlandı", response.Message ?? "Tüm modüller güncellendi.", ToolTipIcon.Info);

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

    /// <summary>
    /// Online güncelleme — servis üzerinden CDN'den indirir ve kurar.
    /// Pipe progress streaming ile canlı ilerleme bilgisi alır.
    /// </summary>
    private async Task RunOnlineUpdateViaServiceAsync(CancellationToken cancellationToken)
    {
        LogInfo("Online güncelleme başlatılıyor (servis üzerinden)...");
        _fileLog.Info("Online güncelleme başlatılıyor — pipe progress streaming.");
        SetStatus("CDN güncelleme başlatılıyor...", Color.Cyan);
        _prgProgress.Style = ProgressBarStyle.Marquee;

        ServiceResponse? response = await _pipeClient.SendCommandWithProgressAsync(
            CommandType.DownloadUpdate,
            onProgress: progress =>
            {
                if (InvokeRequired)
                {
                    BeginInvoke(() => HandleDownloadProgress(progress));
                }
                else
                {
                    HandleDownloadProgress(progress);
                }
            },
            cancellationToken);

        _prgProgress.Style = ProgressBarStyle.Blocks;

        if (response is null)
        {
            LogWarning("Servis yanıt vermedi, online güncelleme tamamlanamadı.");
            _fileLog.Warning("Online güncelleme pipe yanıt vermedi.");
            SetStatus("Bağlantı hatası", Color.OrangeRed);
            ShowTrayBalloon("Bağlantı Hatası", "Servis ile iletişim kurulamadı.", ToolTipIcon.Error);

            return;
        }

        // Modül versiyon bilgilerini göster
        if (response.ModuleVersions.Count > 0)
        {
            DisplayModuleVersions(response.ModuleVersions);
        }

        if (response.Success)
        {
            _prgProgress.Value = 100;
            SetStatus("Güncelleme tamamlandı", Color.LimeGreen);
            LogSuccess(response.Message ?? "Online güncelleme tamamlandı.");
            ShowTrayBalloon("Güncelleme Tamamlandı",
                response.Message ?? "Tüm modüller CDN'den güncellendi.", ToolTipIcon.Info);

            if (_config.AutoLaunchAfterUpdate)
            {
                LaunchMikro();
            }
        }
        else
        {
            SetStatus("Hata", Color.OrangeRed);
            LogError(response.Message ?? "Online güncelleme sırasında hata oluştu.");
            _fileLog.Error($"Online güncelleme hatası: {response.Message}");
            ShowTrayBalloon("Güncelleme Hatası",
                response.Message ?? "Online güncelleme başarısız.", ToolTipIcon.Error);
        }
    }

    /// <summary>
    /// Pipe üzerinden gelen indirme ilerleme mesajını UI'da gösterir.
    /// ProgressBar ve durum etiketini günceller.
    /// </summary>
    private void HandleDownloadProgress(ServiceResponse progress)
    {
        if (progress.DownloadProgress is { } dp)
        {
            // ProgressBar güncelle
            if (dp.Percentage >= 0)
            {
                _prgProgress.Style = ProgressBarStyle.Blocks;
                _prgProgress.Value = Math.Min(dp.Percentage, 100);
            }
            else
            {
                _prgProgress.Style = ProgressBarStyle.Marquee;
            }

            // Durum etiketini güncelle (her tick'de log spam yapmadan)
            SetStatus(dp.StatusText, Color.Cyan);
        }
        else
        {
            // Metin bazlı durum mesajı — log'a yaz
            LogInfo(progress.Message);
            SetStatus(progress.Message, Color.Cyan);
        }
    }

    private async Task RunUpdateDirectAsync(CancellationToken cancellationToken)
    {
        // 1. Versiyon kontrol
        LogInfo("Versiyon kontrol ediliyor (doğrudan mod)...");
        CheckVersionsDirect();

        // Güncellemesi gereken modülleri belirle
        List<UpdateModule> modulesToUpdate = [];

        foreach (UpdateModule module in _config.EnabledModules)
        {
            string localPath = Path.Combine(_config.LocalInstallPath, module.ExeFileName);
            string serverPath = Path.Combine(_config.ServerSharePath, module.ExeFileName);

            try
            {
                Version? localVer = _versionService.GetVersion(localPath);
                Version? serverVer = _versionService.GetVersion(serverPath);

                if (serverVer is not null && (localVer is null || localVer < serverVer))
                {
                    modulesToUpdate.Add(module);
                }
            }
            catch (Exception ex)
            {
                LogError($"[{module.Name}] Versiyon okuma hatası: {ex.Message}");
                _fileLog.Error($"Doğrudan güncelleme versiyon okuma hatası: {module.Name}", ex);
            }
        }

        if (modulesToUpdate.Count == 0)
        {
            LogSuccess("Tüm modüller güncel, güncelleme gerekmiyor.");

            return;
        }

        LogWarning($"{modulesToUpdate.Count} modül güncellenecek.");

        // 2. İlgili süreçleri kapat
        HashSet<string> killedProcesses = [];

        foreach (UpdateModule module in modulesToUpdate)
        {
            if (killedProcesses.Add(module.ExeFileName))
            {
                try
                {
                    LogInfo($"{module.ExeFileName} süreci kapatılıyor...");
                    int killed = _updateService.KillMikroProcess(module.ExeFileName);
                    LogInfo(killed > 0 ? $"{killed} süreç kapatıldı." : "Çalışan süreç bulunamadı.");
                }
                catch (Exception ex)
                {
                    LogError($"Süreç kapatma hatası: {ex.Message}");
                    _fileLog.Error($"Süreç kapatma hatası: {module.ExeFileName}", ex);
                }
            }
        }

        await Task.Delay(1500, cancellationToken);

        // 3. Her modül için setup kopyala ve kur
        int successCount = 0;
        int failCount = 0;
        int totalModules = modulesToUpdate.Count;

        foreach (UpdateModule module in modulesToUpdate)
        {
            string serverSetupPath = Path.Combine(_config.SetupFilesPath, module.SetupFileName);

            // Setup kopyala
            LogInfo($"[{module.Name}] Setup dosyası sunucuda aranıyor: {serverSetupPath}");
            string? setupPath;

            try
            {
                setupPath = _updateService.CopySetupFromServer(serverSetupPath);
            }
            catch (Exception ex)
            {
                LogError($"[{module.Name}] Setup kopyalama hatası: {ex.Message}");
                _fileLog.Error($"Setup dosyası sunucudan kopyalanamadı: {module.Name}", ex);
                failCount++;

                continue;
            }

            if (string.IsNullOrEmpty(setupPath))
            {
                LogError($"[{module.Name}] Setup dosyası sunucuda bulunamadı: {serverSetupPath}");
                _fileLog.Error($"Setup dosyası bulunamadı: {serverSetupPath}");
                failCount++;

                continue;
            }

            LogSuccess($"[{module.Name}] Setup dosyası kopyalandı.");

            // Sessiz kurulum
            LogInfo($"[{module.Name}] Sessiz kurulum başlatılıyor: {Path.GetFileName(setupPath)}");
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
                LogError($"[{module.Name}] Kurulum çalıştırma hatası: {ex.Message}");
                _fileLog.Error($"Sessiz kurulum hatası: {module.Name}", ex);
                failCount++;

                continue;
            }

            _prgProgress.Style = ProgressBarStyle.Blocks;

            if (exitCode == 0)
            {
                LogSuccess($"[{module.Name}] Kurulum başarıyla tamamlandı.");
                successCount++;
            }
            else
            {
                LogError($"[{module.Name}] Kurulum hata kodu: {exitCode}");
                _fileLog.Error($"{module.Name} sessiz kurulum hata kodu: {exitCode}");
                failCount++;
            }

            // İlerleme
            _prgProgress.Value = (int)((successCount + failCount) * 100.0 / totalModules);
        }

        _prgProgress.Value = 100;

        // 4. Sonuç
        if (failCount == 0)
        {
            SetStatus("Güncelleme tamamlandı", Color.LimeGreen);
            LogSuccess($"{successCount} modül başarıyla güncellendi.");
            ShowTrayBalloon("Güncelleme Tamamlandı", $"{successCount} modül güncellendi.", ToolTipIcon.Info);
        }
        else
        {
            SetStatus("Kısmi güncelleme", Color.OrangeRed);
            LogError($"{successCount} başarılı, {failCount} başarısız modül kurulumu.");
            ShowTrayBalloon("Güncelleme Kısmi", $"{failCount} modül kurulamadı.", ToolTipIcon.Warning);
        }

        // 5. Kurulum sonrası versiyon kontrol
        try
        {
            CheckVersionsDirect();
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
        if (_config.AutoLaunchAfterUpdate && failCount == 0)
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

        LogInfo($"Ürün: {_config.MajorVersion} {_config.ProductName} | Modül: {_config.Modules.Count}");
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
        _btnAbout.Enabled = !busy;
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

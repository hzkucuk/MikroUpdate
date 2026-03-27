using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.ServiceProcess;

using MikroUpdate.Shared.Messages;
using MikroUpdate.Shared.Models;
using MikroUpdate.Win.Controls;
using MikroUpdate.Win.Services;

namespace MikroUpdate.Win;

public partial class Form1 : Form
{
    private readonly ConfigService _configService = new();
    private readonly UpdateService _updateService = new();
    private readonly PipeClient _pipeClient = new();
    private readonly FileLogService _fileLog = new();
    private readonly SelfUpdateService _selfUpdateService = new();
    private readonly bool _autoMode;
    private UpdateConfig _config = new();
    private CancellationTokenSource? _cts;
    private bool _forceExit;
    private bool _selfUpdateInProgress;
    private bool _serviceAvailable;

    private DownloadProgressPanel _downloadPanel;
    private readonly TrayIconManager _trayIconManager;

    public Form1(bool autoMode = false)
    {
        InitializeComponent();
        _autoMode = autoMode;
        _pipeClient.OnError = message => _fileLog.Warning(message);

        string version = GetAppVersion();
        Text = $"MikroUpdate v{version}";
        _notifyIcon.Text = $"MikroUpdate v{version}";
        _ctxTray.Renderer = new VersionSidebarRenderer($"MikroUpdate v{version}");

        _trayIconManager = new TrayIconManager(_notifyIcon, Icon ?? SystemIcons.Application);
        _trayIconManager.ServiceStatusChanged += OnServiceStatusChanged;

        InitializeDownloadUI();
        LoadConfig();
    }

    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);

        try
        {
            _serviceAvailable = await _pipeClient.IsServiceRunningAsync();
            UpdateServiceStatus();
            _trayIconManager.Start();

            if (_serviceAvailable)
            {
                LogInfo("MikroUpdate servisi algılandı — servis modu aktif.");
            }
            else
            {
                LogError("MikroUpdate servisi çalışmıyor! Güncelleme işlemleri için servis gereklidir.");
                LogInfo("Servis durumunu kontrol edin: services.msc → MikroUpdateService");
                SetStatus("Servis gerekli", Color.OrangeRed);
            }

            if (_autoMode)
            {
                await RunAutoModeAsync();
            }
            else
            {
                await CheckVersionsAsync();
            }

            // Arka planda uygulama güncellemesi kontrol et (periyodik)
            _ = StartSelfUpdateLoopAsync();
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
        // Sadece kullanıcı X butonuna tıklarsa tray'e minimize et.
        // Self-update, force exit, Windows shutdown veya Restart Manager kapatırsa izin ver.
        if (e.CloseReason == CloseReason.UserClosing && !_forceExit && !_selfUpdateInProgress)
        {
            e.Cancel = true;
            MinimizeToTray();

            return;
        }

        _cts?.Cancel();
        _cts?.Dispose();
        _trayIconManager.Dispose();
        _selfUpdateService.Dispose();
        _fileLog.Info($"Uygulama kapatılıyor. Sebep: {e.CloseReason}");
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

            string modeLabel = _config.UpdateMode switch
            {
                UpdateMode.Online => "🌐 Online",
                UpdateMode.Hybrid => "🔀 Hybrid",
                _ => "📁 Yerel"
            };

            _lblConfigInfo.Text = $"{_config.MajorVersion} {_config.ProductName}  •  {_config.Modules.Count} modül  •  {modeLabel}";
            LogInfo($"Ayarlar yüklendi: {_config.MajorVersion} {_config.ProductName} | {_config.Modules.Count} modül | {_config.UpdateMode}");
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

    #region Service Control

    private const string ServiceName = "MikroUpdateService";

    /// <summary>
    /// Servis durumunu sorgular ve tray menüsünü günceller.
    /// </summary>
    private void UpdateServiceStatus()
    {
        try
        {
            using ServiceController sc = new(ServiceName);
            sc.Refresh();
            ServiceControllerStatus status = sc.Status;

            _tsmServiceStatus.Text = status switch
            {
                ServiceControllerStatus.Running => "Servis: ✔ Çalışıyor",
                ServiceControllerStatus.Stopped => "Servis: ✖ Durduruldu",
                ServiceControllerStatus.StartPending => "Servis: ⏳ Başlatılıyor...",
                ServiceControllerStatus.StopPending => "Servis: ⏳ Durduruluyor...",
                ServiceControllerStatus.Paused => "Servis: ⏸ Duraklatıldı",
                _ => $"Servis: {status}"
            };

            _tsmServiceStart.Enabled = status == ServiceControllerStatus.Stopped;
            _tsmServiceStop.Enabled = status == ServiceControllerStatus.Running;
            _tsmServiceRestart.Enabled = status == ServiceControllerStatus.Running;
        }
        catch (Exception)
        {
            _tsmServiceStatus.Text = "Servis: ✖ Kurulu değil";
            _tsmServiceStart.Enabled = false;
            _tsmServiceStop.Enabled = false;
            _tsmServiceRestart.Enabled = false;
        }
    }

    /// <summary>
    /// TrayIconManager tarafından servis durumu değiştiğinde çağrılır.
    /// </summary>
    private void OnServiceStatusChanged(bool isRunning)
    {
        _serviceAvailable = isRunning;
        UpdateServiceStatus();

        if (isRunning)
        {
            LogSuccess("Servis tekrar çalışıyor.");
            ShowTrayBalloon("Servis Aktif", "MikroUpdateService çalışıyor.", ToolTipIcon.Info);
        }
        else
        {
            LogError("Servis durdu!");
            SetStatus("Servis gerekli", Color.OrangeRed);
            ShowTrayBalloon("Servis Hatası",
                "MikroUpdateService çalışmıyor. Güncelleme için servis gereklidir.",
                ToolTipIcon.Warning);
        }
    }

    private void CtxTray_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        UpdateServiceStatus();
    }

    private async void TsmServiceStart_Click(object? sender, EventArgs e)
    {
        await RunServiceCommandAsync("start", "Servis başlatılıyor...", "Servis başlatıldı.");
    }

    private async void TsmServiceStop_Click(object? sender, EventArgs e)
    {
        await RunServiceCommandAsync("stop", "Servis durduruluyor...", "Servis durduruldu.");
    }

    private async void TsmServiceRestart_Click(object? sender, EventArgs e)
    {
        try
        {
            LogInfo("Servis yeniden başlatılıyor...");
            await RunScCommandAsync("stop");
            await WaitForServiceStatusAsync(ServiceControllerStatus.Stopped);
            await RunScCommandAsync("start");
            await WaitForServiceStatusAsync(ServiceControllerStatus.Running);

            UpdateServiceStatus();
            _trayIconManager.Refresh();
            LogSuccess("Servis yeniden başlatıldı.");
            ShowTrayBalloon("Servis", "MikroUpdateService yeniden başlatıldı.", ToolTipIcon.Info);

            // Pipe bağlantısını yeniden kontrol et
            _serviceAvailable = await _pipeClient.IsServiceRunningAsync();
        }
        catch (Exception ex)
        {
            UpdateServiceStatus();
            LogError($"Servis yeniden başlatma hatası: {ex.Message}");
            _fileLog.Error("Servis yeniden başlatma hatası", ex);
        }
    }

    /// <summary>
    /// Tekli servis komutu çalıştırır (start/stop).
    /// </summary>
    private async Task RunServiceCommandAsync(string command, string startMessage, string successMessage)
    {
        try
        {
            LogInfo(startMessage);
            await RunScCommandAsync(command);

            // Servisin hedef duruma geçmesini bekle
            await WaitForServiceStatusAsync(
                command == "stop" ? ServiceControllerStatus.Stopped : ServiceControllerStatus.Running);

            UpdateServiceStatus();
            _trayIconManager.Refresh();
            LogSuccess(successMessage);
            ShowTrayBalloon("Servis", successMessage, ToolTipIcon.Info);

            // Pipe bağlantısını yeniden kontrol et
            if (command == "start")
            {
                _serviceAvailable = await _pipeClient.IsServiceRunningAsync();
            }
            else if (command == "stop")
            {
                _serviceAvailable = false;
            }
        }
        catch (Exception ex)
        {
            UpdateServiceStatus();
            LogError($"Servis komutu hatası ({command}): {ex.Message}");
            _fileLog.Error($"Servis {command} hatası", ex);
        }
    }

    private static async Task RunScCommandAsync(string command)
    {
        using Process process = new()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "sc.exe",
                Arguments = $"{command} {ServiceName}",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };

        process.Start();
        string output = await process.StandardOutput.ReadToEndAsync().ConfigureAwait(true);
        string error = await process.StandardError.ReadToEndAsync().ConfigureAwait(true);
        await process.WaitForExitAsync().ConfigureAwait(true);

        if (process.ExitCode != 0)
        {
            string details = string.IsNullOrWhiteSpace(error) ? output.Trim() : $"{output.Trim()} | {error.Trim()}";
            throw new InvalidOperationException(
                $"sc.exe {command} başarısız (çıkış kodu: {process.ExitCode}): {details}");
        }
    }

    /// <summary>
    /// Servisin belirtilen duruma geçmesini bekler (UI thread'i bloklamadan).
    /// </summary>
    private static async Task WaitForServiceStatusAsync(ServiceControllerStatus targetStatus)
    {
        await Task.Run(() =>
        {
            try
            {
                using ServiceController sc = new(ServiceName);
                sc.WaitForStatus(targetStatus, TimeSpan.FromSeconds(10));
            }
            catch (System.ServiceProcess.TimeoutException)
            {
                // Zaman aşımı — menü bir sonraki açılışta güncel durumu gösterecek
            }
        }).ConfigureAwait(true);
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

    private void BtnAbout_Click(object? sender, EventArgs e)
    {
        using AboutForm aboutForm = new();
        aboutForm.ShowDialog(this);
    }

    #endregion

    #region Version Check

    /// <summary>
    /// Versiyon kontrolü — servis üzerinden pipe ile yapılır.
    /// </summary>
    private async Task CheckVersionsAsync(CancellationToken cancellationToken = default)
    {
        // Servis durumunu yeniden kontrol et (başlangıçta hazır olmamış olabilir)
        _serviceAvailable = await _pipeClient.IsServiceRunningAsync(cancellationToken);

        if (!_serviceAvailable)
        {
            LogError("Servis çalışmıyor — versiyon kontrolü yapılamıyor.");
            SetStatus("Servis gerekli", Color.OrangeRed);

            return;
        }

        await CheckVersionsViaServiceAsync(cancellationToken);
    }

    private async Task CheckVersionsViaServiceAsync(CancellationToken cancellationToken)
    {
        LogInfo("Servis üzerinden versiyon kontrol ediliyor...");

        ServiceResponse? response = await _pipeClient.SendCommandAsync(
            CommandType.CheckVersion, cancellationToken).ConfigureAwait(true);

        if (response is null)
        {
            LogError("Servis yanıt vermedi — versiyon kontrolü başarısız.");
            _fileLog.Warning("Servis pipe yanıt vermedi.");
            SetStatus("Bağlantı hatası", Color.OrangeRed);
            ShowTrayBalloon("Bağlantı Hatası", "Servis ile iletişim kurulamadı.", ToolTipIcon.Error);

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

    /// <summary>
    /// Modül versiyon bilgilerini DataGridView'da ve log'da gösterir.
    /// Kaynak türü, tooltip'ler ve durum renkleri ile birlikte.
    /// </summary>
    private void DisplayModuleVersions(List<ModuleVersionInfo> moduleVersions)
    {
        _dgvModules.Rows.Clear();

        foreach (ModuleVersionInfo info in moduleVersions)
        {
            string status = info.UpdateRequired ? "▲ Güncelle"
                : info.ServerVersion is null ? "— Erişilemiyor"
                : "✔ Güncel";

            string sourceLabel = info.SourceType switch
            {
                "CDN" => "🌐 CDN",
                "Yerel" => "📁 Yerel",
                _ => "—"
            };

            int rowIndex = _dgvModules.Rows.Add(
                info.ModuleName,
                info.LocalVersion ?? "Kurulu değil",
                info.ServerVersion ?? "Erişilemiyor",
                sourceLabel,
                status);

            DataGridViewRow row = _dgvModules.Rows[rowIndex];

            // Durum rengi
            row.Cells[4].Style.ForeColor = info.UpdateRequired ? Color.Red
                : info.ServerVersion is null ? Color.Orange
                : Color.LimeGreen;

            // Kaynak rengi
            row.Cells[3].Style.ForeColor = info.SourceType switch
            {
                "CDN" => Color.FromArgb(100, 180, 255),
                "Yerel" => Color.FromArgb(180, 180, 140),
                _ => Color.Gray
            };

            // Tooltip: terminal yolu
            string localPath = Path.Combine(_config.LocalInstallPath, 
                _config.EnabledModules.FirstOrDefault(m => m.Name == info.ModuleName)?.ExeFileName ?? "");
            row.Cells[1].ToolTipText = localPath;

            // Tooltip: sunucu yolu
            if (!string.IsNullOrEmpty(info.ServerPath))
            {
                row.Cells[2].ToolTipText = info.ServerPath;
                row.Cells[3].ToolTipText = info.SourceType switch
                {
                    "CDN" => $"HTTP üzerinden CDN'den kontrol ediliyor\n{info.ServerPath}",
                    "Yerel" => $"UNC paylaşım yolundan kontrol ediliyor\n{info.ServerPath}",
                    _ => ""
                };
            }

            LogInfo($"  {info.ModuleName}: {info.LocalVersion ?? "-"} → {info.ServerVersion ?? "-"} [{status}] ({info.SourceType})");
        }
    }

    private void SetStatus(string text, Color color)
    {
        _lblStatus.Text = text;
        _lblStatus.ForeColor = color;
        _notifyIcon.Text = $"MikroUpdate v{GetAppVersion()} — {text}";
    }

    #endregion

    #region Update Workflow

    private async Task RunUpdateAsync(CancellationToken cancellationToken)
    {
        // Servis durumunu yeniden kontrol et (başlangıçta hazır olmamış olabilir)
        _serviceAvailable = await _pipeClient.IsServiceRunningAsync(cancellationToken);

        if (!_serviceAvailable)
        {
            LogError("Güncelleme için MikroUpdate servisi çalışıyor olmalıdır.");
            SetStatus("Servis gerekli", Color.OrangeRed);
            ShowTrayBalloon("Servis Gerekli",
                "Güncelleme için servis çalışıyor olmalıdır.", ToolTipIcon.Error);

            return;
        }

        if (_config.UpdateMode is UpdateMode.Online or UpdateMode.Hybrid)
        {
            await RunOnlineUpdateViaServiceAsync(cancellationToken);
        }
        else
        {
            await RunUpdateViaServiceAsync(cancellationToken);
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
            LogError("Servis yanıt vermedi — güncelleme başarısız.");
            _fileLog.Warning("Güncelleme sırasında servis pipe yanıt vermedi.");
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
            LogSuccess(response.Message ?? "Güncelleme başarıyla tamamlandı.");
            ShowTrayBalloon("Güncelleme Tamamlandı", response.Message ?? "Tüm modüller güncellendi.", ToolTipIcon.Info);

            if (_config.AutoLaunchAfterUpdate)
            {
                LaunchMikroExe();
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
        ShowDownloadPanel();

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

        HideDownloadPanel();

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
                LaunchMikroExe();
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
    /// Download panelini ve ProgressBar'ı günceller.
    /// </summary>
    private void HandleDownloadProgress(ServiceResponse progress)
    {
        if (progress.DownloadProgress is { } dp)
        {
            UpdateDownloadPanel(dp);
        }
        else
        {
            // Metin bazlı durum mesajı — log'a yaz
            LogInfo(progress.Message);
            SetStatus(progress.Message, Color.Cyan);
        }
    }

    /// <summary>
    /// Konfigürasyondaki ana modül EXE'sini başlatır (güncelleme sonrası otomatik başlatma).
    /// </summary>
    private void LaunchMikroExe()
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
    /// Servis üzerinden pipe ile otomatik kontrol ve güncelleme.
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
        LogInfo(_serviceAvailable ? "Mod: Servis" : "Servis çalışmıyor!");

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

        bool updateNeeded = _lblStatus.Text.Contains("mevcut", StringComparison.OrdinalIgnoreCase)
            || _lblStatus.Text.Contains("gerekli", StringComparison.OrdinalIgnoreCase);

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

    #region Download Progress Panel

    /// <summary>
    /// İndirme ilerleme panelini oluşturur ve TLP row 2'ye yerleştirir.
    /// </summary>
    private void InitializeDownloadUI()
    {
        _downloadPanel = new DownloadProgressPanel
        {
            Anchor = AnchorStyles.Left | AnchorStyles.Right,
            Margin = new Padding(0, 2, 0, 6),
            Visible = false
        };

        _tlpMain.Controls.Add(_downloadPanel, 0, 2);
    }

    /// <summary>
    /// İndirme panelini gösterir — progress bar gizlenir, custom panel açılır.
    /// </summary>
    private void ShowDownloadPanel()
    {
        _prgProgress.Visible = false;
        _downloadPanel.Reset();
        _downloadPanel.Visible = true;
        SetStatus("İndiriliyor...", Color.Cyan);
    }

    /// <summary>
    /// İndirme panelini gizler — ince progress bar'a döner.
    /// </summary>
    private void HideDownloadPanel()
    {
        _downloadPanel.Visible = false;
        _prgProgress.Visible = true;
        _prgProgress.Value = 0;
        _prgProgress.Style = ProgressBarStyle.Blocks;
    }

    /// <summary>
    /// İndirme ilerleme bilgilerini panelde günceller.
    /// </summary>
    private void UpdateDownloadPanel(DownloadProgressInfo dp)
    {
        _downloadPanel.ModuleName = dp.ModuleName;
        _downloadPanel.BytesReceived = dp.BytesReceived;
        _downloadPanel.TotalBytes = dp.TotalBytes;
        _downloadPanel.Percentage = dp.Percentage;
        _downloadPanel.SpeedBps = dp.SpeedBytesPerSecond;
        _downloadPanel.Invalidate();
    }

    #endregion

    #region UI Helpers

    private void SetUIBusy(bool busy)
    {
        _btnCheck.Enabled = !busy;
        _btnUpdate.Enabled = !busy;
        _btnSettings.Enabled = !busy;
        _btnAbout.Enabled = !busy;
        _tsmCheck.Enabled = !busy;
        _tsmUpdate.Enabled = !busy;
        _tsmSettings.Enabled = !busy;
        _tsmSelfUpdate.Enabled = !busy;

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

    #region Version Info

    /// <summary>
    /// Assembly'den uygulama sürüm bilgisini okur.
    /// </summary>
    private static string GetAppVersion()
    {
        string? ver = typeof(Form1).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;

        if (string.IsNullOrWhiteSpace(ver))
        {
            return typeof(Form1).Assembly.GetName().Version?.ToString() ?? "?";
        }

        // +commitHash kısmını kes
        int plus = ver.IndexOf('+');
        return plus > 0 ? ver[..plus] : ver;
    }

    #endregion

    #region Self-Update

    /// <summary>
    /// Periyodik self-update kontrolünü başlatır.
    /// İlk kontrol hemen, sonraki kontroller CheckIntervalMinutes aralığında yapılır.
    /// </summary>
    private async Task StartSelfUpdateLoopAsync()
    {
        // İlk kontrol
        await CheckAndApplySelfUpdateAsync();

        // Auto mode'da form kapanacağı için periyodik kontrole gerek yok
        if (_autoMode)
        {
            return;
        }

        // Periyodik kontrol
        int intervalMinutes = Math.Max(_config.CheckIntervalMinutes, 5);

        while (!(_cts?.Token.IsCancellationRequested ?? true))
        {
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), _cts!.Token);
                await CheckAndApplySelfUpdateAsync();
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    /// <summary>
    /// GitHub'dan uygulama güncellemesi kontrol eder.
    /// AutoSelfUpdate aktifse otomatik indirir ve kurar.
    /// Değilse sadece tray bildirimi gösterir.
    /// </summary>
    private async Task CheckAndApplySelfUpdateAsync()
    {
        try
        {
            ReleaseInfo? release = await _selfUpdateService.CheckForUpdateAsync();

            if (release is null)
            {
                LogSuccess("MikroUpdate zaten güncel.");

                return;
            }

            LogInfo($"Yeni MikroUpdate sürümü mevcut: v{release.LatestVersion} (mevcut: v{release.CurrentVersion})");
            _fileLog.Info($"Yeni uygulama sürümü: v{release.LatestVersion}");

            if (!_config.AutoSelfUpdate)
            {
                // Sadece bildirim göster — kullanıcı menüden güncelleyecek
                ShowTrayBalloon(
                    "MikroUpdate Güncellemesi",
                    $"Yeni sürüm v{release.LatestVersion} mevcut. Menüden güncelleyebilirsiniz.",
                    ToolTipIcon.Info);

                return;
            }

            // Otomatik güncelleme — kullanıcıya sormadan indir ve kur
            await DownloadAndInstallSelfUpdateAsync(release);
        }
        catch (Exception ex)
        {
            _fileLog.Warning($"Uygulama güncelleme kontrolü başarısız: {ex.Message}");
        }
    }

    /// <summary>
    /// Güncellemeyi indirip sessiz kurulumu başlatır (kullanıcıya sormadan).
    /// BtnSelfUpdate_Click ile ortak mantık kullanır.
    /// </summary>
    private async Task DownloadAndInstallSelfUpdateAsync(ReleaseInfo release)
    {
        LogInfo($"Güncelleme indiriliyor: v{release.LatestVersion}...");
        ShowDownloadPanel();
        _downloadPanel.ModuleName = "MikroUpdate";
        _downloadPanel.Invalidate();

        Progress<int> progress = new(percent =>
        {
            _downloadPanel.Percentage = Math.Clamp(percent, 0, 100);
            _downloadPanel.Invalidate();
        });

        string installerPath = await _selfUpdateService.DownloadInstallerAsync(release, progress);

        HideDownloadPanel();
        LogSuccess($"İndirme tamamlandı: {Path.GetFileName(installerPath)}");
        LogInfo("Installer başlatılıyor...");

        _fileLog.Info($"Self-update installer başlatılıyor: {installerPath}");

        _serviceAvailable = await _pipeClient.IsServiceRunningAsync();
        _fileLog.Info($"Self-update öncesi servis durumu: {(_serviceAvailable ? "aktif" : "pasif")}");

        _selfUpdateInProgress = true;

        if (_serviceAvailable)
        {
            LogInfo("Servis üzerinden sessiz kurulum başlatılıyor (UAC'sız)...");

            using CancellationTokenSource pipeCts = new(TimeSpan.FromSeconds(15));
            ServiceResponse? response = await _pipeClient.SendCommandAsync(
                CommandType.InstallSelfUpdate, installerPath, pipeCts.Token);

            if (response is not null && response.Success)
            {
                _fileLog.Info("Self-update servise devredildi, uygulama kapatılıyor.");
                Application.Exit();

                return;
            }

            _selfUpdateInProgress = false;
            string errorMsg = response?.Message ?? "Servis yanıt vermedi.";
            _fileLog.Warning($"Servis üzerinden self-update başarısız: {errorMsg}");
            LogError($"Self-update başarısız: {errorMsg}");
            ShowTrayBalloon("Self-Update Hatası",
                $"Servis üzerinden güncelleme yapılamadı: {errorMsg}\nLütfen tekrar deneyin.",
                ToolTipIcon.Error);
        }
        else
        {
            LogInfo("Servis mevcut değil, doğrudan kurulum başlatılıyor (UAC gerekli)...");
            SelfUpdateService.LaunchInstaller(installerPath);
        }
    }

    /// <summary>
    /// Yeni sürümü kontrol eder, kullanıcıya sorar ve kurulumu başlatır.
    /// </summary>
    private async void BtnSelfUpdate_Click(object? sender, EventArgs e)
    {
        try
        {
            LogInfo("Uygulama güncellemesi kontrol ediliyor...");
            SetUIBusy(true);

            ReleaseInfo? release = await _selfUpdateService.CheckForUpdateAsync();

            if (release is null)
            {
                LogSuccess("MikroUpdate zaten güncel.");
                ShowTrayBalloon("MikroUpdate", "Uygulama zaten güncel sürümde.", ToolTipIcon.Info);

                return;
            }

            DialogResult result = MessageBox.Show(
                this,
                $"Yeni MikroUpdate sürümü mevcut!\n\n" +
                $"Mevcut: v{release.CurrentVersion}\n" +
                $"Yeni: v{release.LatestVersion}\n\n" +
                $"Güncelleme indirilip kurulsun mu?\n" +
                $"(Uygulama kapatılıp yeniden başlatılacaktır)",
                "MikroUpdate Güncellemesi",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
            {
                return;
            }

            await DownloadAndInstallSelfUpdateAsync(release);
        }
        catch (Exception ex)
        {
            LogError($"Güncelleme hatası: {ex.Message}");
            _fileLog.Error("Self-update hatası", ex);
            ShowTrayBalloon("Güncelleme Hatası", ex.Message, ToolTipIcon.Error);
        }
        finally
        {
            SetUIBusy(false);
        }
    }

    #endregion
}

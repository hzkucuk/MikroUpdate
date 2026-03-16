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
    private readonly bool _autoMode;
    private UpdateConfig _config = new();
    private CancellationTokenSource? _cts;
    private bool _forceExit;
    private bool _serviceAvailable;

    public Form1(bool autoMode = false)
    {
        InitializeComponent();
        _autoMode = autoMode;
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
        }
        catch (Exception ex)
        {
            LogError($"Başlatma hatası: {ex.Message}");
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
        }
        catch (Exception ex)
        {
            LogError($"Ayarlar yüklenirken hata: {ex.Message}");
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
                }
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
            await CheckVersionsAsync();
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
        }
        else if (response.UpdateRequired)
        {
            SetStatus("Güncelleme mevcut!", Color.Red);
            LogWarning(response.Message ?? "Güncelleme gerekli.");
        }
        else
        {
            SetStatus("Güncel", Color.LimeGreen);
            LogSuccess(response.Message ?? "Terminal güncel.");
        }
    }

    private void CheckVersionsDirect()
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

            if (_config.AutoLaunchAfterUpdate)
            {
                LaunchMikro();
            }
        }
        else
        {
            SetStatus("Hata", Color.OrangeRed);
            LogError(response.Message ?? "Güncelleme sırasında hata oluştu.");
        }
    }

    private async Task RunUpdateDirectAsync(CancellationToken cancellationToken)
    {

        // 1. Versiyon kontrol
        LogInfo("Versiyon kontrol ediliyor (doğrudan mod)...");
        CheckVersionsDirect();

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

        // 3. Setup dosyasını sunucudan al
        LogInfo("Setup dosyası sunucuda aranıyor: " + _config.ServerSetupFilePath);
        string? setupPath = _updateService.CopySetupFromServer(_config.ServerSetupFilePath);

        if (string.IsNullOrEmpty(setupPath))
        {
            LogError("Setup dosyası sunucuda bulunamadı: " + _config.ServerSetupFilePath);
            LogError("Güncelleme iptal edildi.");

            return;
        }

        LogSuccess("Setup dosyası sunucudan kopyalandı.");

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

        // 5. Kurulum sonrası versiyon kontrol
        Version? newVersion = _versionService.GetVersion(_config.LocalExePath);

        if (newVersion is not null)
        {
            _lblLocalVersion.Text = newVersion.ToString();
            LogSuccess($"Yeni versiyon: {newVersion}");
        }

        // 6. Geçici dosyaları temizle
        UpdateService.CleanupTempFiles();
        LogInfo("Geçici dosyalar temizlendi.");

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

        _config = _configService.Load();

        LogInfo($"Ürün: {_config.ProductName} | EXE: {_config.ExeFileName}");
        LogInfo($"Sunucu: {_config.ServerSharePath}");
        LogInfo($"Terminal: {_config.LocalInstallPath}");
        LogInfo(_serviceAvailable ? "Mod: Servis" : "Mod: Doğrudan");

        await CheckVersionsAsync();

        bool updateNeeded = _serviceAvailable
            ? _lblStatus.Text.Contains("mevcut", StringComparison.OrdinalIgnoreCase)
               || _lblStatus.Text.Contains("gerekli", StringComparison.OrdinalIgnoreCase)
            : _versionService.IsUpdateRequired(_config);

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
            _forceExit = true;
            Close();

            return;
        }

        LogWarning("Güncelleme gerekli, kurulum başlatılıyor...");
        SetStatus("Güncelleme yapılıyor...", Color.Orange);

        using CancellationTokenSource cts = new();
        await RunUpdateAsync(cts.Token);

        LogInfo("═══ Otomatik mod tamamlandı ═══");
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

using System.ComponentModel;

using MikroUpdate.Shared.Models;

namespace MikroUpdate.Win;

/// <summary>
/// Mikro ERP güncelleme ayarları düzenleme formu.
/// Modal dialog olarak açılır; OK ile kaydeder, Cancel ile değişiklikleri iptal eder.
/// V16/V17 ana sürüm ve Jump/Fly ürün seçimine göre modül listesi otomatik güncellenir.
/// </summary>
public partial class SettingsForm : Form
{
    private UpdateConfig _config = new();
    private bool _suppressModuleRefresh;

    /// <summary>
    /// Formda düzenlenen yapılandırma nesnesi.
    /// Dialog açılmadan önce set edilir, OK ile kapatıldığında güncel hali okunur.
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public UpdateConfig Config
    {
        get => ReadConfigFromUI();
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            _config = value;
            ApplyConfigToUI();
        }
    }

    public SettingsForm()
    {
        InitializeComponent();

        _cboMajorVersion.SelectedIndexChanged += OnProductOrVersionChanged;
        _cboProduct.SelectedIndexChanged += OnProductOrVersionChanged;
        _txtServerShare.TextChanged += OnSettingsChanged;
        _txtLocalPath.TextChanged += OnSettingsChanged;
        _txtSetupFilesPath.TextChanged += OnSettingsChanged;
    }

    private void ApplyConfigToUI()
    {
        _suppressModuleRefresh = true;

        _cboMajorVersion.SelectedItem = _config.MajorVersion;
        _cboProduct.SelectedItem = _config.ProductName;
        _txtServerShare.Text = _config.ServerSharePath;
        _txtLocalPath.Text = _config.LocalInstallPath;
        _txtSetupFilesPath.Text = _config.SetupFilesPath;
        _nudCheckInterval.Value = Math.Clamp(_config.CheckIntervalMinutes, 1, 1440);
        _chkAutoLaunch.Checked = _config.AutoLaunchAfterUpdate;

        _suppressModuleRefresh = false;

        RefreshModuleGrid(_config.Modules);
        UpdateComputedPaths();
    }

    private UpdateConfig ReadConfigFromUI()
    {
        return new UpdateConfig
        {
            MajorVersion = _cboMajorVersion.SelectedItem?.ToString() ?? "V16",
            ProductName = _cboProduct.SelectedItem?.ToString() ?? "Jump",
            ServerSharePath = _txtServerShare.Text.Trim(),
            LocalInstallPath = _txtLocalPath.Text.Trim(),
            SetupFilesPath = _txtSetupFilesPath.Text.Trim(),
            CheckIntervalMinutes = (int)_nudCheckInterval.Value,
            AutoLaunchAfterUpdate = _chkAutoLaunch.Checked,
            Modules = ReadModulesFromGrid()
        };
    }

    /// <summary>
    /// Ürün veya ana sürüm değiştiğinde modül listesini varsayılanlarla günceller.
    /// </summary>
    private void OnProductOrVersionChanged(object? sender, EventArgs e)
    {
        if (_suppressModuleRefresh)
        {
            return;
        }

        string product = _cboProduct.SelectedItem?.ToString() ?? "Jump";
        string version = _cboMajorVersion.SelectedItem?.ToString() ?? "V16";
        List<UpdateModule> defaults = UpdateConfig.GetDefaultModules(product, version);

        RefreshModuleGrid(defaults);
        UpdateComputedPaths();
    }

    /// <summary>
    /// Ayar alanları değiştiğinde hesaplanan yolları günceller.
    /// </summary>
    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        UpdateComputedPaths();
    }

    private void UpdateComputedPaths()
    {
        string version = _cboMajorVersion.SelectedItem?.ToString() ?? "V16";
        string product = _cboProduct.SelectedItem?.ToString() ?? "Jump";
        string setupFilesPath = _txtSetupFilesPath.Text.Trim();
        List<UpdateModule> modules = ReadModulesFromGrid();
        int enabledCount = modules.Count(m => m.Enabled);

        _lblComputedPaths.Text =
            $"{version} {product}  •  {modules.Count} modül ({enabledCount} aktif)\n" +
            $"Sunucu: {_txtServerShare.Text.Trim()}\n" +
            $"Terminal: {_txtLocalPath.Text.Trim()}\n" +
            $"Setup: {setupFilesPath}";
    }

    /// <summary>
    /// Modül listesini DataGridView'a yükler.
    /// </summary>
    private void RefreshModuleGrid(List<UpdateModule> modules)
    {
        _dgvModules.Rows.Clear();

        foreach (UpdateModule module in modules)
        {
            _dgvModules.Rows.Add(
                module.Enabled,
                module.Name,
                module.SetupFileName,
                module.ExeFileName);
        }
    }

    /// <summary>
    /// DataGridView'dan modül listesini okur.
    /// </summary>
    private List<UpdateModule> ReadModulesFromGrid()
    {
        List<UpdateModule> modules = [];

        foreach (DataGridViewRow row in _dgvModules.Rows)
        {
            if (row.IsNewRow)
            {
                continue;
            }

            modules.Add(new UpdateModule
            {
                Enabled = row.Cells[0].Value is true,
                Name = row.Cells[1].Value?.ToString() ?? string.Empty,
                SetupFileName = row.Cells[2].Value?.ToString() ?? string.Empty,
                ExeFileName = row.Cells[3].Value?.ToString() ?? string.Empty
            });
        }

        return modules;
    }

    private void BtnBrowseServer_Click(object? sender, EventArgs e)
    {
        using FolderBrowserDialog dialog = new()
        {
            Description = "Sunucu paylaşım klasörünü seçin",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = false
        };

        if (!string.IsNullOrWhiteSpace(_txtServerShare.Text) && Directory.Exists(_txtServerShare.Text))
        {
            dialog.InitialDirectory = _txtServerShare.Text;
        }

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _txtServerShare.Text = dialog.SelectedPath;
        }
    }

    private void BtnBrowseLocal_Click(object? sender, EventArgs e)
    {
        using FolderBrowserDialog dialog = new()
        {
            Description = "Terminal kurulum klasörünü seçin",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = true
        };

        if (!string.IsNullOrWhiteSpace(_txtLocalPath.Text) && Directory.Exists(_txtLocalPath.Text))
        {
            dialog.InitialDirectory = _txtLocalPath.Text;
        }

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _txtLocalPath.Text = dialog.SelectedPath;
        }
    }

    private void BtnBrowseSetup_Click(object? sender, EventArgs e)
    {
        using FolderBrowserDialog dialog = new()
        {
            Description = "Setup dosyalarının bulunduğu klasörü seçin",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = false
        };

        if (!string.IsNullOrWhiteSpace(_txtSetupFilesPath.Text) && Directory.Exists(_txtSetupFilesPath.Text))
        {
            dialog.InitialDirectory = _txtSetupFilesPath.Text;
        }

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _txtSetupFilesPath.Text = dialog.SelectedPath;
        }
    }

    /// <summary>
    /// Modülleri seçilen ürün ve sürüme göre varsayılanlara sıfırlar.
    /// </summary>
    private void BtnResetModules_Click(object? sender, EventArgs e)
    {
        string product = _cboProduct.SelectedItem?.ToString() ?? "Jump";
        string version = _cboMajorVersion.SelectedItem?.ToString() ?? "V16";
        List<UpdateModule> defaults = UpdateConfig.GetDefaultModules(product, version);

        RefreshModuleGrid(defaults);
        UpdateComputedPaths();
    }
}

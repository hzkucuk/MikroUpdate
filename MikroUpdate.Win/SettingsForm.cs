using System.ComponentModel;

using MikroUpdate.Win.Models;

namespace MikroUpdate.Win;

/// <summary>
/// Mikro ERP güncelleme ayarları düzenleme formu.
/// Modal dialog olarak açılır; OK ile kaydeder, Cancel ile değişiklikleri iptal eder.
/// </summary>
public partial class SettingsForm : Form
{
    private UpdateConfig _config = new();

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

        _cboProduct.SelectedIndexChanged += OnSettingsChanged;
        _txtServerShare.TextChanged += OnSettingsChanged;
        _txtLocalPath.TextChanged += OnSettingsChanged;
        _txtSetupFile.TextChanged += OnSettingsChanged;
        _txtCdnUrl.TextChanged += OnSettingsChanged;
        _txtEDefterFile.TextChanged += OnSettingsChanged;
    }

    private void ApplyConfigToUI()
    {
        _cboProduct.SelectedItem = _config.ProductName;
        _txtServerShare.Text = _config.ServerSharePath;
        _txtLocalPath.Text = _config.LocalInstallPath;
        _txtSetupFile.Text = _config.SetupFileName;
        _txtCdnUrl.Text = _config.CdnBaseUrl;
        _txtEDefterFile.Text = _config.EDefterSetupFileName;
        _chkAutoLaunch.Checked = _config.AutoLaunchAfterUpdate;
        _chkEDefter.Checked = _config.IncludeEDefter;

        UpdateComputedPaths();
    }

    private UpdateConfig ReadConfigFromUI()
    {
        return new UpdateConfig
        {
            ProductName = _cboProduct.SelectedItem?.ToString() ?? "Jump",
            ServerSharePath = _txtServerShare.Text.Trim(),
            LocalInstallPath = _txtLocalPath.Text.Trim(),
            SetupFileName = _txtSetupFile.Text.Trim(),
            CdnBaseUrl = _txtCdnUrl.Text.Trim(),
            EDefterSetupFileName = _txtEDefterFile.Text.Trim(),
            AutoLaunchAfterUpdate = _chkAutoLaunch.Checked,
            IncludeEDefter = _chkEDefter.Checked
        };
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
        UpdateConfig current = ReadConfigFromUI();

        _lblExeFileValue.Text = current.ExeFileName;
        _lblServerExeValue.Text = current.ServerExePath;
        _lblLocalExeValue.Text = current.LocalExePath;
        _lblSetupPathValue.Text = current.ServerSetupFilePath;
        _lblCdnSetupValue.Text = current.CdnSetupUrl;
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
}

namespace MikroUpdate.Win;

partial class SettingsForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        // 1. Instantiate
        components = new System.ComponentModel.Container();
        _tlpMain = new TableLayoutPanel();
        _lblProduct = new Label();
        _cboProduct = new ComboBox();
        _lblServerShare = new Label();
        _txtServerShare = new TextBox();
        _btnBrowseServer = new Button();
        _lblLocalPath = new Label();
        _txtLocalPath = new TextBox();
        _btnBrowseLocal = new Button();
        _lblSetupFile = new Label();
        _txtSetupFile = new TextBox();
        _lblSetupFilesPath = new Label();
        _txtSetupFilesPath = new TextBox();
        _btnBrowseSetup = new Button();
        _chkAutoLaunch = new CheckBox();
        _grpComputedPaths = new GroupBox();
        _tlpComputed = new TableLayoutPanel();
        _lblExeFileCaption = new Label();
        _lblExeFileValue = new Label();
        _lblServerExeCaption = new Label();
        _lblServerExeValue = new Label();
        _lblLocalExeCaption = new Label();
        _lblLocalExeValue = new Label();
        _lblSetupPathCaption = new Label();
        _lblSetupPathValue = new Label();
        _flpButtons = new FlowLayoutPanel();
        _btnOK = new Button();
        _btnCancel = new Button();

        // 3. Suspend
        _tlpMain.SuspendLayout();
        _grpComputedPaths.SuspendLayout();
        _tlpComputed.SuspendLayout();
        _flpButtons.SuspendLayout();
        SuspendLayout();

        // 4. Configure controls

        // _tlpMain (8 rows: Product, ServerShare, LocalPath, SetupFile, SetupFilesPath, AutoLaunch, ComputedPaths, Buttons)
        _tlpMain.ColumnCount = 3;
        _tlpMain.ColumnStyles.Add(new ColumnStyle());
        _tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _tlpMain.ColumnStyles.Add(new ColumnStyle());
        _tlpMain.RowCount = 8;
        _tlpMain.RowStyles.Add(new RowStyle());
        _tlpMain.RowStyles.Add(new RowStyle());
        _tlpMain.RowStyles.Add(new RowStyle());
        _tlpMain.RowStyles.Add(new RowStyle());
        _tlpMain.RowStyles.Add(new RowStyle());
        _tlpMain.RowStyles.Add(new RowStyle());
        _tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _tlpMain.RowStyles.Add(new RowStyle());
        _tlpMain.Dock = DockStyle.Fill;
        _tlpMain.Padding = new Padding(12);
        _tlpMain.Name = "_tlpMain";
        _tlpMain.Controls.Add(_lblProduct, 0, 0);
        _tlpMain.Controls.Add(_cboProduct, 1, 0);
        _tlpMain.Controls.Add(_lblServerShare, 0, 1);
        _tlpMain.Controls.Add(_txtServerShare, 1, 1);
        _tlpMain.Controls.Add(_btnBrowseServer, 2, 1);
        _tlpMain.Controls.Add(_lblLocalPath, 0, 2);
        _tlpMain.Controls.Add(_txtLocalPath, 1, 2);
        _tlpMain.Controls.Add(_btnBrowseLocal, 2, 2);
        _tlpMain.Controls.Add(_lblSetupFile, 0, 3);
        _tlpMain.Controls.Add(_txtSetupFile, 1, 3);
        _tlpMain.Controls.Add(_lblSetupFilesPath, 0, 4);
        _tlpMain.Controls.Add(_txtSetupFilesPath, 1, 4);
        _tlpMain.Controls.Add(_btnBrowseSetup, 2, 4);
        _tlpMain.Controls.Add(_chkAutoLaunch, 0, 5);
        _tlpMain.Controls.Add(_grpComputedPaths, 0, 6);
        _tlpMain.Controls.Add(_flpButtons, 0, 7);
        _tlpMain.SetColumnSpan(_cboProduct, 2);
        _tlpMain.SetColumnSpan(_txtSetupFile, 2);
        _tlpMain.SetColumnSpan(_chkAutoLaunch, 3);
        _tlpMain.SetColumnSpan(_grpComputedPaths, 3);
        _tlpMain.SetColumnSpan(_flpButtons, 3);

        // Row 0: Ürün
        _lblProduct.Text = "Ürün:";
        _lblProduct.AutoSize = true;
        _lblProduct.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _lblProduct.Name = "_lblProduct";

        _cboProduct.DropDownStyle = ComboBoxStyle.DropDownList;
        _cboProduct.Items.AddRange(new object[] { "Jump", "Fly" });
        _cboProduct.SelectedIndex = 0;
        _cboProduct.Anchor = AnchorStyles.Left;
        _cboProduct.Name = "_cboProduct";
        _cboProduct.AccessibleName = "Ürün seçimi";
        _cboProduct.TabIndex = 0;

        // Row 1: Sunucu Paylaşım Yolu
        _lblServerShare.Text = "Sunucu Paylaşım Yolu:";
        _lblServerShare.AutoSize = true;
        _lblServerShare.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _lblServerShare.Name = "_lblServerShare";

        _txtServerShare.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _txtServerShare.Name = "_txtServerShare";
        _txtServerShare.AccessibleName = "Sunucu paylaşım yolu";
        _txtServerShare.TabIndex = 1;

        _btnBrowseServer.Text = "...";
        _btnBrowseServer.Size = new Size(30, 23);
        _btnBrowseServer.Name = "_btnBrowseServer";
        _btnBrowseServer.AccessibleName = "Sunucu yolu seç";
        _btnBrowseServer.TabIndex = 2;
        _btnBrowseServer.Click += BtnBrowseServer_Click;

        // Row 2: Terminal Kurulum Yolu
        _lblLocalPath.Text = "Terminal Kurulum Yolu:";
        _lblLocalPath.AutoSize = true;
        _lblLocalPath.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _lblLocalPath.Name = "_lblLocalPath";

        _txtLocalPath.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _txtLocalPath.Name = "_txtLocalPath";
        _txtLocalPath.AccessibleName = "Terminal kurulum yolu";
        _txtLocalPath.TabIndex = 3;

        _btnBrowseLocal.Text = "...";
        _btnBrowseLocal.Size = new Size(30, 23);
        _btnBrowseLocal.Name = "_btnBrowseLocal";
        _btnBrowseLocal.AccessibleName = "Terminal yolu seç";
        _btnBrowseLocal.TabIndex = 4;
        _btnBrowseLocal.Click += BtnBrowseLocal_Click;

        // Row 3: Setup Dosyası
        _lblSetupFile.Text = "Setup Dosyası:";
        _lblSetupFile.AutoSize = true;
        _lblSetupFile.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _lblSetupFile.Name = "_lblSetupFile";

        _txtSetupFile.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _txtSetupFile.Name = "_txtSetupFile";
        _txtSetupFile.AccessibleName = "Setup dosya adı";
        _txtSetupFile.TabIndex = 5;

        // Row 4: Setup Dosyaları Yolu
        _lblSetupFilesPath.Text = "Setup Dosyaları Yolu:";
        _lblSetupFilesPath.AutoSize = true;
        _lblSetupFilesPath.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _lblSetupFilesPath.Name = "_lblSetupFilesPath";

        _txtSetupFilesPath.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _txtSetupFilesPath.Name = "_txtSetupFilesPath";
        _txtSetupFilesPath.AccessibleName = "Setup dosyaları yolu";
        _txtSetupFilesPath.TabIndex = 6;

        _btnBrowseSetup.Text = "...";
        _btnBrowseSetup.Size = new Size(30, 23);
        _btnBrowseSetup.Name = "_btnBrowseSetup";
        _btnBrowseSetup.AccessibleName = "Setup dosyaları yolu seç";
        _btnBrowseSetup.TabIndex = 7;
        _btnBrowseSetup.Click += BtnBrowseSetup_Click;

        // Row 5: Auto Launch Checkbox
        _chkAutoLaunch.Text = "Güncelleme sonrası Mikro'yu otomatik başlat";
        _chkAutoLaunch.AutoSize = true;
        _chkAutoLaunch.Checked = true;
        _chkAutoLaunch.Name = "_chkAutoLaunch";
        _chkAutoLaunch.AccessibleName = "Otomatik başlatma";
        _chkAutoLaunch.TabIndex = 8;

        // Row 6: Computed Paths GroupBox
        _grpComputedPaths.Text = "Hesaplanan Yollar (salt okunur)";
        _grpComputedPaths.AutoSize = true;
        _grpComputedPaths.AutoSizeMode = AutoSizeMode.GrowOnly;
        _grpComputedPaths.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _grpComputedPaths.Padding = new Padding(8);
        _grpComputedPaths.Name = "_grpComputedPaths";
        _grpComputedPaths.Controls.Add(_tlpComputed);

        _tlpComputed.ColumnCount = 2;
        _tlpComputed.ColumnStyles.Add(new ColumnStyle());
        _tlpComputed.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _tlpComputed.RowCount = 4;
        _tlpComputed.RowStyles.Add(new RowStyle());
        _tlpComputed.RowStyles.Add(new RowStyle());
        _tlpComputed.RowStyles.Add(new RowStyle());
        _tlpComputed.RowStyles.Add(new RowStyle());
        _tlpComputed.Dock = DockStyle.Fill;
        _tlpComputed.Name = "_tlpComputed";
        _tlpComputed.Controls.Add(_lblExeFileCaption, 0, 0);
        _tlpComputed.Controls.Add(_lblExeFileValue, 1, 0);
        _tlpComputed.Controls.Add(_lblServerExeCaption, 0, 1);
        _tlpComputed.Controls.Add(_lblServerExeValue, 1, 1);
        _tlpComputed.Controls.Add(_lblLocalExeCaption, 0, 2);
        _tlpComputed.Controls.Add(_lblLocalExeValue, 1, 2);
        _tlpComputed.Controls.Add(_lblSetupPathCaption, 0, 3);
        _tlpComputed.Controls.Add(_lblSetupPathValue, 1, 3);

        _lblExeFileCaption.Text = "EXE Dosyası:";
        _lblExeFileCaption.AutoSize = true;
        _lblExeFileCaption.Font = new Font("Segoe UI", 8.25F);
        _lblExeFileCaption.ForeColor = SystemColors.GrayText;
        _lblExeFileCaption.Name = "_lblExeFileCaption";

        _lblExeFileValue.Text = "---";
        _lblExeFileValue.AutoSize = true;
        _lblExeFileValue.Font = new Font("Segoe UI", 8.25F);
        _lblExeFileValue.ForeColor = SystemColors.GrayText;
        _lblExeFileValue.Name = "_lblExeFileValue";

        _lblServerExeCaption.Text = "Sunucu EXE:";
        _lblServerExeCaption.AutoSize = true;
        _lblServerExeCaption.Font = new Font("Segoe UI", 8.25F);
        _lblServerExeCaption.ForeColor = SystemColors.GrayText;
        _lblServerExeCaption.Name = "_lblServerExeCaption";

        _lblServerExeValue.Text = "---";
        _lblServerExeValue.AutoSize = true;
        _lblServerExeValue.Font = new Font("Segoe UI", 8.25F);
        _lblServerExeValue.ForeColor = SystemColors.GrayText;
        _lblServerExeValue.Name = "_lblServerExeValue";

        _lblLocalExeCaption.Text = "Terminal EXE:";
        _lblLocalExeCaption.AutoSize = true;
        _lblLocalExeCaption.Font = new Font("Segoe UI", 8.25F);
        _lblLocalExeCaption.ForeColor = SystemColors.GrayText;
        _lblLocalExeCaption.Name = "_lblLocalExeCaption";

        _lblLocalExeValue.Text = "---";
        _lblLocalExeValue.AutoSize = true;
        _lblLocalExeValue.Font = new Font("Segoe UI", 8.25F);
        _lblLocalExeValue.ForeColor = SystemColors.GrayText;
        _lblLocalExeValue.Name = "_lblLocalExeValue";

        _lblSetupPathCaption.Text = "Setup Yolu:";
        _lblSetupPathCaption.AutoSize = true;
        _lblSetupPathCaption.Font = new Font("Segoe UI", 8.25F);
        _lblSetupPathCaption.ForeColor = SystemColors.GrayText;
        _lblSetupPathCaption.Name = "_lblSetupPathCaption";

        _lblSetupPathValue.Text = "---";
        _lblSetupPathValue.AutoSize = true;
        _lblSetupPathValue.Font = new Font("Segoe UI", 8.25F);
        _lblSetupPathValue.ForeColor = SystemColors.GrayText;
        _lblSetupPathValue.Name = "_lblSetupPathValue";

        // Row 7: Buttons
        _flpButtons.FlowDirection = FlowDirection.RightToLeft;
        _flpButtons.AutoSize = true;
        _flpButtons.AutoSizeMode = AutoSizeMode.GrowOnly;
        _flpButtons.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _flpButtons.Padding = new Padding(0, 8, 0, 0);
        _flpButtons.Name = "_flpButtons";
        _flpButtons.Controls.Add(_btnCancel);
        _flpButtons.Controls.Add(_btnOK);

        _btnOK.Text = "Kaydet ve Kapat";
        _btnOK.AutoSize = true;
        _btnOK.Padding = new Padding(12, 4, 12, 4);
        _btnOK.DialogResult = DialogResult.OK;
        _btnOK.Name = "_btnOK";
        _btnOK.AccessibleName = "Kaydet ve kapat";
        _btnOK.TabIndex = 9;

        _btnCancel.Text = "İptal";
        _btnCancel.AutoSize = true;
        _btnCancel.Padding = new Padding(12, 4, 12, 4);
        _btnCancel.DialogResult = DialogResult.Cancel;
        _btnCancel.Name = "_btnCancel";
        _btnCancel.AccessibleName = "İptal";
        _btnCancel.TabIndex = 10;

        // 5. Configure Form
        AcceptButton = _btnOK;
        CancelButton = _btnCancel;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(620, 400);
        MinimumSize = new Size(500, 350);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Controls.Add(_tlpMain);
        Name = "SettingsForm";
        Text = "Ayarlar - MikroUpdate Yapılandırma";

        // 6. Resume
        _tlpComputed.ResumeLayout(false);
        _tlpComputed.PerformLayout();
        _grpComputedPaths.ResumeLayout(false);
        _grpComputedPaths.PerformLayout();
        _flpButtons.ResumeLayout(false);
        _flpButtons.PerformLayout();
        _tlpMain.ResumeLayout(false);
        _tlpMain.PerformLayout();
        ResumeLayout(false);
    }

    #endregion

    private TableLayoutPanel _tlpMain;
    private Label _lblProduct;
    private ComboBox _cboProduct;
    private Label _lblServerShare;
    private TextBox _txtServerShare;
    private Button _btnBrowseServer;
    private Label _lblLocalPath;
    private TextBox _txtLocalPath;
    private Button _btnBrowseLocal;
    private Label _lblSetupFile;
    private TextBox _txtSetupFile;
    private Label _lblSetupFilesPath;
    private TextBox _txtSetupFilesPath;
    private Button _btnBrowseSetup;
    private CheckBox _chkAutoLaunch;
    private GroupBox _grpComputedPaths;
    private TableLayoutPanel _tlpComputed;
    private Label _lblExeFileCaption;
    private Label _lblExeFileValue;
    private Label _lblServerExeCaption;
    private Label _lblServerExeValue;
    private Label _lblLocalExeCaption;
    private Label _lblLocalExeValue;
    private Label _lblSetupPathCaption;
    private Label _lblSetupPathValue;
    private FlowLayoutPanel _flpButtons;
    private Button _btnOK;
    private Button _btnCancel;
}

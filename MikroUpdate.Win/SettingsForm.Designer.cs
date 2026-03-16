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
        DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
        DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
        _tlpMain = new TableLayoutPanel();
        _lblMajorVersion = new Label();
        _cboMajorVersion = new ComboBox();
        _lblProduct = new Label();
        _cboProduct = new ComboBox();
        _lblServerShare = new Label();
        _txtServerShare = new TextBox();
        _btnBrowseServer = new Button();
        _lblLocalPath = new Label();
        _txtLocalPath = new TextBox();
        _btnBrowseLocal = new Button();
        _lblSetupFilesPath = new Label();
        _txtSetupFilesPath = new TextBox();
        _btnBrowseSetup = new Button();
        _lblCheckInterval = new Label();
        _nudCheckInterval = new NumericUpDown();
        _lblCheckIntervalUnit = new Label();
        _chkAutoLaunch = new CheckBox();
        _lblUpdateMode = new Label();
        _cboUpdateMode = new ComboBox();
        _lblCdnBaseUrl = new Label();
        _txtCdnBaseUrl = new TextBox();
        _lblGeminiApiKey = new Label();
        _txtGeminiApiKey = new TextBox();
        _lblUpdatePageUrl = new Label();
        _txtUpdatePageUrl = new TextBox();
        _lblProxyAddress = new Label();
        _txtProxyAddress = new TextBox();
        _lblHttpTimeout = new Label();
        _nudHttpTimeout = new NumericUpDown();
        _lblHttpTimeoutUnit = new Label();
        _lblModulesCaption = new Label();
        _btnResetModules = new Button();
        _dgvModules = new DataGridView();
        _colEnabled = new DataGridViewCheckBoxColumn();
        _colModuleName = new DataGridViewTextBoxColumn();
        _colSetupFile = new DataGridViewTextBoxColumn();
        _colExeFile = new DataGridViewTextBoxColumn();
        _pnlComputed = new Panel();
        _lblComputedPaths = new Label();
        _flpButtons = new FlowLayoutPanel();
        _btnCancel = new Button();
        _btnOK = new Button();
        _btnDefaults = new Button();
        _tlpMain.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_nudCheckInterval).BeginInit();
        ((System.ComponentModel.ISupportInitialize)_nudHttpTimeout).BeginInit();
        ((System.ComponentModel.ISupportInitialize)_dgvModules).BeginInit();
        _pnlComputed.SuspendLayout();
        _flpButtons.SuspendLayout();
        SuspendLayout();
        // 
        // _tlpMain
        // 
        _tlpMain.ColumnCount = 3;
        _tlpMain.ColumnStyles.Add(new ColumnStyle());
        _tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _tlpMain.ColumnStyles.Add(new ColumnStyle());
        _tlpMain.Controls.Add(_lblMajorVersion, 0, 0);
        _tlpMain.Controls.Add(_cboMajorVersion, 1, 0);
        _tlpMain.Controls.Add(_lblProduct, 0, 1);
        _tlpMain.Controls.Add(_cboProduct, 1, 1);
        _tlpMain.Controls.Add(_lblServerShare, 0, 2);
        _tlpMain.Controls.Add(_txtServerShare, 1, 2);
        _tlpMain.Controls.Add(_btnBrowseServer, 2, 2);
        _tlpMain.Controls.Add(_lblLocalPath, 0, 3);
        _tlpMain.Controls.Add(_txtLocalPath, 1, 3);
        _tlpMain.Controls.Add(_btnBrowseLocal, 2, 3);
        _tlpMain.Controls.Add(_lblSetupFilesPath, 0, 4);
        _tlpMain.Controls.Add(_txtSetupFilesPath, 1, 4);
        _tlpMain.Controls.Add(_btnBrowseSetup, 2, 4);
        _tlpMain.Controls.Add(_lblCheckInterval, 0, 5);
        _tlpMain.Controls.Add(_nudCheckInterval, 1, 5);
        _tlpMain.Controls.Add(_lblCheckIntervalUnit, 2, 5);
        _tlpMain.Controls.Add(_chkAutoLaunch, 0, 6);
        _tlpMain.Controls.Add(_lblUpdateMode, 0, 7);
        _tlpMain.Controls.Add(_cboUpdateMode, 1, 7);
        _tlpMain.Controls.Add(_lblCdnBaseUrl, 0, 8);
        _tlpMain.Controls.Add(_txtCdnBaseUrl, 1, 8);
        _tlpMain.Controls.Add(_lblGeminiApiKey, 0, 9);
        _tlpMain.Controls.Add(_txtGeminiApiKey, 1, 9);
        _tlpMain.Controls.Add(_lblUpdatePageUrl, 0, 10);
        _tlpMain.Controls.Add(_txtUpdatePageUrl, 1, 10);
        _tlpMain.Controls.Add(_lblProxyAddress, 0, 11);
        _tlpMain.Controls.Add(_txtProxyAddress, 1, 11);
        _tlpMain.Controls.Add(_lblHttpTimeout, 0, 12);
        _tlpMain.Controls.Add(_nudHttpTimeout, 1, 12);
        _tlpMain.Controls.Add(_lblHttpTimeoutUnit, 2, 12);
        _tlpMain.Controls.Add(_lblModulesCaption, 0, 13);
        _tlpMain.Controls.Add(_btnResetModules, 2, 13);
        _tlpMain.Controls.Add(_dgvModules, 0, 14);
        _tlpMain.Controls.Add(_pnlComputed, 0, 15);
        _tlpMain.Controls.Add(_flpButtons, 0, 16);
        _tlpMain.Dock = DockStyle.Fill;
        _tlpMain.Location = new Point(0, 0);
        _tlpMain.Name = "_tlpMain";
        _tlpMain.Padding = new Padding(16, 16, 16, 12);
        _tlpMain.RowCount = 17;
        _tlpMain.RowStyles.Add(new RowStyle());
        _tlpMain.RowStyles.Add(new RowStyle());
        _tlpMain.RowStyles.Add(new RowStyle());
        _tlpMain.RowStyles.Add(new RowStyle());
        _tlpMain.RowStyles.Add(new RowStyle());
        _tlpMain.RowStyles.Add(new RowStyle());
        _tlpMain.RowStyles.Add(new RowStyle());
        _tlpMain.RowStyles.Add(new RowStyle());
        _tlpMain.RowStyles.Add(new RowStyle());
        _tlpMain.RowStyles.Add(new RowStyle());
        _tlpMain.RowStyles.Add(new RowStyle());
        _tlpMain.RowStyles.Add(new RowStyle());
        _tlpMain.RowStyles.Add(new RowStyle());
        _tlpMain.RowStyles.Add(new RowStyle());
        _tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _tlpMain.RowStyles.Add(new RowStyle());
        _tlpMain.RowStyles.Add(new RowStyle());
        _tlpMain.Size = new Size(620, 620);
        _tlpMain.TabIndex = 0;
        // 
        // _lblMajorVersion
        // 
        _lblMajorVersion.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _lblMajorVersion.AutoSize = true;
        _lblMajorVersion.ForeColor = SystemColors.GrayText;
        _lblMajorVersion.Location = new Point(19, 23);
        _lblMajorVersion.Name = "_lblMajorVersion";
        _lblMajorVersion.Size = new Size(128, 15);
        _lblMajorVersion.TabIndex = 0;
        _lblMajorVersion.Text = "Ana Sürüm";
        // 
        // _cboMajorVersion
        // 
        _cboMajorVersion.AccessibleName = "Ana sürüm seçimi";
        _cboMajorVersion.Anchor = AnchorStyles.Left;
        _tlpMain.SetColumnSpan(_cboMajorVersion, 2);
        _cboMajorVersion.DropDownStyle = ComboBoxStyle.DropDownList;
        _cboMajorVersion.FlatStyle = FlatStyle.Flat;
        _cboMajorVersion.Items.AddRange(new object[] { "V16", "V17" });
        _cboMajorVersion.Location = new Point(153, 19);
        _cboMajorVersion.Name = "_cboMajorVersion";
        _cboMajorVersion.Size = new Size(121, 23);
        _cboMajorVersion.TabIndex = 0;
        // 
        // _lblProduct
        // 
        _lblProduct.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _lblProduct.AutoSize = true;
        _lblProduct.ForeColor = SystemColors.GrayText;
        _lblProduct.Location = new Point(19, 52);
        _lblProduct.Name = "_lblProduct";
        _lblProduct.Size = new Size(128, 15);
        _lblProduct.TabIndex = 1;
        _lblProduct.Text = "Ürün";
        // 
        // _cboProduct
        // 
        _cboProduct.AccessibleName = "Ürün seçimi";
        _cboProduct.Anchor = AnchorStyles.Left;
        _tlpMain.SetColumnSpan(_cboProduct, 2);
        _cboProduct.DropDownStyle = ComboBoxStyle.DropDownList;
        _cboProduct.FlatStyle = FlatStyle.Flat;
        _cboProduct.Items.AddRange(new object[] { "Jump", "Fly" });
        _cboProduct.Location = new Point(153, 48);
        _cboProduct.Name = "_cboProduct";
        _cboProduct.Size = new Size(121, 23);
        _cboProduct.TabIndex = 1;
        // 
        // _lblServerShare
        // 
        _lblServerShare.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _lblServerShare.AutoSize = true;
        _lblServerShare.ForeColor = SystemColors.GrayText;
        _lblServerShare.Location = new Point(19, 81);
        _lblServerShare.Name = "_lblServerShare";
        _lblServerShare.Size = new Size(128, 15);
        _lblServerShare.TabIndex = 2;
        _lblServerShare.Text = "Sunucu Paylaşım Yolu";
        // 
        // _txtServerShare
        // 
        _txtServerShare.AccessibleName = "Sunucu paylaşım yolu";
        _txtServerShare.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _txtServerShare.Location = new Point(153, 77);
        _txtServerShare.Name = "_txtServerShare";
        _txtServerShare.Size = new Size(390, 23);
        _txtServerShare.TabIndex = 2;
        // 
        // _btnBrowseServer
        // 
        _btnBrowseServer.AccessibleName = "Sunucu yolu seç";
        _btnBrowseServer.FlatAppearance.BorderSize = 0;
        _btnBrowseServer.FlatStyle = FlatStyle.Flat;
        _btnBrowseServer.Location = new Point(549, 77);
        _btnBrowseServer.Name = "_btnBrowseServer";
        _btnBrowseServer.Size = new Size(28, 23);
        _btnBrowseServer.TabIndex = 3;
        _btnBrowseServer.Text = "…";
        _btnBrowseServer.Click += BtnBrowseServer_Click;
        // 
        // _lblLocalPath
        // 
        _lblLocalPath.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _lblLocalPath.AutoSize = true;
        _lblLocalPath.ForeColor = SystemColors.GrayText;
        _lblLocalPath.Location = new Point(19, 110);
        _lblLocalPath.Name = "_lblLocalPath";
        _lblLocalPath.Size = new Size(128, 15);
        _lblLocalPath.TabIndex = 4;
        _lblLocalPath.Text = "Terminal Kurulum Yolu";
        // 
        // _txtLocalPath
        // 
        _txtLocalPath.AccessibleName = "Terminal kurulum yolu";
        _txtLocalPath.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _txtLocalPath.Location = new Point(153, 106);
        _txtLocalPath.Name = "_txtLocalPath";
        _txtLocalPath.Size = new Size(390, 23);
        _txtLocalPath.TabIndex = 4;
        // 
        // _btnBrowseLocal
        // 
        _btnBrowseLocal.AccessibleName = "Terminal yolu seç";
        _btnBrowseLocal.FlatAppearance.BorderSize = 0;
        _btnBrowseLocal.FlatStyle = FlatStyle.Flat;
        _btnBrowseLocal.Location = new Point(549, 106);
        _btnBrowseLocal.Name = "_btnBrowseLocal";
        _btnBrowseLocal.Size = new Size(28, 23);
        _btnBrowseLocal.TabIndex = 5;
        _btnBrowseLocal.Text = "…";
        _btnBrowseLocal.Click += BtnBrowseLocal_Click;
        // 
        // _lblSetupFilesPath
        // 
        _lblSetupFilesPath.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _lblSetupFilesPath.AutoSize = true;
        _lblSetupFilesPath.ForeColor = SystemColors.GrayText;
        _lblSetupFilesPath.Location = new Point(19, 139);
        _lblSetupFilesPath.Name = "_lblSetupFilesPath";
        _lblSetupFilesPath.Size = new Size(128, 15);
        _lblSetupFilesPath.TabIndex = 6;
        _lblSetupFilesPath.Text = "Setup Dosyaları Yolu";
        // 
        // _txtSetupFilesPath
        // 
        _txtSetupFilesPath.AccessibleName = "Setup dosyaları yolu";
        _txtSetupFilesPath.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _txtSetupFilesPath.Location = new Point(153, 135);
        _txtSetupFilesPath.Name = "_txtSetupFilesPath";
        _txtSetupFilesPath.Size = new Size(390, 23);
        _txtSetupFilesPath.TabIndex = 6;
        // 
        // _btnBrowseSetup
        // 
        _btnBrowseSetup.AccessibleName = "Setup dosyaları yolu seç";
        _btnBrowseSetup.FlatAppearance.BorderSize = 0;
        _btnBrowseSetup.FlatStyle = FlatStyle.Flat;
        _btnBrowseSetup.Location = new Point(549, 135);
        _btnBrowseSetup.Name = "_btnBrowseSetup";
        _btnBrowseSetup.Size = new Size(28, 23);
        _btnBrowseSetup.TabIndex = 7;
        _btnBrowseSetup.Text = "…";
        _btnBrowseSetup.Click += BtnBrowseSetup_Click;
        // 
        // _lblCheckInterval
        // 
        _lblCheckInterval.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _lblCheckInterval.AutoSize = true;
        _lblCheckInterval.ForeColor = SystemColors.GrayText;
        _lblCheckInterval.Location = new Point(19, 168);
        _lblCheckInterval.Name = "_lblCheckInterval";
        _lblCheckInterval.Size = new Size(128, 15);
        _lblCheckInterval.TabIndex = 8;
        _lblCheckInterval.Text = "Kontrol Aralığı";
        // 
        // _nudCheckInterval
        // 
        _nudCheckInterval.AccessibleName = "Kontrol aralığı dakika";
        _nudCheckInterval.Anchor = AnchorStyles.Left;
        _nudCheckInterval.Increment = new decimal(new int[] { 5, 0, 0, 0 });
        _nudCheckInterval.Location = new Point(153, 164);
        _nudCheckInterval.Maximum = new decimal(new int[] { 1440, 0, 0, 0 });
        _nudCheckInterval.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        _nudCheckInterval.Name = "_nudCheckInterval";
        _nudCheckInterval.Size = new Size(80, 23);
        _nudCheckInterval.TabIndex = 8;
        _nudCheckInterval.Value = new decimal(new int[] { 30, 0, 0, 0 });
        // 
        // _lblCheckIntervalUnit
        // 
        _lblCheckIntervalUnit.Anchor = AnchorStyles.Left;
        _lblCheckIntervalUnit.AutoSize = true;
        _lblCheckIntervalUnit.ForeColor = SystemColors.GrayText;
        _lblCheckIntervalUnit.Location = new Point(549, 168);
        _lblCheckIntervalUnit.Name = "_lblCheckIntervalUnit";
        _lblCheckIntervalUnit.Size = new Size(20, 15);
        _lblCheckIntervalUnit.TabIndex = 9;
        _lblCheckIntervalUnit.Text = "dk";
        // 
        // _chkAutoLaunch
        // 
        _chkAutoLaunch.AccessibleName = "Otomatik başlatma";
        _chkAutoLaunch.AutoSize = true;
        _chkAutoLaunch.Checked = true;
        _chkAutoLaunch.CheckState = CheckState.Checked;
        _tlpMain.SetColumnSpan(_chkAutoLaunch, 3);
        _chkAutoLaunch.Location = new Point(19, 198);
        _chkAutoLaunch.Margin = new Padding(3, 8, 3, 3);
        _chkAutoLaunch.Name = "_chkAutoLaunch";
        _chkAutoLaunch.Size = new Size(264, 19);
        _chkAutoLaunch.TabIndex = 9;
        _chkAutoLaunch.Text = "Güncelleme sonrası Mikro'yu otomatik başlat";
        // 
        // _lblUpdateMode
        // 
        _lblUpdateMode.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _lblUpdateMode.AutoSize = true;
        _lblUpdateMode.ForeColor = SystemColors.GrayText;
        _lblUpdateMode.Location = new Point(19, 228);
        _lblUpdateMode.Margin = new Padding(3, 8, 3, 3);
        _lblUpdateMode.Name = "_lblUpdateMode";
        _lblUpdateMode.Size = new Size(128, 15);
        _lblUpdateMode.TabIndex = 14;
        _lblUpdateMode.Text = "G\u00fcncelleme Modu";
        // 
        // _cboUpdateMode
        // 
        _cboUpdateMode.AccessibleName = "G\u00fcncelleme modu se\u00e7imi";
        _cboUpdateMode.Anchor = AnchorStyles.Left;
        _tlpMain.SetColumnSpan(_cboUpdateMode, 2);
        _cboUpdateMode.DropDownStyle = ComboBoxStyle.DropDownList;
        _cboUpdateMode.FlatStyle = FlatStyle.Flat;
        _cboUpdateMode.Items.AddRange(new object[] { "Local", "Online", "Hybrid", "AI" });
        _cboUpdateMode.Location = new Point(153, 224);
        _cboUpdateMode.Name = "_cboUpdateMode";
        _cboUpdateMode.Size = new Size(121, 23);
        _cboUpdateMode.TabIndex = 14;
        // 
        // _lblCdnBaseUrl
        // 
        _lblCdnBaseUrl.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _lblCdnBaseUrl.AutoSize = true;
        _lblCdnBaseUrl.ForeColor = SystemColors.GrayText;
        _lblCdnBaseUrl.Location = new Point(19, 257);
        _lblCdnBaseUrl.Name = "_lblCdnBaseUrl";
        _lblCdnBaseUrl.Size = new Size(128, 15);
        _lblCdnBaseUrl.TabIndex = 15;
        _lblCdnBaseUrl.Text = "CDN URL";
        _lblCdnBaseUrl.Visible = false;
        // 
        // _txtCdnBaseUrl
        // 
        _txtCdnBaseUrl.AccessibleName = "CDN temel URL";
        _txtCdnBaseUrl.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _tlpMain.SetColumnSpan(_txtCdnBaseUrl, 2);
        _txtCdnBaseUrl.Location = new Point(153, 253);
        _txtCdnBaseUrl.Name = "_txtCdnBaseUrl";
        _txtCdnBaseUrl.Size = new Size(448, 23);
        _txtCdnBaseUrl.TabIndex = 15;
        _txtCdnBaseUrl.Visible = false;
        // 
        // _lblGeminiApiKey
        // 
        _lblGeminiApiKey.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _lblGeminiApiKey.AutoSize = true;
        _lblGeminiApiKey.ForeColor = SystemColors.GrayText;
        _lblGeminiApiKey.Location = new Point(19, 282);
        _lblGeminiApiKey.Name = "_lblGeminiApiKey";
        _lblGeminiApiKey.Size = new Size(128, 15);
        _lblGeminiApiKey.TabIndex = 16;
        _lblGeminiApiKey.Text = "Gemini API Anahtarı";
        _lblGeminiApiKey.Visible = false;
        // 
        // _txtGeminiApiKey
        // 
        _txtGeminiApiKey.AccessibleName = "Gemini API anahtarı";
        _txtGeminiApiKey.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _tlpMain.SetColumnSpan(_txtGeminiApiKey, 2);
        _txtGeminiApiKey.Location = new Point(153, 278);
        _txtGeminiApiKey.Name = "_txtGeminiApiKey";
        _txtGeminiApiKey.Size = new Size(448, 23);
        _txtGeminiApiKey.TabIndex = 16;
        _txtGeminiApiKey.UseSystemPasswordChar = true;
        _txtGeminiApiKey.Visible = false;
        // 
        // _lblUpdatePageUrl
        // 
        _lblUpdatePageUrl.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _lblUpdatePageUrl.AutoSize = true;
        _lblUpdatePageUrl.ForeColor = SystemColors.GrayText;
        _lblUpdatePageUrl.Location = new Point(19, 311);
        _lblUpdatePageUrl.Name = "_lblUpdatePageUrl";
        _lblUpdatePageUrl.Size = new Size(128, 15);
        _lblUpdatePageUrl.TabIndex = 17;
        _lblUpdatePageUrl.Text = "Güncelleme Sayfası URL";
        _lblUpdatePageUrl.Visible = false;
        // 
        // _txtUpdatePageUrl
        // 
        _txtUpdatePageUrl.AccessibleName = "Güncelleme sayfası URL";
        _txtUpdatePageUrl.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _tlpMain.SetColumnSpan(_txtUpdatePageUrl, 2);
        _txtUpdatePageUrl.Location = new Point(153, 307);
        _txtUpdatePageUrl.Name = "_txtUpdatePageUrl";
        _txtUpdatePageUrl.Size = new Size(448, 23);
        _txtUpdatePageUrl.TabIndex = 17;
        _txtUpdatePageUrl.Visible = false;
        // 
        // _lblProxyAddress
        // 
        _lblProxyAddress.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _lblProxyAddress.AutoSize = true;
        _lblProxyAddress.ForeColor = SystemColors.GrayText;
        _lblProxyAddress.Location = new Point(19, 340);
        _lblProxyAddress.Name = "_lblProxyAddress";
        _lblProxyAddress.Size = new Size(128, 15);
        _lblProxyAddress.TabIndex = 18;
        _lblProxyAddress.Text = "Proxy Adresi";
        _lblProxyAddress.Visible = false;
        // 
        // _txtProxyAddress
        // 
        _txtProxyAddress.AccessibleName = "Proxy adresi";
        _txtProxyAddress.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _tlpMain.SetColumnSpan(_txtProxyAddress, 2);
        _txtProxyAddress.Location = new Point(153, 336);
        _txtProxyAddress.Name = "_txtProxyAddress";
        _txtProxyAddress.PlaceholderText = "http://proxy:8080";
        _txtProxyAddress.Size = new Size(448, 23);
        _txtProxyAddress.TabIndex = 18;
        _txtProxyAddress.Visible = false;
        // 
        // _lblHttpTimeout
        // 
        _lblHttpTimeout.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _lblHttpTimeout.AutoSize = true;
        _lblHttpTimeout.ForeColor = SystemColors.GrayText;
        _lblHttpTimeout.Location = new Point(19, 369);
        _lblHttpTimeout.Name = "_lblHttpTimeout";
        _lblHttpTimeout.Size = new Size(128, 15);
        _lblHttpTimeout.TabIndex = 19;
        _lblHttpTimeout.Text = "HTTP Zaman A\u015F\u0131m\u0131";
        _lblHttpTimeout.Visible = false;
        // 
        // _nudHttpTimeout
        // 
        _nudHttpTimeout.Anchor = AnchorStyles.Left;
        _nudHttpTimeout.Location = new Point(153, 365);
        _nudHttpTimeout.Maximum = new decimal(new int[] { 3600, 0, 0, 0 });
        _nudHttpTimeout.Name = "_nudHttpTimeout";
        _nudHttpTimeout.Size = new Size(80, 23);
        _nudHttpTimeout.TabIndex = 19;
        _nudHttpTimeout.Visible = false;
        // 
        // _lblHttpTimeoutUnit
        // 
        _lblHttpTimeoutUnit.Anchor = AnchorStyles.Left;
        _lblHttpTimeoutUnit.AutoSize = true;
        _lblHttpTimeoutUnit.ForeColor = SystemColors.GrayText;
        _lblHttpTimeoutUnit.Location = new Point(239, 369);
        _lblHttpTimeoutUnit.Name = "_lblHttpTimeoutUnit";
        _lblHttpTimeoutUnit.Size = new Size(95, 15);
        _lblHttpTimeoutUnit.TabIndex = 19;
        _lblHttpTimeoutUnit.Text = "sn (0 = varsay\u0131lan)";
        _lblHttpTimeoutUnit.Visible = false;
        // 
        // _lblModulesCaption
        //
        _lblModulesCaption.Anchor = AnchorStyles.Left;
        _lblModulesCaption.AutoSize = true;
        _tlpMain.SetColumnSpan(_lblModulesCaption, 2);
        _lblModulesCaption.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
        _lblModulesCaption.ForeColor = SystemColors.GrayText;
        _lblModulesCaption.Location = new Point(19, 235);
        _lblModulesCaption.Margin = new Padding(3, 10, 3, 2);
        _lblModulesCaption.Name = "_lblModulesCaption";
        _lblModulesCaption.Size = new Size(59, 12);
        _lblModulesCaption.TabIndex = 10;
        _lblModulesCaption.Text = "MODÜLLER";
        // 
        // _btnResetModules
        // 
        _btnResetModules.AccessibleName = "Modülleri sıfırla";
        _btnResetModules.Anchor = AnchorStyles.Right;
        _btnResetModules.AutoSize = true;
        _btnResetModules.FlatAppearance.BorderSize = 0;
        _btnResetModules.FlatStyle = FlatStyle.Flat;
        _btnResetModules.Font = new Font("Segoe UI", 7.5F);
        _btnResetModules.ForeColor = SystemColors.GrayText;
        _btnResetModules.Location = new Point(549, 230);
        _btnResetModules.Margin = new Padding(3, 10, 3, 2);
        _btnResetModules.Name = "_btnResetModules";
        _btnResetModules.Size = new Size(52, 22);
        _btnResetModules.TabIndex = 10;
        _btnResetModules.Text = "Sıfırla";
        _btnResetModules.Click += BtnResetModules_Click;
        // 
        // _dgvModules
        // 
        _dgvModules.AllowUserToAddRows = false;
        _dgvModules.AllowUserToDeleteRows = false;
        _dgvModules.AllowUserToResizeRows = false;
        _dgvModules.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        _dgvModules.BackgroundColor = Color.FromArgb(35, 35, 35);
        _dgvModules.BorderStyle = BorderStyle.None;
        dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
        dataGridViewCellStyle1.BackColor = Color.FromArgb(40, 40, 40);
        dataGridViewCellStyle1.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
        dataGridViewCellStyle1.ForeColor = SystemColors.GrayText;
        dataGridViewCellStyle1.SelectionBackColor = Color.FromArgb(40, 40, 40);
        dataGridViewCellStyle1.SelectionForeColor = SystemColors.GrayText;
        dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
        _dgvModules.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
        _dgvModules.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        _dgvModules.Columns.AddRange(new DataGridViewColumn[] { _colEnabled, _colModuleName, _colSetupFile, _colExeFile });
        _tlpMain.SetColumnSpan(_dgvModules, 3);
        dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
        dataGridViewCellStyle2.BackColor = Color.FromArgb(35, 35, 35);
        dataGridViewCellStyle2.Font = new Font("Segoe UI", 8.5F);
        dataGridViewCellStyle2.ForeColor = Color.FromArgb(230, 230, 230);
        dataGridViewCellStyle2.SelectionBackColor = Color.FromArgb(50, 50, 50);
        dataGridViewCellStyle2.SelectionForeColor = Color.FromArgb(230, 230, 230);
        dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
        _dgvModules.DefaultCellStyle = dataGridViewCellStyle2;
        _dgvModules.EnableHeadersVisualStyles = false;
        _dgvModules.GridColor = Color.FromArgb(50, 50, 50);
        _dgvModules.Location = new Point(19, 257);
        _dgvModules.MultiSelect = false;
        _dgvModules.Name = "_dgvModules";
        _dgvModules.RowHeadersVisible = false;
        _dgvModules.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _dgvModules.Size = new Size(582, 215);
        _dgvModules.TabIndex = 11;
        // 
        // _colEnabled
        // 
        _colEnabled.HeaderText = "";
        _colEnabled.Name = "_colEnabled";
        _colEnabled.Width = 32;
        // 
        // _colModuleName
        // 
        _colModuleName.HeaderText = "MODÜL";
        _colModuleName.Name = "_colModuleName";
        _colModuleName.SortMode = DataGridViewColumnSortMode.NotSortable;
        _colModuleName.Width = 85;
        // 
        // _colSetupFile
        // 
        _colSetupFile.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        _colSetupFile.HeaderText = "SETUP DOSYASI";
        _colSetupFile.Name = "_colSetupFile";
        _colSetupFile.SortMode = DataGridViewColumnSortMode.NotSortable;
        // 
        // _colExeFile
        // 
        _colExeFile.HeaderText = "EXE DOSYASI";
        _colExeFile.Name = "_colExeFile";
        _colExeFile.SortMode = DataGridViewColumnSortMode.NotSortable;
        _colExeFile.Width = 150;
        // 
        // _pnlComputed
        // 
        _pnlComputed.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _pnlComputed.AutoSize = true;
        _tlpMain.SetColumnSpan(_pnlComputed, 3);
        _pnlComputed.Controls.Add(_lblComputedPaths);
        _pnlComputed.Location = new Point(19, 478);
        _pnlComputed.Name = "_pnlComputed";
        _pnlComputed.Padding = new Padding(0, 4, 0, 0);
        _pnlComputed.Size = new Size(582, 16);
        _pnlComputed.TabIndex = 11;
        // 
        // _lblComputedPaths
        // 
        _lblComputedPaths.AutoSize = true;
        _lblComputedPaths.Dock = DockStyle.Top;
        _lblComputedPaths.Font = new Font("Segoe UI", 7.5F);
        _lblComputedPaths.ForeColor = SystemColors.GrayText;
        _lblComputedPaths.Location = new Point(0, 4);
        _lblComputedPaths.Name = "_lblComputedPaths";
        _lblComputedPaths.Size = new Size(15, 12);
        _lblComputedPaths.TabIndex = 0;
        _lblComputedPaths.Text = "—";
        // 
        // _flpButtons
        // 
        _flpButtons.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _flpButtons.AutoSize = true;
        _tlpMain.SetColumnSpan(_flpButtons, 3);
        _flpButtons.Controls.Add(_btnCancel);
        _flpButtons.Controls.Add(_btnOK);
        _flpButtons.Controls.Add(_btnDefaults);
        _flpButtons.FlowDirection = FlowDirection.RightToLeft;
        _flpButtons.Location = new Point(19, 500);
        _flpButtons.Name = "_flpButtons";
        _flpButtons.Padding = new Padding(0, 4, 0, 0);
        _flpButtons.Size = new Size(582, 45);
        _flpButtons.TabIndex = 12;
        // 
        // _btnCancel
        // 
        _btnCancel.AccessibleName = "İptal";
        _btnCancel.AutoSize = true;
        _btnCancel.DialogResult = DialogResult.Cancel;
        _btnCancel.FlatStyle = FlatStyle.Flat;
        _btnCancel.Location = new Point(504, 7);
        _btnCancel.Name = "_btnCancel";
        _btnCancel.Padding = new Padding(12, 4, 12, 4);
        _btnCancel.Size = new Size(75, 35);
        _btnCancel.TabIndex = 12;
        _btnCancel.Text = "İptal";
        // 
        // _btnOK
        // 
        _btnOK.AccessibleName = "Kaydet ve kapat";
        _btnOK.AutoSize = true;
        _btnOK.BackColor = Color.FromArgb(0, 150, 80);
        _btnOK.DialogResult = DialogResult.OK;
        _btnOK.FlatAppearance.BorderSize = 0;
        _btnOK.FlatStyle = FlatStyle.Flat;
        _btnOK.Font = new Font("Segoe UI Semibold", 9F);
        _btnOK.ForeColor = Color.White;
        _btnOK.Location = new Point(413, 7);
        _btnOK.Name = "_btnOK";
        _btnOK.Padding = new Padding(16, 4, 16, 4);
        _btnOK.Size = new Size(85, 33);
        _btnOK.TabIndex = 11;
        _btnOK.Text = "Kaydet";
        _btnOK.UseVisualStyleBackColor = false;
        // 
        // _btnDefaults
        // 
        _btnDefaults.AccessibleDescription = "Tüm ayarları fabrika varsayılanlarına sıfırlar";
        _btnDefaults.AccessibleName = "Varsayılan ayarlara dön";
        _btnDefaults.AutoSize = true;
        _btnDefaults.FlatAppearance.BorderSize = 0;
        _btnDefaults.FlatStyle = FlatStyle.Flat;
        _btnDefaults.Font = new Font("Segoe UI", 8F);
        _btnDefaults.ForeColor = Color.FromArgb(180, 180, 180);
        _btnDefaults.Location = new Point(322, 7);
        _btnDefaults.Name = "_btnDefaults";
        _btnDefaults.Padding = new Padding(8, 4, 8, 4);
        _btnDefaults.Size = new Size(85, 33);
        _btnDefaults.TabIndex = 13;
        _btnDefaults.Text = "Varsayılan";
        _btnDefaults.Click += BtnDefaults_Click;
        // 
        // SettingsForm
        // 
        AcceptButton = _btnOK;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(30, 30, 30);
        CancelButton = _btnCancel;
        ClientSize = new Size(620, 620);
        Controls.Add(_tlpMain);
        ForeColor = Color.FromArgb(230, 230, 230);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        MinimumSize = new Size(540, 480);
        Name = "SettingsForm";
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Ayarlar";
        _tlpMain.ResumeLayout(false);
        _tlpMain.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)_nudCheckInterval).EndInit();
        ((System.ComponentModel.ISupportInitialize)_nudHttpTimeout).EndInit();
        ((System.ComponentModel.ISupportInitialize)_dgvModules).EndInit();
        _pnlComputed.ResumeLayout(false);
        _pnlComputed.PerformLayout();
        _flpButtons.ResumeLayout(false);
        _flpButtons.PerformLayout();
        ResumeLayout(false);
    }

    #endregion

    private TableLayoutPanel _tlpMain;
    private Label _lblMajorVersion;
    private ComboBox _cboMajorVersion;
    private Label _lblProduct;
    private ComboBox _cboProduct;
    private Label _lblServerShare;
    private TextBox _txtServerShare;
    private Button _btnBrowseServer;
    private Label _lblLocalPath;
    private TextBox _txtLocalPath;
    private Button _btnBrowseLocal;
    private Label _lblSetupFilesPath;
    private TextBox _txtSetupFilesPath;
    private Button _btnBrowseSetup;
    private Label _lblCheckInterval;
    private NumericUpDown _nudCheckInterval;
    private Label _lblCheckIntervalUnit;
    private CheckBox _chkAutoLaunch;
    private Label _lblModulesCaption;
    private Button _btnResetModules;
    private DataGridView _dgvModules;
    private DataGridViewCheckBoxColumn _colEnabled;
    private DataGridViewTextBoxColumn _colModuleName;
    private DataGridViewTextBoxColumn _colSetupFile;
    private DataGridViewTextBoxColumn _colExeFile;
    private Panel _pnlComputed;
    private Label _lblComputedPaths;
    private FlowLayoutPanel _flpButtons;
    private Button _btnOK;
    private Button _btnCancel;
    private Button _btnDefaults;
    private Label _lblUpdateMode;
    private ComboBox _cboUpdateMode;
    private Label _lblCdnBaseUrl;
    private TextBox _txtCdnBaseUrl;
    private Label _lblGeminiApiKey;
    private TextBox _txtGeminiApiKey;
    private Label _lblUpdatePageUrl;
    private TextBox _txtUpdatePageUrl;
    private Label _lblProxyAddress;
    private TextBox _txtProxyAddress;
    private Label _lblHttpTimeout;
    private NumericUpDown _nudHttpTimeout;
    private Label _lblHttpTimeoutUnit;
}


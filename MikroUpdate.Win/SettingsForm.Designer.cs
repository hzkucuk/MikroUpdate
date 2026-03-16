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
        _tlpMain = new TableLayoutPanel();
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
        _lblSetupFile = new Label();
        _txtSetupFile = new TextBox();
        _lblCheckInterval = new Label();
        _nudCheckInterval = new NumericUpDown();
        _lblCheckIntervalUnit = new Label();
        _chkAutoLaunch = new CheckBox();
        _pnlComputed = new Panel();
        _lblComputedPaths = new Label();
        _flpButtons = new FlowLayoutPanel();
        _btnCancel = new Button();
        _btnOK = new Button();
        _tlpMain.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_nudCheckInterval).BeginInit();
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
        _tlpMain.Controls.Add(_lblProduct, 0, 0);
        _tlpMain.Controls.Add(_cboProduct, 1, 0);
        _tlpMain.Controls.Add(_lblServerShare, 0, 1);
        _tlpMain.Controls.Add(_txtServerShare, 1, 1);
        _tlpMain.Controls.Add(_btnBrowseServer, 2, 1);
        _tlpMain.Controls.Add(_lblLocalPath, 0, 2);
        _tlpMain.Controls.Add(_txtLocalPath, 1, 2);
        _tlpMain.Controls.Add(_btnBrowseLocal, 2, 2);
        _tlpMain.Controls.Add(_lblSetupFilesPath, 0, 3);
        _tlpMain.Controls.Add(_txtSetupFilesPath, 1, 3);
        _tlpMain.Controls.Add(_btnBrowseSetup, 2, 3);
        _tlpMain.Controls.Add(_lblSetupFile, 0, 4);
        _tlpMain.Controls.Add(_txtSetupFile, 1, 4);
        _tlpMain.Controls.Add(_lblCheckInterval, 0, 5);
        _tlpMain.Controls.Add(_nudCheckInterval, 1, 5);
        _tlpMain.Controls.Add(_lblCheckIntervalUnit, 2, 5);
        _tlpMain.Controls.Add(_chkAutoLaunch, 0, 6);
        _tlpMain.Controls.Add(_pnlComputed, 0, 7);
        _tlpMain.Controls.Add(_flpButtons, 0, 8);
        _tlpMain.Dock = DockStyle.Fill;
        _tlpMain.Location = new Point(0, 0);
        _tlpMain.Name = "_tlpMain";
        _tlpMain.Padding = new Padding(16, 16, 16, 12);
        _tlpMain.RowCount = 9;
        _tlpMain.RowStyles.Add(new RowStyle());
        _tlpMain.RowStyles.Add(new RowStyle());
        _tlpMain.RowStyles.Add(new RowStyle());
        _tlpMain.RowStyles.Add(new RowStyle());
        _tlpMain.RowStyles.Add(new RowStyle());
        _tlpMain.RowStyles.Add(new RowStyle());
        _tlpMain.RowStyles.Add(new RowStyle());
        _tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _tlpMain.RowStyles.Add(new RowStyle());
        _tlpMain.Size = new Size(560, 420);
        _tlpMain.TabIndex = 0;
        // 
        // _lblProduct
        // 
        _lblProduct.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _lblProduct.AutoSize = true;
        _lblProduct.ForeColor = SystemColors.GrayText;
        _lblProduct.Location = new Point(19, 23);
        _lblProduct.Name = "_lblProduct";
        _lblProduct.Size = new Size(128, 15);
        _lblProduct.TabIndex = 0;
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
        _cboProduct.Location = new Point(153, 19);
        _cboProduct.Name = "_cboProduct";
        _cboProduct.Size = new Size(121, 23);
        _cboProduct.TabIndex = 0;
        // 
        // _lblServerShare
        // 
        _lblServerShare.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _lblServerShare.AutoSize = true;
        _lblServerShare.ForeColor = SystemColors.GrayText;
        _lblServerShare.Location = new Point(19, 52);
        _lblServerShare.Name = "_lblServerShare";
        _lblServerShare.Size = new Size(128, 15);
        _lblServerShare.TabIndex = 1;
        _lblServerShare.Text = "Sunucu Paylaşım Yolu";
        // 
        // _txtServerShare
        // 
        _txtServerShare.AccessibleName = "Sunucu paylaşım yolu";
        _txtServerShare.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _txtServerShare.Location = new Point(153, 48);
        _txtServerShare.Name = "_txtServerShare";
        _txtServerShare.Size = new Size(354, 23);
        _txtServerShare.TabIndex = 1;
        // 
        // _btnBrowseServer
        // 
        _btnBrowseServer.AccessibleName = "Sunucu yolu seç";
        _btnBrowseServer.FlatAppearance.BorderSize = 0;
        _btnBrowseServer.FlatStyle = FlatStyle.Flat;
        _btnBrowseServer.Location = new Point(513, 48);
        _btnBrowseServer.Name = "_btnBrowseServer";
        _btnBrowseServer.Size = new Size(28, 23);
        _btnBrowseServer.TabIndex = 2;
        _btnBrowseServer.Text = "…";
        _btnBrowseServer.Click += BtnBrowseServer_Click;
        // 
        // _lblLocalPath
        // 
        _lblLocalPath.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _lblLocalPath.AutoSize = true;
        _lblLocalPath.ForeColor = SystemColors.GrayText;
        _lblLocalPath.Location = new Point(19, 81);
        _lblLocalPath.Name = "_lblLocalPath";
        _lblLocalPath.Size = new Size(128, 15);
        _lblLocalPath.TabIndex = 3;
        _lblLocalPath.Text = "Terminal Kurulum Yolu";
        // 
        // _txtLocalPath
        // 
        _txtLocalPath.AccessibleName = "Terminal kurulum yolu";
        _txtLocalPath.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _txtLocalPath.Location = new Point(153, 77);
        _txtLocalPath.Name = "_txtLocalPath";
        _txtLocalPath.Size = new Size(354, 23);
        _txtLocalPath.TabIndex = 3;
        // 
        // _btnBrowseLocal
        // 
        _btnBrowseLocal.AccessibleName = "Terminal yolu seç";
        _btnBrowseLocal.FlatAppearance.BorderSize = 0;
        _btnBrowseLocal.FlatStyle = FlatStyle.Flat;
        _btnBrowseLocal.Location = new Point(513, 77);
        _btnBrowseLocal.Name = "_btnBrowseLocal";
        _btnBrowseLocal.Size = new Size(28, 23);
        _btnBrowseLocal.TabIndex = 4;
        _btnBrowseLocal.Text = "…";
        _btnBrowseLocal.Click += BtnBrowseLocal_Click;
        // 
        // _lblSetupFilesPath
        // 
        _lblSetupFilesPath.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _lblSetupFilesPath.AutoSize = true;
        _lblSetupFilesPath.ForeColor = SystemColors.GrayText;
        _lblSetupFilesPath.Location = new Point(19, 110);
        _lblSetupFilesPath.Name = "_lblSetupFilesPath";
        _lblSetupFilesPath.Size = new Size(128, 15);
        _lblSetupFilesPath.TabIndex = 5;
        _lblSetupFilesPath.Text = "Setup Dosyaları Yolu";
        // 
        // _txtSetupFilesPath
        // 
        _txtSetupFilesPath.AccessibleName = "Setup dosyaları yolu";
        _txtSetupFilesPath.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _txtSetupFilesPath.Location = new Point(153, 106);
        _txtSetupFilesPath.Name = "_txtSetupFilesPath";
        _txtSetupFilesPath.Size = new Size(354, 23);
        _txtSetupFilesPath.TabIndex = 5;
        // 
        // _btnBrowseSetup
        // 
        _btnBrowseSetup.AccessibleName = "Setup dosyaları yolu seç";
        _btnBrowseSetup.FlatAppearance.BorderSize = 0;
        _btnBrowseSetup.FlatStyle = FlatStyle.Flat;
        _btnBrowseSetup.Location = new Point(513, 106);
        _btnBrowseSetup.Name = "_btnBrowseSetup";
        _btnBrowseSetup.Size = new Size(28, 23);
        _btnBrowseSetup.TabIndex = 6;
        _btnBrowseSetup.Text = "…";
        _btnBrowseSetup.Click += BtnBrowseSetup_Click;
        // 
        // _lblSetupFile
        // 
        _lblSetupFile.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _lblSetupFile.AutoSize = true;
        _lblSetupFile.ForeColor = SystemColors.GrayText;
        _lblSetupFile.Location = new Point(19, 139);
        _lblSetupFile.Name = "_lblSetupFile";
        _lblSetupFile.Size = new Size(128, 15);
        _lblSetupFile.TabIndex = 7;
        _lblSetupFile.Text = "Setup Dosyası";
        // 
        // _txtSetupFile
        // 
        _txtSetupFile.AccessibleName = "Setup dosya adı";
        _txtSetupFile.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _tlpMain.SetColumnSpan(_txtSetupFile, 2);
        _txtSetupFile.Location = new Point(153, 135);
        _txtSetupFile.Name = "_txtSetupFile";
        _txtSetupFile.Size = new Size(388, 23);
        _txtSetupFile.TabIndex = 7;
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
        _lblCheckIntervalUnit.Location = new Point(513, 168);
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
        // _pnlComputed
        // 
        _pnlComputed.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        _pnlComputed.AutoScroll = true;
        _tlpMain.SetColumnSpan(_pnlComputed, 3);
        _pnlComputed.Controls.Add(_lblComputedPaths);
        _pnlComputed.Location = new Point(19, 223);
        _pnlComputed.Name = "_pnlComputed";
        _pnlComputed.Padding = new Padding(0, 8, 0, 0);
        _pnlComputed.Size = new Size(522, 131);
        _pnlComputed.TabIndex = 10;
        // 
        // _lblComputedPaths
        // 
        _lblComputedPaths.AutoSize = true;
        _lblComputedPaths.Dock = DockStyle.Top;
        _lblComputedPaths.Font = new Font("Segoe UI", 7.5F);
        _lblComputedPaths.ForeColor = SystemColors.GrayText;
        _lblComputedPaths.Location = new Point(0, 8);
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
        _flpButtons.FlowDirection = FlowDirection.RightToLeft;
        _flpButtons.Location = new Point(19, 360);
        _flpButtons.Name = "_flpButtons";
        _flpButtons.Padding = new Padding(0, 4, 0, 0);
        _flpButtons.Size = new Size(522, 45);
        _flpButtons.TabIndex = 11;
        // 
        // _btnCancel
        // 
        _btnCancel.AccessibleName = "İptal";
        _btnCancel.AutoSize = true;
        _btnCancel.DialogResult = DialogResult.Cancel;
        _btnCancel.FlatStyle = FlatStyle.Flat;
        _btnCancel.Location = new Point(444, 7);
        _btnCancel.Name = "_btnCancel";
        _btnCancel.Padding = new Padding(12, 4, 12, 4);
        _btnCancel.Size = new Size(75, 35);
        _btnCancel.TabIndex = 11;
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
        _btnOK.Location = new Point(353, 7);
        _btnOK.Name = "_btnOK";
        _btnOK.Padding = new Padding(16, 4, 16, 4);
        _btnOK.Size = new Size(85, 33);
        _btnOK.TabIndex = 10;
        _btnOK.Text = "Kaydet";
        _btnOK.UseVisualStyleBackColor = false;
        // 
        // SettingsForm
        // 
        AcceptButton = _btnOK;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(30, 30, 30);
        CancelButton = _btnCancel;
        ClientSize = new Size(560, 420);
        Controls.Add(_tlpMain);
        ForeColor = Color.FromArgb(230, 230, 230);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        MinimumSize = new Size(480, 380);
        Name = "SettingsForm";
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Ayarlar";
        _tlpMain.ResumeLayout(false);
        _tlpMain.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)_nudCheckInterval).EndInit();
        _pnlComputed.ResumeLayout(false);
        _pnlComputed.PerformLayout();
        _flpButtons.ResumeLayout(false);
        _flpButtons.PerformLayout();
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
    private Label _lblSetupFilesPath;
    private TextBox _txtSetupFilesPath;
    private Button _btnBrowseSetup;
    private Label _lblSetupFile;
    private TextBox _txtSetupFile;
    private Label _lblCheckInterval;
    private NumericUpDown _nudCheckInterval;
    private Label _lblCheckIntervalUnit;
    private CheckBox _chkAutoLaunch;
    private Panel _pnlComputed;
    private Label _lblComputedPaths;
    private FlowLayoutPanel _flpButtons;
    private Button _btnOK;
    private Button _btnCancel;
}

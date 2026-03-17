namespace MikroUpdate.Win;

partial class Form1
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
        _tlpHeader = new TableLayoutPanel();
        _lblConfigInfo = new Label();
        _lblStatusCaption = new Label();
        _lblStatus = new Label();
        _dgvModules = new DataGridView();
        _colModuleName = new DataGridViewTextBoxColumn();
        _colLocalVersion = new DataGridViewTextBoxColumn();
        _colServerVersion = new DataGridViewTextBoxColumn();
        _colStatus = new DataGridViewTextBoxColumn();
        _prgProgress = new ProgressBar();
        _rtbLog = new RichTextBox();
        _flpButtons = new FlowLayoutPanel();
        _btnLaunch = new Button();
        _btnUpdate = new Button();
        _btnCheck = new Button();
        _btnSettings = new Button();
        _btnAbout = new Button();
        _notifyIcon = new NotifyIcon(components);
        _ctxTray = new ContextMenuStrip(components);
        _tsmShow = new ToolStripMenuItem();
        _tsmCheck = new ToolStripMenuItem();
        _tsmUpdate = new ToolStripMenuItem();
        _tsmSettings = new ToolStripMenuItem();
        _tsmAbout = new ToolStripMenuItem();
        _tsmSepUpdate = new ToolStripSeparator();
        _tsmSelfUpdate = new ToolStripMenuItem();
        _tsmSeparator = new ToolStripSeparator();
        _tsmExit = new ToolStripMenuItem();

        // 3. Suspend
        _tlpMain.SuspendLayout();
        _tlpHeader.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_dgvModules).BeginInit();
        _flpButtons.SuspendLayout();
        _ctxTray.SuspendLayout();
        SuspendLayout();

        // 4. Configure controls

        // _ctxTray
        _ctxTray.Items.AddRange(new ToolStripItem[] { _tsmShow, _tsmCheck, _tsmUpdate, _tsmSettings, _tsmAbout, _tsmSepUpdate, _tsmSelfUpdate, _tsmSeparator, _tsmExit });
        _ctxTray.Name = "_ctxTray";
        _ctxTray.Size = new Size(180, 120);

        _tsmShow.Text = "Göster";
        _tsmShow.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        _tsmShow.Name = "_tsmShow";
        _tsmShow.Click += TsmShow_Click;

        _tsmCheck.Text = "Kontrol Et";
        _tsmCheck.Name = "_tsmCheck";
        _tsmCheck.Click += BtnCheck_Click;

        _tsmUpdate.Text = "Güncelle";
        _tsmUpdate.Name = "_tsmUpdate";
        _tsmUpdate.Click += BtnUpdate_Click;

        _tsmSettings.Text = "Ayarlar";
        _tsmSettings.Name = "_tsmSettings";
        _tsmSettings.Click += BtnSettings_Click;

        _tsmAbout.Text = "Hakkında";
        _tsmAbout.Name = "_tsmAbout";
        _tsmAbout.Click += BtnAbout_Click;

        _tsmSepUpdate.Name = "_tsmSepUpdate";

        _tsmSelfUpdate.Text = "Uygulama Güncellemesi";
        _tsmSelfUpdate.Name = "_tsmSelfUpdate";
        _tsmSelfUpdate.Click += BtnSelfUpdate_Click;

        _tsmSeparator.Name = "_tsmSeparator";

        _tsmExit.Text = "Çıkış";
        _tsmExit.Name = "_tsmExit";
        _tsmExit.Click += TsmExit_Click;

        // _notifyIcon
        _notifyIcon.ContextMenuStrip = _ctxTray;
        _notifyIcon.Icon = Icon;
        _notifyIcon.Text = "MikroUpdate";
        _notifyIcon.Visible = true;
        _notifyIcon.DoubleClick += TsmShow_Click;

        // _tlpMain (5 rows: Header, Modules, Progress, Log, Buttons)
        _tlpMain.ColumnCount = 1;
        _tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _tlpMain.RowCount = 5;
        _tlpMain.RowStyles.Add(new RowStyle());
        _tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 110F));
        _tlpMain.RowStyles.Add(new RowStyle());
        _tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _tlpMain.RowStyles.Add(new RowStyle());
        _tlpMain.Dock = DockStyle.Fill;
        _tlpMain.Padding = new Padding(12, 12, 12, 8);
        _tlpMain.Name = "_tlpMain";
        _tlpMain.Controls.Add(_tlpHeader, 0, 0);
        _tlpMain.Controls.Add(_dgvModules, 0, 1);
        _tlpMain.Controls.Add(_prgProgress, 0, 2);
        _tlpMain.Controls.Add(_rtbLog, 0, 3);
        _tlpMain.Controls.Add(_flpButtons, 0, 4);

        // _tlpHeader (config info + status)
        _tlpHeader.ColumnCount = 2;
        _tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
        _tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
        _tlpHeader.RowCount = 2;
        _tlpHeader.RowStyles.Add(new RowStyle());
        _tlpHeader.RowStyles.Add(new RowStyle());
        _tlpHeader.AutoSize = true;
        _tlpHeader.AutoSizeMode = AutoSizeMode.GrowOnly;
        _tlpHeader.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _tlpHeader.Margin = new Padding(0, 0, 0, 4);
        _tlpHeader.Name = "_tlpHeader";
        _tlpHeader.Controls.Add(_lblConfigInfo, 0, 0);
        _tlpHeader.Controls.Add(_lblStatusCaption, 1, 0);
        _tlpHeader.Controls.Add(_lblStatus, 1, 1);

        _lblConfigInfo.Text = "—";
        _lblConfigInfo.AutoSize = true;
        _lblConfigInfo.Font = new Font("Segoe UI Semibold", 10F);
        _lblConfigInfo.Margin = new Padding(0, 4, 0, 4);
        _lblConfigInfo.Name = "_lblConfigInfo";
        _tlpHeader.SetRowSpan(_lblConfigInfo, 2);
        _lblConfigInfo.Anchor = AnchorStyles.Left;

        _lblStatusCaption.Text = "DURUM";
        _lblStatusCaption.AutoSize = true;
        _lblStatusCaption.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
        _lblStatusCaption.ForeColor = SystemColors.GrayText;
        _lblStatusCaption.Margin = new Padding(0, 4, 0, 0);
        _lblStatusCaption.Anchor = AnchorStyles.Right;
        _lblStatusCaption.Name = "_lblStatusCaption";

        _lblStatus.Text = "Kontrol edilmedi";
        _lblStatus.AutoSize = true;
        _lblStatus.Font = new Font("Segoe UI Semibold", 12F);
        _lblStatus.Margin = new Padding(0, 0, 0, 4);
        _lblStatus.Anchor = AnchorStyles.Right;
        _lblStatus.Name = "_lblStatus";

        // _dgvModules
        _dgvModules.AllowUserToAddRows = false;
        _dgvModules.AllowUserToDeleteRows = false;
        _dgvModules.AllowUserToResizeRows = false;
        _dgvModules.ReadOnly = true;
        _dgvModules.RowHeadersVisible = false;
        _dgvModules.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _dgvModules.MultiSelect = false;
        _dgvModules.BackgroundColor = Color.FromArgb(35, 35, 35);
        _dgvModules.GridColor = Color.FromArgb(50, 50, 50);
        _dgvModules.BorderStyle = BorderStyle.FixedSingle;
        _dgvModules.DefaultCellStyle.BackColor = Color.FromArgb(35, 35, 35);
        _dgvModules.DefaultCellStyle.ForeColor = Color.FromArgb(210, 210, 210);
        _dgvModules.DefaultCellStyle.SelectionBackColor = Color.FromArgb(50, 50, 50);
        _dgvModules.DefaultCellStyle.SelectionForeColor = Color.FromArgb(230, 230, 230);
        _dgvModules.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
        _dgvModules.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(40, 40, 40);
        _dgvModules.ColumnHeadersDefaultCellStyle.ForeColor = SystemColors.GrayText;
        _dgvModules.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
        _dgvModules.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(40, 40, 40);
        _dgvModules.ColumnHeadersDefaultCellStyle.SelectionForeColor = SystemColors.GrayText;
        _dgvModules.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        _dgvModules.EnableHeadersVisualStyles = false;
        _dgvModules.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        _dgvModules.Margin = new Padding(0, 0, 0, 4);
        _dgvModules.Name = "_dgvModules";
        _dgvModules.Columns.AddRange(new DataGridViewColumn[] { _colModuleName, _colLocalVersion, _colServerVersion, _colStatus });

        _colModuleName.HeaderText = "MODÜL";
        _colModuleName.Name = "_colModuleName";
        _colModuleName.Width = 110;
        _colModuleName.SortMode = DataGridViewColumnSortMode.NotSortable;

        _colLocalVersion.HeaderText = "TERMINAL";
        _colLocalVersion.Name = "_colLocalVersion";
        _colLocalVersion.Width = 130;
        _colLocalVersion.SortMode = DataGridViewColumnSortMode.NotSortable;

        _colServerVersion.HeaderText = "SUNUCU";
        _colServerVersion.Name = "_colServerVersion";
        _colServerVersion.Width = 130;
        _colServerVersion.SortMode = DataGridViewColumnSortMode.NotSortable;

        _colStatus.HeaderText = "DURUM";
        _colStatus.Name = "_colStatus";
        _colStatus.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        _colStatus.SortMode = DataGridViewColumnSortMode.NotSortable;

        // _prgProgress (slim)
        _prgProgress.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _prgProgress.Margin = new Padding(0, 2, 0, 6);
        _prgProgress.Size = new Size(0, 4);
        _prgProgress.Name = "_prgProgress";
        _prgProgress.TabIndex = 1;

        // _rtbLog (borderless, dark, directly in main layout)
        _rtbLog.AccessibleName = "İşlem günlüğü";
        _rtbLog.BackColor = Color.FromArgb(25, 25, 25);
        _rtbLog.BorderStyle = BorderStyle.None;
        _rtbLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        _rtbLog.Font = new Font("Cascadia Mono", 8.5F);
        _rtbLog.ForeColor = Color.FromArgb(210, 210, 210);
        _rtbLog.Name = "_rtbLog";
        _rtbLog.ReadOnly = true;
        _rtbLog.TabIndex = 2;
        _rtbLog.Text = "";

        // _flpButtons
        _flpButtons.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _flpButtons.AutoSize = true;
        _flpButtons.AutoSizeMode = AutoSizeMode.GrowOnly;
        _flpButtons.FlowDirection = FlowDirection.RightToLeft;
        _flpButtons.MinimumSize = new Size(0, 40);
        _flpButtons.BackColor = Color.FromArgb(35, 35, 35);
        _flpButtons.Name = "_flpButtons";
        _flpButtons.Padding = new Padding(0, 6, 0, 0);
        _flpButtons.TabIndex = 3;
        _flpButtons.Controls.Add(_btnLaunch);
        _flpButtons.Controls.Add(_btnUpdate);
        _flpButtons.Controls.Add(_btnCheck);
        _flpButtons.Controls.Add(_btnSettings);
        _flpButtons.Controls.Add(_btnAbout);

        _btnLaunch.AccessibleName = "Mikro'yu Başlat";
        _btnLaunch.AutoSize = true;
        _btnLaunch.FlatStyle = FlatStyle.Flat;
        _btnLaunch.FlatAppearance.BorderSize = 0;
        _btnLaunch.FlatAppearance.BorderColor = Color.FromArgb(0, 120, 60);
        _btnLaunch.BackColor = Color.FromArgb(0, 150, 80);
        _btnLaunch.ForeColor = Color.White;
        _btnLaunch.Font = new Font("Segoe UI Semibold", 9F);
        _btnLaunch.Name = "_btnLaunch";
        _btnLaunch.Padding = new Padding(12, 4, 12, 4);
        _btnLaunch.TabIndex = 7;
        _btnLaunch.Text = "▶  Başlat";
        _btnLaunch.Click += BtnLaunch_Click;

        _btnUpdate.AccessibleName = "Güncelleme başlat";
        _btnUpdate.AutoSize = true;
        _btnUpdate.FlatStyle = FlatStyle.Flat;
        _btnUpdate.FlatAppearance.BorderSize = 1;
        _btnUpdate.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 80);
        _btnUpdate.BackColor = Color.FromArgb(50, 50, 50);
        _btnUpdate.ForeColor = Color.FromArgb(230, 230, 230);
        _btnUpdate.Name = "_btnUpdate";
        _btnUpdate.Padding = new Padding(10, 4, 10, 4);
        _btnUpdate.TabIndex = 6;
        _btnUpdate.Text = "Güncelle";
        _btnUpdate.Click += BtnUpdate_Click;

        _btnCheck.AccessibleName = "Versiyon kontrol et";
        _btnCheck.AutoSize = true;
        _btnCheck.FlatStyle = FlatStyle.Flat;
        _btnCheck.FlatAppearance.BorderSize = 1;
        _btnCheck.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 80);
        _btnCheck.BackColor = Color.FromArgb(50, 50, 50);
        _btnCheck.ForeColor = Color.FromArgb(230, 230, 230);
        _btnCheck.Name = "_btnCheck";
        _btnCheck.Padding = new Padding(10, 4, 10, 4);
        _btnCheck.TabIndex = 5;
        _btnCheck.Text = "Kontrol Et";
        _btnCheck.Click += BtnCheck_Click;

        _btnSettings.AccessibleName = "Ayarları aç";
        _btnSettings.AutoSize = true;
        _btnSettings.FlatStyle = FlatStyle.Flat;
        _btnSettings.FlatAppearance.BorderSize = 1;
        _btnSettings.FlatAppearance.BorderColor = Color.FromArgb(60, 60, 60);
        _btnSettings.ForeColor = Color.FromArgb(180, 180, 180);
        _btnSettings.Name = "_btnSettings";
        _btnSettings.Padding = new Padding(6, 4, 6, 4);
        _btnSettings.TabIndex = 4;
        _btnSettings.Text = "⚙  Ayarlar";
        _btnSettings.Click += BtnSettings_Click;

        _btnAbout.AccessibleName = "Hakkında";
        _btnAbout.AutoSize = true;
        _btnAbout.FlatStyle = FlatStyle.Flat;
        _btnAbout.FlatAppearance.BorderSize = 1;
        _btnAbout.FlatAppearance.BorderColor = Color.FromArgb(60, 60, 60);
        _btnAbout.ForeColor = Color.FromArgb(180, 180, 180);
        _btnAbout.Name = "_btnAbout";
        _btnAbout.Padding = new Padding(6, 4, 6, 4);
        _btnAbout.TabIndex = 8;
        _btnAbout.Text = "ℹ  Hakkında";
        _btnAbout.Click += BtnAbout_Click;

        // 5. Configure Form
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(30, 30, 30);
        ForeColor = Color.FromArgb(230, 230, 230);
        ClientSize = new Size(720, 500);
        MinimumSize = new Size(620, 420);
        Controls.Add(_tlpMain);
        Name = "Form1";
        Text = "MikroUpdate";

        // 6. Resume
        _ctxTray.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)_dgvModules).EndInit();
        _tlpHeader.ResumeLayout(false);
        _tlpHeader.PerformLayout();
        _flpButtons.ResumeLayout(false);
        _flpButtons.PerformLayout();
        _tlpMain.ResumeLayout(false);
        _tlpMain.PerformLayout();
        ResumeLayout(false);
    }

    #endregion

    private TableLayoutPanel _tlpMain;
    private TableLayoutPanel _tlpHeader;
    private Label _lblConfigInfo;
    private Label _lblStatusCaption;
    private Label _lblStatus;
    private DataGridView _dgvModules;
    private DataGridViewTextBoxColumn _colModuleName;
    private DataGridViewTextBoxColumn _colLocalVersion;
    private DataGridViewTextBoxColumn _colServerVersion;
    private DataGridViewTextBoxColumn _colStatus;
    private ProgressBar _prgProgress;
    private RichTextBox _rtbLog;
    private FlowLayoutPanel _flpButtons;
    private Button _btnSettings;
    private Button _btnCheck;
    private Button _btnUpdate;
    private Button _btnLaunch;
    private NotifyIcon _notifyIcon;
    private ContextMenuStrip _ctxTray;
    private ToolStripMenuItem _tsmShow;
    private ToolStripMenuItem _tsmCheck;
    private ToolStripMenuItem _tsmUpdate;
    private ToolStripMenuItem _tsmSettings;
    private ToolStripMenuItem _tsmAbout;
    private ToolStripSeparator _tsmSepUpdate;
    private ToolStripMenuItem _tsmSelfUpdate;
    private ToolStripSeparator _tsmSeparator;
    private ToolStripMenuItem _tsmExit;
    private Button _btnAbout;
}

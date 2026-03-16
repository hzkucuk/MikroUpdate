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
        _grpStatus = new GroupBox();
        _tlpStatus = new TableLayoutPanel();
        _lblLocalVerCaption = new Label();
        _lblLocalVersion = new Label();
        _lblServerVerCaption = new Label();
        _lblServerVersion = new Label();
        _lblStatusCaption = new Label();
        _lblStatus = new Label();
        _prgProgress = new ProgressBar();
        _grpLog = new GroupBox();
        _rtbLog = new RichTextBox();
        _flpButtons = new FlowLayoutPanel();
        _btnLaunch = new Button();
        _btnUpdate = new Button();
        _btnCheck = new Button();
        _btnSettings = new Button();
        _notifyIcon = new NotifyIcon(components);
        _ctxTray = new ContextMenuStrip(components);
        _tsmShow = new ToolStripMenuItem();
        _tsmCheck = new ToolStripMenuItem();
        _tsmUpdate = new ToolStripMenuItem();
        _tsmSettings = new ToolStripMenuItem();
        _tsmSeparator = new ToolStripSeparator();
        _tsmExit = new ToolStripMenuItem();

        // 3. Suspend
        _tlpMain.SuspendLayout();
        _grpStatus.SuspendLayout();
        _tlpStatus.SuspendLayout();
        _grpLog.SuspendLayout();
        _flpButtons.SuspendLayout();
        _ctxTray.SuspendLayout();
        SuspendLayout();

        // 4. Configure controls

        // _ctxTray
        _ctxTray.Items.AddRange(new ToolStripItem[] { _tsmShow, _tsmCheck, _tsmUpdate, _tsmSettings, _tsmSeparator, _tsmExit });
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

        // _tlpMain (4 rows: Status, Progress, Log, Buttons)
        _tlpMain.ColumnCount = 1;
        _tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _tlpMain.RowCount = 4;
        _tlpMain.RowStyles.Add(new RowStyle());
        _tlpMain.RowStyles.Add(new RowStyle());
        _tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _tlpMain.RowStyles.Add(new RowStyle());
        _tlpMain.Dock = DockStyle.Fill;
        _tlpMain.Padding = new Padding(6);
        _tlpMain.Name = "_tlpMain";
        _tlpMain.Controls.Add(_grpStatus, 0, 0);
        _tlpMain.Controls.Add(_prgProgress, 0, 1);
        _tlpMain.Controls.Add(_grpLog, 0, 2);
        _tlpMain.Controls.Add(_flpButtons, 0, 3);

        // _grpStatus
        _grpStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _grpStatus.AutoSize = true;
        _grpStatus.AutoSizeMode = AutoSizeMode.GrowOnly;
        _grpStatus.Controls.Add(_tlpStatus);
        _grpStatus.Name = "_grpStatus";
        _grpStatus.Padding = new Padding(8);
        _grpStatus.TabIndex = 0;
        _grpStatus.TabStop = false;
        _grpStatus.Text = "Versiyon Durumu";

        // _tlpStatus
        _tlpStatus.ColumnCount = 3;
        _tlpStatus.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        _tlpStatus.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));
        _tlpStatus.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        _tlpStatus.RowCount = 2;
        _tlpStatus.RowStyles.Add(new RowStyle());
        _tlpStatus.RowStyles.Add(new RowStyle());
        _tlpStatus.Dock = DockStyle.Fill;
        _tlpStatus.Name = "_tlpStatus";
        _tlpStatus.Controls.Add(_lblLocalVerCaption, 0, 0);
        _tlpStatus.Controls.Add(_lblLocalVersion, 0, 1);
        _tlpStatus.Controls.Add(_lblServerVerCaption, 1, 0);
        _tlpStatus.Controls.Add(_lblServerVersion, 1, 1);
        _tlpStatus.Controls.Add(_lblStatusCaption, 2, 0);
        _tlpStatus.Controls.Add(_lblStatus, 2, 1);

        _lblLocalVerCaption.AutoSize = true;
        _lblLocalVerCaption.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        _lblLocalVerCaption.Name = "_lblLocalVerCaption";
        _lblLocalVerCaption.Text = "Terminal Versiyonu:";

        _lblLocalVersion.AutoSize = true;
        _lblLocalVersion.Name = "_lblLocalVersion";
        _lblLocalVersion.Text = "---";

        _lblServerVerCaption.AutoSize = true;
        _lblServerVerCaption.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        _lblServerVerCaption.Name = "_lblServerVerCaption";
        _lblServerVerCaption.Text = "Sunucu Versiyonu:";

        _lblServerVersion.AutoSize = true;
        _lblServerVersion.Name = "_lblServerVersion";
        _lblServerVersion.Text = "---";

        _lblStatusCaption.AutoSize = true;
        _lblStatusCaption.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        _lblStatusCaption.Name = "_lblStatusCaption";
        _lblStatusCaption.Text = "Durum:";

        _lblStatus.AutoSize = true;
        _lblStatus.Name = "_lblStatus";
        _lblStatus.Text = "Kontrol edilmedi";

        // _prgProgress
        _prgProgress.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _prgProgress.Name = "_prgProgress";
        _prgProgress.TabIndex = 1;

        // _grpLog
        _grpLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        _grpLog.Controls.Add(_rtbLog);
        _grpLog.Name = "_grpLog";
        _grpLog.Padding = new Padding(8);
        _grpLog.TabIndex = 2;
        _grpLog.TabStop = false;
        _grpLog.Text = "İşlem Günlüğü";

        // _rtbLog
        _rtbLog.AccessibleName = "İşlem günlüğü";
        _rtbLog.BackColor = Color.FromArgb(30, 30, 30);
        _rtbLog.BorderStyle = BorderStyle.None;
        _rtbLog.Dock = DockStyle.Fill;
        _rtbLog.Font = new Font("Consolas", 9F);
        _rtbLog.ForeColor = Color.White;
        _rtbLog.Name = "_rtbLog";
        _rtbLog.ReadOnly = true;
        _rtbLog.Text = "";

        // _flpButtons
        _flpButtons.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _flpButtons.AutoSize = true;
        _flpButtons.AutoSizeMode = AutoSizeMode.GrowOnly;
        _flpButtons.FlowDirection = FlowDirection.RightToLeft;
        _flpButtons.Name = "_flpButtons";
        _flpButtons.Padding = new Padding(0, 4, 0, 4);
        _flpButtons.TabIndex = 3;
        _flpButtons.Controls.Add(_btnLaunch);
        _flpButtons.Controls.Add(_btnUpdate);
        _flpButtons.Controls.Add(_btnCheck);
        _flpButtons.Controls.Add(_btnSettings);

        _btnLaunch.AccessibleName = "Mikro'yu Başlat";
        _btnLaunch.AutoSize = true;
        _btnLaunch.Name = "_btnLaunch";
        _btnLaunch.Padding = new Padding(8, 2, 8, 2);
        _btnLaunch.TabIndex = 7;
        _btnLaunch.Text = "Mikro'yu Başlat";
        _btnLaunch.Click += BtnLaunch_Click;

        _btnUpdate.AccessibleName = "Güncelleme başlat";
        _btnUpdate.AutoSize = true;
        _btnUpdate.Name = "_btnUpdate";
        _btnUpdate.Padding = new Padding(8, 2, 8, 2);
        _btnUpdate.TabIndex = 6;
        _btnUpdate.Text = "Güncelle";
        _btnUpdate.Click += BtnUpdate_Click;

        _btnCheck.AccessibleName = "Versiyon kontrol et";
        _btnCheck.AutoSize = true;
        _btnCheck.Name = "_btnCheck";
        _btnCheck.Padding = new Padding(8, 2, 8, 2);
        _btnCheck.TabIndex = 5;
        _btnCheck.Text = "Kontrol Et";
        _btnCheck.Click += BtnCheck_Click;

        _btnSettings.AccessibleName = "Ayarları aç";
        _btnSettings.AutoSize = true;
        _btnSettings.Name = "_btnSettings";
        _btnSettings.Padding = new Padding(8, 2, 8, 2);
        _btnSettings.TabIndex = 4;
        _btnSettings.Text = "Ayarlar";
        _btnSettings.Click += BtnSettings_Click;

        // 5. Configure Form
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(850, 500);
        MinimumSize = new Size(700, 400);
        Controls.Add(_tlpMain);
        Name = "Form1";
        Text = "MikroUpdate - Otomatik Güncelleme Sistemi";

        // 6. Resume
        _ctxTray.ResumeLayout(false);
        _tlpStatus.ResumeLayout(false);
        _tlpStatus.PerformLayout();
        _grpStatus.ResumeLayout(false);
        _grpStatus.PerformLayout();
        _grpLog.ResumeLayout(false);
        _flpButtons.ResumeLayout(false);
        _flpButtons.PerformLayout();
        _tlpMain.ResumeLayout(false);
        _tlpMain.PerformLayout();
        ResumeLayout(false);
    }

    #endregion

    private TableLayoutPanel _tlpMain;
    private GroupBox _grpStatus;
    private TableLayoutPanel _tlpStatus;
    private Label _lblLocalVerCaption;
    private Label _lblLocalVersion;
    private Label _lblServerVerCaption;
    private Label _lblServerVersion;
    private Label _lblStatusCaption;
    private Label _lblStatus;
    private ProgressBar _prgProgress;
    private GroupBox _grpLog;
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
    private ToolStripSeparator _tsmSeparator;
    private ToolStripMenuItem _tsmExit;
}



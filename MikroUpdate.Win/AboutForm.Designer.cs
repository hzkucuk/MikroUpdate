namespace MikroUpdate.Win;

partial class AboutForm
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
        _lblAppName = new Label();
        _lblVersion = new Label();
        _lblDescription = new Label();
        _tlpInfo = new TableLayoutPanel();
        _lblDeveloperCaption = new Label();
        _lblDeveloper = new Label();
        _lblLicenseCaption = new Label();
        _lblLicense = new Label();
        _lblTechCaption = new Label();
        _lblTech = new Label();
        _lblGitHubCaption = new Label();
        _lnkGitHub = new LinkLabel();
        _lnkEmail = new LinkLabel();
        _btnClose = new Button();

        // 3. Suspend
        _tlpMain.SuspendLayout();
        _tlpInfo.SuspendLayout();
        SuspendLayout();

        // 4. Configure controls

        // _tlpMain (7 rows: AppName, Version, Description, InfoGrid, spacer, Email+GitHub, CloseButton)
        _tlpMain.ColumnCount = 1;
        _tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _tlpMain.RowCount = 7;
        _tlpMain.RowStyles.Add(new RowStyle());
        _tlpMain.RowStyles.Add(new RowStyle());
        _tlpMain.RowStyles.Add(new RowStyle());
        _tlpMain.RowStyles.Add(new RowStyle());
        _tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _tlpMain.RowStyles.Add(new RowStyle());
        _tlpMain.RowStyles.Add(new RowStyle());
        _tlpMain.Dock = DockStyle.Fill;
        _tlpMain.Padding = new Padding(24, 20, 24, 16);
        _tlpMain.Name = "_tlpMain";
        _tlpMain.Controls.Add(_lblAppName, 0, 0);
        _tlpMain.Controls.Add(_lblVersion, 0, 1);
        _tlpMain.Controls.Add(_lblDescription, 0, 2);
        _tlpMain.Controls.Add(_tlpInfo, 0, 3);
        _tlpMain.Controls.Add(_lnkEmail, 0, 5);
        _tlpMain.Controls.Add(_btnClose, 0, 6);

        // _lblAppName
        _lblAppName.AutoSize = true;
        _lblAppName.Font = new Font("Segoe UI Semibold", 18F);
        _lblAppName.ForeColor = Color.FromArgb(100, 180, 255);
        _lblAppName.Margin = new Padding(0, 0, 0, 2);
        _lblAppName.Name = "_lblAppName";
        _lblAppName.Text = "MikroUpdate";

        // _lblVersion
        _lblVersion.AutoSize = true;
        _lblVersion.Font = new Font("Segoe UI", 10F);
        _lblVersion.ForeColor = SystemColors.GrayText;
        _lblVersion.Margin = new Padding(0, 0, 0, 12);
        _lblVersion.Name = "_lblVersion";
        _lblVersion.Text = "v1.9.0";

        // _lblDescription
        _lblDescription.AutoSize = true;
        _lblDescription.Font = new Font("Segoe UI", 9.5F);
        _lblDescription.ForeColor = Color.FromArgb(210, 210, 210);
        _lblDescription.Margin = new Padding(0, 0, 0, 16);
        _lblDescription.MaximumSize = new Size(340, 0);
        _lblDescription.Name = "_lblDescription";
        _lblDescription.Text = "Mikro ERP (Jump / Fly) yaz\u0131l\u0131mlar\u0131 i\u00e7in domain ortam\u0131nda \u00e7al\u0131\u015fan otomatik g\u00fcncelleme sistemi.";

        // _tlpInfo (4 rows x 2 cols: Developer, License, Tech, GitHub)
        _tlpInfo.ColumnCount = 2;
        _tlpInfo.ColumnStyles.Add(new ColumnStyle());
        _tlpInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _tlpInfo.RowCount = 4;
        _tlpInfo.RowStyles.Add(new RowStyle());
        _tlpInfo.RowStyles.Add(new RowStyle());
        _tlpInfo.RowStyles.Add(new RowStyle());
        _tlpInfo.RowStyles.Add(new RowStyle());
        _tlpInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _tlpInfo.Margin = new Padding(0, 0, 0, 8);
        _tlpInfo.Name = "_tlpInfo";
        _tlpInfo.Controls.Add(_lblDeveloperCaption, 0, 0);
        _tlpInfo.Controls.Add(_lblDeveloper, 1, 0);
        _tlpInfo.Controls.Add(_lblLicenseCaption, 0, 1);
        _tlpInfo.Controls.Add(_lblLicense, 1, 1);
        _tlpInfo.Controls.Add(_lblTechCaption, 0, 2);
        _tlpInfo.Controls.Add(_lblTech, 1, 2);
        _tlpInfo.Controls.Add(_lblGitHubCaption, 0, 3);
        _tlpInfo.Controls.Add(_lnkGitHub, 1, 3);

        // Developer
        _lblDeveloperCaption.AutoSize = true;
        _lblDeveloperCaption.Font = new Font("Segoe UI Semibold", 9F);
        _lblDeveloperCaption.ForeColor = SystemColors.GrayText;
        _lblDeveloperCaption.Margin = new Padding(0, 4, 12, 4);
        _lblDeveloperCaption.Name = "_lblDeveloperCaption";
        _lblDeveloperCaption.Text = "Geli\u015ftirici:";

        _lblDeveloper.AutoSize = true;
        _lblDeveloper.Font = new Font("Segoe UI", 9F);
        _lblDeveloper.ForeColor = Color.FromArgb(230, 230, 230);
        _lblDeveloper.Margin = new Padding(0, 4, 0, 4);
        _lblDeveloper.Name = "_lblDeveloper";
        _lblDeveloper.Text = "H\u00fcseyin K\u00fc\u00e7\u00fck";

        // License
        _lblLicenseCaption.AutoSize = true;
        _lblLicenseCaption.Font = new Font("Segoe UI Semibold", 9F);
        _lblLicenseCaption.ForeColor = SystemColors.GrayText;
        _lblLicenseCaption.Margin = new Padding(0, 4, 12, 4);
        _lblLicenseCaption.Name = "_lblLicenseCaption";
        _lblLicenseCaption.Text = "Lisans:";

        _lblLicense.AutoSize = true;
        _lblLicense.Font = new Font("Segoe UI", 9F);
        _lblLicense.ForeColor = Color.FromArgb(230, 230, 230);
        _lblLicense.Margin = new Padding(0, 4, 0, 4);
        _lblLicense.Name = "_lblLicense";
        _lblLicense.Text = "MIT Lisans\u0131";

        // Technologies
        _lblTechCaption.AutoSize = true;
        _lblTechCaption.Font = new Font("Segoe UI Semibold", 9F);
        _lblTechCaption.ForeColor = SystemColors.GrayText;
        _lblTechCaption.Margin = new Padding(0, 4, 12, 4);
        _lblTechCaption.Name = "_lblTechCaption";
        _lblTechCaption.Text = "Teknoloji:";

        _lblTech.AutoSize = true;
        _lblTech.Font = new Font("Segoe UI", 9F);
        _lblTech.ForeColor = Color.FromArgb(230, 230, 230);
        _lblTech.Margin = new Padding(0, 4, 0, 4);
        _lblTech.MaximumSize = new Size(260, 0);
        _lblTech.Name = "_lblTech";
        _lblTech.Text = ".NET 10  \u2022  WinForms  \u2022  Worker Service\r\nNamed Pipes (IPC)  \u2022  Inno Setup";

        // GitHub
        _lblGitHubCaption.AutoSize = true;
        _lblGitHubCaption.Font = new Font("Segoe UI Semibold", 9F);
        _lblGitHubCaption.ForeColor = SystemColors.GrayText;
        _lblGitHubCaption.Margin = new Padding(0, 4, 12, 4);
        _lblGitHubCaption.Name = "_lblGitHubCaption";
        _lblGitHubCaption.Text = "GitHub:";

        _lnkGitHub.AutoSize = true;
        _lnkGitHub.Font = new Font("Segoe UI", 9F);
        _lnkGitHub.LinkColor = Color.FromArgb(100, 180, 255);
        _lnkGitHub.ActiveLinkColor = Color.FromArgb(140, 200, 255);
        _lnkGitHub.VisitedLinkColor = Color.FromArgb(100, 180, 255);
        _lnkGitHub.Margin = new Padding(0, 4, 0, 4);
        _lnkGitHub.Name = "_lnkGitHub";
        _lnkGitHub.Text = "github.com/hzkucuk/MikroUpdate";
        _lnkGitHub.LinkClicked += LnkGitHub_LinkClicked;

        // _lnkEmail
        _lnkEmail.AutoSize = true;
        _lnkEmail.Font = new Font("Segoe UI", 8.5F);
        _lnkEmail.LinkColor = Color.FromArgb(160, 160, 160);
        _lnkEmail.ActiveLinkColor = Color.FromArgb(200, 200, 200);
        _lnkEmail.VisitedLinkColor = Color.FromArgb(160, 160, 160);
        _lnkEmail.Margin = new Padding(0, 0, 0, 8);
        _lnkEmail.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        _lnkEmail.Name = "_lnkEmail";
        _lnkEmail.Text = "hzkucuk@gmail.com";
        _lnkEmail.LinkClicked += LnkEmail_LinkClicked;

        // _btnClose
        _btnClose.AccessibleName = "Kapat";
        _btnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        _btnClose.AutoSize = true;
        _btnClose.DialogResult = DialogResult.OK;
        _btnClose.FlatStyle = FlatStyle.Flat;
        _btnClose.FlatAppearance.BorderSize = 1;
        _btnClose.Font = new Font("Segoe UI", 9F);
        _btnClose.Padding = new Padding(16, 4, 16, 4);
        _btnClose.Margin = new Padding(0, 4, 0, 0);
        _btnClose.Name = "_btnClose";
        _btnClose.TabIndex = 0;
        _btnClose.Text = "Tamam";

        // 5. Configure Form
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(30, 30, 30);
        ForeColor = Color.FromArgb(230, 230, 230);
        ClientSize = new Size(400, 340);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        AcceptButton = _btnClose;
        CancelButton = _btnClose;
        Controls.Add(_tlpMain);
        Name = "AboutForm";
        Text = "Hakk\u0131nda — MikroUpdate";

        // 6. Resume
        _tlpInfo.ResumeLayout(false);
        _tlpInfo.PerformLayout();
        _tlpMain.ResumeLayout(false);
        _tlpMain.PerformLayout();
        ResumeLayout(false);
    }

    #endregion

    private TableLayoutPanel _tlpMain;
    private Label _lblAppName;
    private Label _lblVersion;
    private Label _lblDescription;
    private TableLayoutPanel _tlpInfo;
    private Label _lblDeveloperCaption;
    private Label _lblDeveloper;
    private Label _lblLicenseCaption;
    private Label _lblLicense;
    private Label _lblTechCaption;
    private Label _lblTech;
    private Label _lblGitHubCaption;
    private LinkLabel _lnkGitHub;
    private LinkLabel _lnkEmail;
    private Button _btnClose;
}

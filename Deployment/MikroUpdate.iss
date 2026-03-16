; ============================================================
;  MikroUpdate — Inno Setup Kurulum Scripti
;  Mikro ERP otomatik güncelleme sistemi
; ============================================================

#define MyAppName "MikroUpdate"
#define MyAppVersion "1.6.0"
#define MyAppPublisher "MikroUpdate"
#define MyAppURL "https://github.com/hzkucuk/MikroUpdate"
#define MyAppExeName "MikroUpdate.Win.exe"

[Setup]
AppId={{21CEB31C-1D57-42E1-B0D5-65536218CD0F}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputDir=..\installer
OutputBaseFilename=MikroUpdate_Setup_{#MyAppVersion}
Compression=lzma2/ultra64
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=admin
MinVersion=10.0
WizardStyle=modern
SetupIconFile=..\MikroUpdate.Win\app.ico
UninstallDisplayIcon={app}\Win\{#MyAppExeName}

[Languages]
Name: "turkish"; MessagesFile: "compiler:Languages\Turkish.isl"

; ============================================================
;  Özel Sayfa Parametreleri
; ============================================================
[Messages]
turkish.WelcomeLabel2=Bu sihirbaz bilgisayarınıza {#MyAppName} {#MyAppVersion} kurulumunu yapacaktır.%n%nMikro ERP (Jump / Fly) otomatik güncelleme sistemi.

; ============================================================
;  Kurulum Dosyaları
; ============================================================
[Files]
; Win Tray Uygulaması
Source: "..\publish\win\*"; DestDir: "{app}\Win"; Flags: ignoreversion recursesubdirs createallsubdirs

; Windows Servisi
Source: "..\publish\service\*"; DestDir: "{app}\Service"; Flags: ignoreversion recursesubdirs createallsubdirs

; ============================================================
;  Dizin Yapısı
; ============================================================
[Dirs]
Name: "{commonappdata}\MikroUpdate"; Permissions: everyone-modify
Name: "{commonappdata}\MikroUpdate\logs"; Permissions: everyone-modify

; ============================================================
;  Simgeler / Kısayollar
; ============================================================
[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\Win\{#MyAppExeName}"; WorkingDir: "{app}\Win"
Name: "{group}\{#MyAppName} Kaldır"; Filename: "{uninstallexe}"
Name: "{commonstartup}\{#MyAppName}"; Filename: "{app}\Win\{#MyAppExeName}"; Parameters: "/auto"; WorkingDir: "{app}\Win"; Tasks: startuptask

; ============================================================
;  Görevler (Tasks)
; ============================================================
[Tasks]
Name: "servicetask"; Description: "MikroUpdate Windows servisini kur ve başlat"; GroupDescription: "Ek görevler:"; Flags: checkedonce
Name: "startuptask"; Description: "Windows başlangıcında otomatik çalıştır"; GroupDescription: "Ek görevler:"; Flags: checkedonce

; ============================================================
;  Kurulum Sonrası: Servis Kaydı
; ============================================================
[Run]
; Servis kaydı (task seçiliyse)
Filename: "sc.exe"; Parameters: "create MikroUpdateService binPath=""{app}\Service\MikroUpdate.Service.exe"" start=auto DisplayName=""MikroUpdate Güncelleme Servisi"""; Flags: runhidden waituntilterminated; Tasks: servicetask
Filename: "sc.exe"; Parameters: "description MikroUpdateService ""Mikro ERP otomatik güncelleme servisi"""; Flags: runhidden waituntilterminated; Tasks: servicetask
Filename: "sc.exe"; Parameters: "start MikroUpdateService"; Flags: runhidden waituntilterminated; Tasks: servicetask

; Kurulum sonrası uygulamayı başlat (opsiyonel)
Filename: "{app}\Win\{#MyAppExeName}"; Description: "MikroUpdate'i şimdi başlat"; Flags: nowait postinstall skipifsilent unchecked

; ============================================================
;  Kaldırma: Servis Temizliği
; ============================================================
[UninstallRun]
Filename: "sc.exe"; Parameters: "stop MikroUpdateService"; Flags: runhidden waituntilterminated; RunOnceId: "StopService"
Filename: "sc.exe"; Parameters: "delete MikroUpdateService"; Flags: runhidden waituntilterminated; RunOnceId: "DeleteService"

; ============================================================
;  Registry (Uninstall bilgileri)
; ============================================================
[Registry]
Root: HKLM; Subkey: "Software\MikroUpdate"; ValueType: string; ValueName: "InstallDir"; ValueData: "{app}"; Flags: uninsdeletekey

; ============================================================
;  Pascal Script — Özel Sayfa: Ürün ve Sunucu Ayarları
; ============================================================
[Code]

var
  ConfigPage: TWizardPage;
  ProductCombo: TNewComboBox;
  ServerPathEdit: TNewEdit;
  LocalPathEdit: TNewEdit;
  SetupFilesPathEdit: TNewEdit;
  SetupFileNameEdit: TNewEdit;

procedure InitializeWizard;
var
  LabelProduct, LabelServer, LabelLocal, LabelSetupPath, LabelSetupFile: TNewStaticText;
  TopPos: Integer;
begin
  ConfigPage := CreateCustomPage(
    wpSelectTasks,
    'Mikro ERP Yapılandırması',
    'Güncelleme için ürün ve sunucu bilgilerini girin.');

  TopPos := 8;

  { Ürün Seçimi }
  LabelProduct := TNewStaticText.Create(ConfigPage);
  LabelProduct.Parent := ConfigPage.Surface;
  LabelProduct.Caption := 'Ürün:';
  LabelProduct.Top := TopPos;
  LabelProduct.Left := 0;

  ProductCombo := TNewComboBox.Create(ConfigPage);
  ProductCombo.Parent := ConfigPage.Surface;
  ProductCombo.Top := TopPos + 20;
  ProductCombo.Left := 0;
  ProductCombo.Width := ConfigPage.SurfaceWidth;
  ProductCombo.Style := csDropDownList;
  ProductCombo.Items.Add('Jump');
  ProductCombo.Items.Add('Fly');
  ProductCombo.ItemIndex := 0;

  TopPos := TopPos + 52;

  { Sunucu Paylaşım Yolu }
  LabelServer := TNewStaticText.Create(ConfigPage);
  LabelServer.Parent := ConfigPage.Surface;
  LabelServer.Caption := 'Sunucu Paylaşım Yolu (ör: \\SERVER\MikroV16xx):';
  LabelServer.Top := TopPos;
  LabelServer.Left := 0;

  ServerPathEdit := TNewEdit.Create(ConfigPage);
  ServerPathEdit.Parent := ConfigPage.Surface;
  ServerPathEdit.Top := TopPos + 20;
  ServerPathEdit.Left := 0;
  ServerPathEdit.Width := ConfigPage.SurfaceWidth;
  ServerPathEdit.Text := '\\SERVER\MikroV16xx';

  TopPos := TopPos + 52;

  { Terminal Kurulum Yolu }
  LabelLocal := TNewStaticText.Create(ConfigPage);
  LabelLocal.Parent := ConfigPage.Surface;
  LabelLocal.Caption := 'Terminal Kurulum Yolu (ör: C:\Mikro\v16xx):';
  LabelLocal.Top := TopPos;
  LabelLocal.Left := 0;

  LocalPathEdit := TNewEdit.Create(ConfigPage);
  LocalPathEdit.Parent := ConfigPage.Surface;
  LocalPathEdit.Top := TopPos + 20;
  LocalPathEdit.Left := 0;
  LocalPathEdit.Width := ConfigPage.SurfaceWidth;
  LocalPathEdit.Text := 'C:\Mikro\v16xx';

  TopPos := TopPos + 52;

  { Setup Dosyaları Yolu }
  LabelSetupPath := TNewStaticText.Create(ConfigPage);
  LabelSetupPath.Parent := ConfigPage.Surface;
  LabelSetupPath.Caption := 'Setup Dosyaları Yolu (ör: \\SERVER\MikroV16xx\CLIENT):';
  LabelSetupPath.Top := TopPos;
  LabelSetupPath.Left := 0;

  SetupFilesPathEdit := TNewEdit.Create(ConfigPage);
  SetupFilesPathEdit.Parent := ConfigPage.Surface;
  SetupFilesPathEdit.Top := TopPos + 20;
  SetupFilesPathEdit.Left := 0;
  SetupFilesPathEdit.Width := ConfigPage.SurfaceWidth;
  SetupFilesPathEdit.Text := '\\SERVER\MikroV16xx\CLIENT';

  TopPos := TopPos + 52;

  { Setup Dosya Adı }
  LabelSetupFile := TNewStaticText.Create(ConfigPage);
  LabelSetupFile.Parent := ConfigPage.Surface;
  LabelSetupFile.Caption := 'Setup Dosya Adı (ör: Jump_v16xx_Client_Setupx064.exe):';
  LabelSetupFile.Top := TopPos;
  LabelSetupFile.Left := 0;

  SetupFileNameEdit := TNewEdit.Create(ConfigPage);
  SetupFileNameEdit.Parent := ConfigPage.Surface;
  SetupFileNameEdit.Top := TopPos + 20;
  SetupFileNameEdit.Left := 0;
  SetupFileNameEdit.Width := ConfigPage.SurfaceWidth;
  SetupFileNameEdit.Text := 'Jump_v16xx_Client_Setupx064.exe';
end;

function GenerateConfigJson: String;
begin
  Result :=
    '{' + #13#10 +
    '  "ProductName": "' + ProductCombo.Items[ProductCombo.ItemIndex] + '",' + #13#10 +
    '  "ServerSharePath": "' + ServerPathEdit.Text + '",' + #13#10 +
    '  "LocalInstallPath": "' + LocalPathEdit.Text + '",' + #13#10 +
    '  "SetupFilesPath": "' + SetupFilesPathEdit.Text + '",' + #13#10 +
    '  "SetupFileName": "' + SetupFileNameEdit.Text + '",' + #13#10 +
    '  "AutoLaunchAfterUpdate": true,' + #13#10 +
    '  "CheckIntervalMinutes": 30' + #13#10 +
    '}';
end;

procedure WriteConfigFile;
var
  ConfigDir, ConfigPath, JsonContent: String;
begin
  ConfigDir := ExpandConstant('{commonappdata}\MikroUpdate');
  ConfigPath := ConfigDir + '\config.json';

  { Dizin zaten [Dirs] bölümünde oluşturuluyor }
  if not DirExists(ConfigDir) then
    ForceDirectories(ConfigDir);

  { Mevcut config varsa üzerine yazma }
  if not FileExists(ConfigPath) then
  begin
    JsonContent := GenerateConfigJson;
    SaveStringToFile(ConfigPath, JsonContent, False);
  end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    WriteConfigFile;
  end;
end;

{ Kaldırma sonrası ProgramData temizliği (opsiyonel) }
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usPostUninstall then
  begin
    if MsgBox('Yapılandırma ve log dosyaları kaldırılsın mı?'#13#10 +
              '(config.json ve log dosyaları)', mbConfirmation, MB_YESNO) = IDYES then
    begin
      DelTree(ExpandConstant('{commonappdata}\MikroUpdate'), True, True, True);
    end;
  end;
end;

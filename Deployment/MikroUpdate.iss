; ============================================================
;  MikroUpdate — Inno Setup Kurulum Scripti
;  Mikro ERP otomatik güncelleme sistemi
; ============================================================

#define MyAppName "MikroUpdate"
#define MyAppVersion "1.18.4"
#define MyAppPublisher "MikroUpdate"
#define MyAppURL "https://github.com/hzkucuk/MikroUpdate"
#define MyAppExeName "MikroUpdate.exe"

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
CloseApplications=force
RestartApplications=yes
SetupIconFile=..\MikroUpdate.Win\app.ico
UninstallDisplayIcon={app}\Win\{#MyAppExeName}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
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
Name: "startuptask"; Description: "Windows başlangıcında otomatik çalıştır"; GroupDescription: "Ek görevler:"; Flags: checkedonce

; ============================================================
;  Kurulum Sonrası: Uygulama Başlatma
; ============================================================
[Run]
; Servis kurulumu CurStepChanged(ssPostInstall) içinde yapılır (zorunlu, her kurulumda)

; Kurulum sonrası uygulamayı başlat (interaktif kurulum — kullanıcı seçer)
Filename: "{app}\Win\{#MyAppExeName}"; Description: "MikroUpdate'i şimdi başlat"; Flags: nowait postinstall skipifsilent unchecked

; Sessiz kurulum sonrası uygulamayı otomatik başlat (self-update için)
Filename: "{app}\Win\{#MyAppExeName}"; Flags: nowait postinstall skipifnotsilent runasoriginaluser

; ============================================================
;  Kaldırma: Süreç ve Servis Temizliği
; ============================================================
[UninstallRun]
; Önce tray uygulamasını kapat
Filename: "taskkill.exe"; Parameters: "/F /IM MikroUpdate.exe"; Flags: runhidden waituntilterminated; RunOnceId: "KillTrayApp"
; Servisi durdur, bekleme sonrası sil
Filename: "sc.exe"; Parameters: "stop MikroUpdateService"; Flags: runhidden waituntilterminated; RunOnceId: "StopService"
Filename: "cmd.exe"; Parameters: "/C timeout /T 3 /NOBREAK >nul"; Flags: runhidden waituntilterminated; RunOnceId: "WaitAfterStop"
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
  MajorVersionCombo: TNewComboBox;
  ProductCombo: TNewComboBox;
  UpdateModeCombo: TNewComboBox;
  ServerPathEdit: TNewEdit;
  LocalPathEdit: TNewEdit;
  SetupFilesPathEdit: TNewEdit;
  ChkClient: TNewCheckBox;
  ChkEDefter: TNewCheckBox;
  ChkBeyanname: TNewCheckBox;

procedure OnMajorVersionChange(Sender: TObject);
var
  Ver, OldTag, NewTag, OldMikro, NewMikro, Tmp: String;
begin
  Ver := MajorVersionCombo.Items[MajorVersionCombo.ItemIndex];

  if Ver = 'V17' then
  begin
    OldTag := 'v16xx';
    NewTag := 'v17xx';
    OldMikro := 'MikroV16xx';
    NewMikro := 'MikroV17xx';
  end
  else
  begin
    OldTag := 'v17xx';
    NewTag := 'v16xx';
    OldMikro := 'MikroV17xx';
    NewMikro := 'MikroV16xx';
  end;

  { Sunucu yolunu güncelle }
  Tmp := ServerPathEdit.Text;
  StringChangeEx(Tmp, OldMikro, NewMikro, True);
  StringChangeEx(Tmp, OldTag, NewTag, True);
  ServerPathEdit.Text := Tmp;

  { Terminal yolunu güncelle }
  Tmp := LocalPathEdit.Text;
  StringChangeEx(Tmp, OldTag, NewTag, True);
  LocalPathEdit.Text := Tmp;

  { Setup dosyaları yolunu güncelle }
  Tmp := SetupFilesPathEdit.Text;
  StringChangeEx(Tmp, OldMikro, NewMikro, True);
  StringChangeEx(Tmp, OldTag, NewTag, True);
  SetupFilesPathEdit.Text := Tmp;
end;

procedure InitializeWizard;
var
  LabelMajorVersion, LabelProduct, LabelServer, LabelLocal, LabelSetupPath: TNewStaticText;
  TopPos: Integer;
begin
  ConfigPage := CreateCustomPage(
    wpSelectTasks,
    'Mikro ERP Yapılandırması',
    'Güncelleme için sürüm, ürün ve sunucu bilgilerini girin.');

  TopPos := 8;

  { Ana Sürüm Seçimi }
  LabelMajorVersion := TNewStaticText.Create(ConfigPage);
  LabelMajorVersion.Parent := ConfigPage.Surface;
  LabelMajorVersion.Caption := 'Ana Sürüm:';
  LabelMajorVersion.Top := TopPos;
  LabelMajorVersion.Left := 0;

  MajorVersionCombo := TNewComboBox.Create(ConfigPage);
  MajorVersionCombo.Parent := ConfigPage.Surface;
  MajorVersionCombo.Top := TopPos + 20;
  MajorVersionCombo.Left := 0;
  MajorVersionCombo.Width := ConfigPage.SurfaceWidth div 2;
  MajorVersionCombo.Style := csDropDownList;
  MajorVersionCombo.Items.Add('V16');
  MajorVersionCombo.Items.Add('V17');
  MajorVersionCombo.ItemIndex := 0;
  MajorVersionCombo.OnChange := @OnMajorVersionChange;

  { Ürün Seçimi (sürüm yanında) }
  LabelProduct := TNewStaticText.Create(ConfigPage);
  LabelProduct.Parent := ConfigPage.Surface;
  LabelProduct.Caption := 'Ürün:';
  LabelProduct.Top := TopPos;
  LabelProduct.Left := (ConfigPage.SurfaceWidth div 2) + 12;

  ProductCombo := TNewComboBox.Create(ConfigPage);
  ProductCombo.Parent := ConfigPage.Surface;
  ProductCombo.Top := TopPos + 20;
  ProductCombo.Left := (ConfigPage.SurfaceWidth div 2) + 12;
  ProductCombo.Width := (ConfigPage.SurfaceWidth div 2) - 12;
  ProductCombo.Style := csDropDownList;
  ProductCombo.Items.Add('Jump');
  ProductCombo.Items.Add('Fly');
  ProductCombo.ItemIndex := 0;

  TopPos := TopPos + 52;

  { Güncelleme Modu Seçimi }
  with TNewStaticText.Create(ConfigPage) do
  begin
    Parent := ConfigPage.Surface;
    Caption := 'Güncelleme Modu:';
    Top := TopPos;
    Left := 0;
  end;

  UpdateModeCombo := TNewComboBox.Create(ConfigPage);
  UpdateModeCombo.Parent := ConfigPage.Surface;
  UpdateModeCombo.Top := TopPos + 20;
  UpdateModeCombo.Left := 0;
  UpdateModeCombo.Width := ConfigPage.SurfaceWidth;
  UpdateModeCombo.Style := csDropDownList;
  UpdateModeCombo.Items.Add('Local');
  UpdateModeCombo.Items.Add('Online');
  UpdateModeCombo.Items.Add('Hybrid');
  UpdateModeCombo.ItemIndex := 0;  { Varsayılan: Local }

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

  { Modül Seçimi }
  with TNewStaticText.Create(ConfigPage) do
  begin
    Parent := ConfigPage.Surface;
    Caption := 'Güncellenecek Modüller:';
    Top := TopPos;
    Left := 0;
  end;

  TopPos := TopPos + 20;

  ChkClient := TNewCheckBox.Create(ConfigPage);
  ChkClient.Parent := ConfigPage.Surface;
  ChkClient.Top := TopPos;
  ChkClient.Left := 0;
  ChkClient.Width := ConfigPage.SurfaceWidth div 3;
  ChkClient.Caption := 'Client';
  ChkClient.Checked := True;
  ChkClient.Enabled := False;  { Client zorunlu modül }

  ChkEDefter := TNewCheckBox.Create(ConfigPage);
  ChkEDefter.Parent := ConfigPage.Surface;
  ChkEDefter.Top := TopPos;
  ChkEDefter.Left := ConfigPage.SurfaceWidth div 3;
  ChkEDefter.Width := ConfigPage.SurfaceWidth div 3;
  ChkEDefter.Caption := 'e-Defter';
  ChkEDefter.Checked := False;

  ChkBeyanname := TNewCheckBox.Create(ConfigPage);
  ChkBeyanname.Parent := ConfigPage.Surface;
  ChkBeyanname.Top := TopPos;
  ChkBeyanname.Left := (ConfigPage.SurfaceWidth div 3) * 2;
  ChkBeyanname.Width := ConfigPage.SurfaceWidth div 3;
  ChkBeyanname.Caption := 'Beyanname';
  ChkBeyanname.Checked := False;
end;

function BoolToStr(Value: Boolean): String;
begin
  if Value then
    Result := 'true'
  else
    Result := 'false';
end;

function GetModulesJson: String;
var
  MajorVer, Product, Prefix, Ver: String;
  IsFly: Boolean;
  ClientExe, EDeftExe: String;
begin
  MajorVer := MajorVersionCombo.Items[MajorVersionCombo.ItemIndex];
  Product := ProductCombo.Items[ProductCombo.ItemIndex];
  IsFly := (Product = 'Fly');

  if MajorVer = 'V17' then
    Ver := 'v17xx'
  else
    Ver := 'v16xx';

  if IsFly then
  begin
    Prefix := 'Fly';
    ClientExe := 'MikroFly.EXE';
    EDeftExe := 'MyeDefter.exe';
  end
  else
  begin
    Prefix := 'Jump';
    ClientExe := 'MikroJump.EXE';
    EDeftExe := 'myEDefterStandart.exe';
  end;

  Result :=
    '    {' + #13#10 +
    '      "Name": "Client",' + #13#10 +
    '      "SetupFileName": "' + Prefix + '_' + Ver + '_Client_Setupx064.exe",' + #13#10 +
    '      "ExeFileName": "' + ClientExe + '",' + #13#10 +
    '      "Enabled": true' + #13#10 +
    '    },' + #13#10 +
    '    {' + #13#10 +
    '      "Name": "e-Defter",' + #13#10 +
    '      "SetupFileName": "' + Prefix + '_' + Ver + '_eDefter_Setupx064.exe",' + #13#10 +
    '      "ExeFileName": "' + EDeftExe + '",' + #13#10 +
    '      "Enabled": ' + BoolToStr(ChkEDefter.Checked) + #13#10 +
    '    },' + #13#10 +
    '    {' + #13#10 +
    '      "Name": "Beyanname",' + #13#10 +
    '      "SetupFileName": "' + Ver + '_BEYANNAME_Setupx064.exe",' + #13#10 +
    '      "ExeFileName": "BEYANNAME.EXE",' + #13#10 +
    '      "Enabled": ' + BoolToStr(ChkBeyanname.Checked) + #13#10 +
    '    }';
end;

function GenerateConfigJson: String;
var
  UpdateMode: String;
begin
  UpdateMode := UpdateModeCombo.Items[UpdateModeCombo.ItemIndex];
  Result :=
    '{' + #13#10 +
    '  "MajorVersion": "' + MajorVersionCombo.Items[MajorVersionCombo.ItemIndex] + '",' + #13#10 +
    '  "ProductName": "' + ProductCombo.Items[ProductCombo.ItemIndex] + '",' + #13#10 +
    '  "ServerSharePath": "' + ServerPathEdit.Text + '",' + #13#10 +
    '  "LocalInstallPath": "' + LocalPathEdit.Text + '",' + #13#10 +
    '  "SetupFilesPath": "' + SetupFilesPathEdit.Text + '",' + #13#10 +
    '  "AutoLaunchAfterUpdate": true,' + #13#10 +
    '  "CheckIntervalMinutes": 30,' + #13#10 +
    '  "UpdateMode": "' + UpdateMode + '",' + #13#10 +
    '  "CdnBaseUrl": "https://cdn-mikro.atros.com.tr/mikro",' + #13#10 +
    '  "Modules": [' + #13#10 +
    GetModulesJson + #13#10 +
    '  ]' + #13#10 +
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

function IsServiceInstalled: Boolean;
var
  ResultCode: Integer;
begin
  { sc query returns 0 if service exists (running or stopped) }
  Result := Exec('sc.exe', 'query MikroUpdateService', '', SW_HIDE, ewWaitUntilTerminated, ResultCode)
            and (ResultCode = 0);
end;

procedure InstallAndStartService;
var
  ResultCode: Integer;
  BinPath: String;
begin
  BinPath := ExpandConstant('{app}\Service\MikroUpdate.Service.exe');

  if IsServiceInstalled then
  begin
    { Upgrade: servisi durdur, sil, yeniden oluştur (dosyalar güncellenmiş olabilir) }
    Log('MikroUpdateService zaten mevcut — yeniden oluşturuluyor...');
    Exec('sc.exe', 'stop MikroUpdateService', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
    Sleep(2000);
    Exec('sc.exe', 'delete MikroUpdateService', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
    Sleep(1000);
  end;

  { Servisi oluştur }
  Exec('sc.exe',
    Format('create MikroUpdateService binPath= "%s" start= auto DisplayName= "MikroUpdate Güncelleme Servisi"', [BinPath]),
    '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  Log(Format('sc create sonuç: %d', [ResultCode]));

  { Açıklamasını ayarla }
  Exec('sc.exe',
    'description MikroUpdateService "Mikro ERP otomatik güncelleme servisi"',
    '', SW_HIDE, ewWaitUntilTerminated, ResultCode);

  { Çökme durumunda otomatik yeniden başlatma: 5sn / 10sn / 30sn }
  Exec('sc.exe',
    'failure MikroUpdateService reset= 60 actions= restart/5000/restart/10000/restart/30000',
    '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  Log(Format('sc failure sonuç: %d', [ResultCode]));

  { Servisi başlat }
  Exec('sc.exe', 'start MikroUpdateService', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  Log(Format('sc start sonuç: %d', [ResultCode]));

  { Kurulum doğrulama }
  if IsServiceInstalled then
    Log('MikroUpdateService başarıyla kuruldu ve başlatıldı.')
  else
    Log('UYARI: MikroUpdateService kurulumu doğrulanamadı!');
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    WriteConfigFile;
    InstallAndStartService;
  end;
end;

{ ============================================================ }
{  .NET 10 Desktop Runtime Kontrolü ve Otomatik Kurulumu        }
{ ============================================================ }

function IsDotNet10DesktopInstalled: Boolean;
var
  BasePath: String;
  FindRec: TFindRec;
begin
  Result := False;
  BasePath := ExpandConstant('{commonpf64}\dotnet\shared\Microsoft.WindowsDesktop.App');
  if FindFirst(BasePath + '\10.*', FindRec) then
  begin
    try
      repeat
        if FindRec.Attributes and FILE_ATTRIBUTE_DIRECTORY <> 0 then
        begin
          Result := True;
          Break;
        end;
      until not FindNext(FindRec);
    finally
      FindClose(FindRec);
    end;
  end;
end;

function OnDownloadProgress(const Url, FileName: String; const Progress, ProgressMax: Int64): Boolean;
begin
  if ProgressMax <> 0 then
    Log(Format('  %s — %d / %d bayt (%d%%)', [FileName, Progress, ProgressMax, Progress * 100 div ProgressMax]))
  else
    Log(Format('  %s — %d bayt indiriliyor...', [FileName, Progress]));
  Result := True;
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
var
  DownloadPage: TDownloadWizardPage;
  ResultCode: Integer;
begin
  Result := '';

  if IsDotNet10DesktopInstalled then
  begin
    Log('.NET 10 Desktop Runtime zaten yüklü.');
    Exit;
  end;

  Log('.NET 10 Desktop Runtime bulunamadı — indirme başlatılıyor...');

  DownloadPage := CreateDownloadPage(
    '.NET 10 Desktop Runtime Gerekli',
    '.NET 10 Desktop Runtime indiriliyor, lütfen bekleyin...',
    @OnDownloadProgress);
  DownloadPage.Clear;
  DownloadPage.Add(
    'https://aka.ms/dotnet/10.0/windowsdesktop-runtime-win-x64.exe',
    'windowsdesktop-runtime-win-x64.exe',
    '');
  DownloadPage.Show;
  try
    try
      DownloadPage.Download;
    except
      Result := '.NET 10 Desktop Runtime indirilemedi: ' + GetExceptionMessage + #13#10 +
                'Lütfen internet bağlantınızı kontrol edip tekrar deneyin veya ' +
                'runtime''ı elle kurun: https://dotnet.microsoft.com/download/dotnet/10.0';
      Exit;
    end;
  finally
    DownloadPage.Hide;
  end;

  Log('.NET 10 Desktop Runtime sessiz kurulumu başlatılıyor...');

  if not Exec(
    ExpandConstant('{tmp}\windowsdesktop-runtime-win-x64.exe'),
    '/install /quiet /norestart', '',
    SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    Result := '.NET 10 Desktop Runtime başlatılamadı. Lütfen elle kurun: ' +
              'https://dotnet.microsoft.com/download/dotnet/10.0';
  end
  else if (ResultCode <> 0) and (ResultCode <> 3010) then
  begin
    Result := '.NET 10 Desktop Runtime kurulumu başarısız oldu (çıkış kodu: ' +
              IntToStr(ResultCode) + '). Lütfen elle kurun: ' +
              'https://dotnet.microsoft.com/download/dotnet/10.0';
  end
  else
  begin
    if ResultCode = 3010 then
    begin
      NeedsRestart := True;
      Log('.NET 10 Desktop Runtime kuruldu — yeniden başlatma gerekiyor.');
    end
    else
      Log('.NET 10 Desktop Runtime başarıyla kuruldu.');
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

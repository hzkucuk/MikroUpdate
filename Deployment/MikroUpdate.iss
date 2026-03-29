; ============================================================
;  MikroUpdate — Inno Setup Kurulum Scripti
;  Mikro ERP otomatik güncelleme sistemi
; ============================================================

#define MyAppName "MikroUpdate"
#define MyAppVersion "1.27.5"
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
RestartApplications=no
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
Name: "{commonappdata}\MikroUpdate\Updates"; Permissions: everyone-modify

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
; /NOPOSTLAUNCH=1 parametresi verildiğinde atlanır (servis kendisi başlatır)
Filename: "{app}\Win\{#MyAppExeName}"; Flags: nowait postinstall skipifnotsilent runasoriginaluser; Check: ShouldPostInstallLaunch

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
  ClientSetupArgsEdit: TNewEdit;
  ChkClient: TNewCheckBox;
  ChkEDefter: TNewCheckBox;
  ChkBeyanname: TNewCheckBox;
  { Mevcut config.json'dan korunan, UI'da olmayan alanlar }
  ExistingAutoLaunch: Boolean;
  ExistingAutoSelfUpdate: Boolean;
  ExistingCheckInterval: Integer;
  ExistingCdnBaseUrl: String;
  ExistingProxyAddress: String;
  ExistingHttpTimeout: Integer;
  ExistingConfigLoaded: Boolean;
  { Modül bazlı ek kurulum argümanları }
  ExistingEDefterSetupArgs: String;
  ExistingBeyannameSetupArgs: String;

{ ============================================================ }
{  JSON Yardımcı Fonksiyonları (basit anahtar-değer çıkarma)    }
{ ============================================================ }

function GetJsonStringValue(const Json, Key: String): String;
var
  SearchKey, Sub: String;
  StartPos, EndPos: Integer;
begin
  Result := '';
  SearchKey := '"' + Key + '"';
  StartPos := Pos(SearchKey, Json);
  if StartPos = 0 then
    Exit;

  Sub := Copy(Json, StartPos + Length(SearchKey), Length(Json));
  { ':' karakterini ve boşlukları atla }
  StartPos := Pos(':', Sub);
  if StartPos = 0 then
    Exit;
  Sub := Copy(Sub, StartPos + 1, Length(Sub));

  { Baştaki boşlukları atla }
  while (Length(Sub) > 0) and ((Sub[1] = ' ') or (Sub[1] = #9)) do
    Sub := Copy(Sub, 2, Length(Sub));

  if (Length(Sub) > 0) and (Sub[1] = '"') then
  begin
    { String değer }
    Sub := Copy(Sub, 2, Length(Sub));
    EndPos := Pos('"', Sub);
    if EndPos > 0 then
      Result := Copy(Sub, 1, EndPos - 1);
  end;
end;

function GetJsonBoolValue(const Json, Key: String; DefaultValue: Boolean): Boolean;
var
  SearchKey, Sub: String;
  StartPos: Integer;
begin
  Result := DefaultValue;
  SearchKey := '"' + Key + '"';
  StartPos := Pos(SearchKey, Json);
  if StartPos = 0 then
    Exit;

  Sub := Copy(Json, StartPos + Length(SearchKey), Length(Json));
  if Pos('true', Lowercase(Sub)) < Pos('false', Lowercase(Sub)) then
    Result := True
  else if Pos('false', Lowercase(Sub)) > 0 then
    Result := False;
end;

function GetJsonIntValue(const Json, Key: String; DefaultValue: Integer): Integer;
var
  SearchKey, Sub, NumStr: String;
  StartPos, I: Integer;
begin
  Result := DefaultValue;
  SearchKey := '"' + Key + '"';
  StartPos := Pos(SearchKey, Json);
  if StartPos = 0 then
    Exit;

  Sub := Copy(Json, StartPos + Length(SearchKey), Length(Json));
  StartPos := Pos(':', Sub);
  if StartPos = 0 then
    Exit;
  Sub := Copy(Sub, StartPos + 1, Length(Sub));

  { Baştaki boşlukları atla }
  while (Length(Sub) > 0) and ((Sub[1] = ' ') or (Sub[1] = #9)) do
    Sub := Copy(Sub, 2, Length(Sub));

  { Sayıyı oku }
  NumStr := '';
  for I := 1 to Length(Sub) do
  begin
    if (Sub[I] >= '0') and (Sub[I] <= '9') then
      NumStr := NumStr + Sub[I]
    else
      Break;
  end;

  if Length(NumStr) > 0 then
    Result := StrToIntDef(NumStr, DefaultValue);
end;

function IsModuleEnabled(const Json, ModuleName: String): Boolean;
var
  SearchKey, Sub: String;
  StartPos, EnabledPos: Integer;
begin
  Result := False;
  SearchKey := '"Name": "' + ModuleName + '"';
  StartPos := Pos(SearchKey, Json);
  if StartPos = 0 then
    Exit;

  { Modül bloğunun geri kalanından Enabled değerini oku }
  Sub := Copy(Json, StartPos, 200);
  EnabledPos := Pos('"Enabled"', Sub);
  if EnabledPos = 0 then
    Exit;

  Sub := Copy(Sub, EnabledPos, 50);
  Result := Pos('true', Lowercase(Sub)) > 0;
end;

function GetModuleSetupArgs(const Json, ModuleName: String): String;
var
  SearchKey, Sub: String;
  StartPos, ArgsPos, QuoteStart, QuoteEnd: Integer;
begin
  Result := '';
  SearchKey := '"Name": "' + ModuleName + '"';
  StartPos := Pos(SearchKey, Json);
  if StartPos = 0 then
    Exit;

  { Moduel blogundan SetupArgs degerini oku }
  Sub := Copy(Json, StartPos, 500);
  ArgsPos := Pos('"SetupArgs"', Sub);
  if ArgsPos = 0 then
    Exit;

  Sub := Copy(Sub, ArgsPos + Length('"SetupArgs"'), 300);
  QuoteStart := Pos('"', Sub);
  if QuoteStart = 0 then
    Exit;
  Sub := Copy(Sub, QuoteStart + 1, Length(Sub));
  QuoteEnd := Pos('"', Sub);
  if QuoteEnd > 1 then
    Result := Copy(Sub, 1, QuoteEnd - 1);
end;

function SetComboByValue(Combo: TNewComboBox; const Value: String): Boolean;
var
  I: Integer;
begin
  Result := False;
  for I := 0 to Combo.Items.Count - 1 do
  begin
    if CompareText(Combo.Items[I], Value) = 0 then
    begin
      Combo.ItemIndex := I;
      Result := True;
      Exit;
    end;
  end;
end;

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

{ Urune gore Client Setup argumanlarini olusturur }
function GetDefaultClientSetupArgs: String;
var
  ProductComp: String;
begin
  if ProductCombo.Items[ProductCombo.ItemIndex] = 'Fly' then
    ProductComp := 'mikrofly'
  else
    ProductComp := 'mikrojump';

  Result := '/LANG=tr /TYPE=custom /COMPONENTS="main,main\efatura,main\tuik,main\kep,' + ProductComp + '" /TASKS="desktopicon"';
end;

procedure UpdateClientSetupArgs;
begin
  ClientSetupArgsEdit.Text := GetDefaultClientSetupArgs;
end;

procedure OnProductChange(Sender: TObject);
begin
  UpdateClientSetupArgs;
end;

{ JSON string değerlerinde backslash'ları escape eder: \ → \\ }
function JsonEscapeStr(const Value: String): String;
begin
  Result := Value;
  StringChangeEx(Result, '\', '\\', True);
end;

{ JSON string değerlerinde escape'leri geri alır: \\ → \ }
function JsonUnescapeStr(const Value: String): String;
begin
  Result := Value;
  StringChangeEx(Result, '\\', '\', True);
end;

function BoolToStr(Value: Boolean): String;
begin
  if Value then
    Result := 'true'
  else
    Result := 'false';
end;

procedure LoadExistingConfig;
var
  ConfigPath, Val: String;
  RawJson: AnsiString;
  Json: String;
begin
  ExistingConfigLoaded := False;
  { Varsayilan degerler }
  ExistingAutoLaunch := True;
  ExistingAutoSelfUpdate := True;
  ExistingCheckInterval := 30;
  ExistingCdnBaseUrl := 'https://cdn-mikro.atros.com.tr/mikro';
  ExistingProxyAddress := '';
  ExistingHttpTimeout := 0;
  ExistingEDefterSetupArgs := '';
  ExistingBeyannameSetupArgs := '';

  ConfigPath := ExpandConstant('{commonappdata}\MikroUpdate\config.json');
  if not FileExists(ConfigPath) then
  begin
    Log('Mevcut config.json bulunamadı — varsayılan ayarlar kullanılacak.');
    Exit;
  end;

  if not LoadStringFromFile(ConfigPath, RawJson) then
  begin
    Log('config.json okunamadı — varsayılan ayarlar kullanılacak.');
    Exit;
  end;

  Json := String(RawJson);
  Log('Mevcut config.json okundu — UI senkronize ediliyor...');
  ExistingConfigLoaded := True;

  { UI alanlarını mevcut config ile doldur }
  Val := GetJsonStringValue(Json, 'MajorVersion');
  if Length(Val) > 0 then
    SetComboByValue(MajorVersionCombo, Val);

  Val := GetJsonStringValue(Json, 'ProductName');
  if Length(Val) > 0 then
    SetComboByValue(ProductCombo, Val);

  Val := GetJsonStringValue(Json, 'UpdateMode');
  if Length(Val) > 0 then
    SetComboByValue(UpdateModeCombo, Val);

  Val := JsonUnescapeStr(GetJsonStringValue(Json, 'ServerSharePath'));
  if Length(Val) > 0 then
    ServerPathEdit.Text := Val;

  Val := JsonUnescapeStr(GetJsonStringValue(Json, 'LocalInstallPath'));
  if Length(Val) > 0 then
    LocalPathEdit.Text := Val;

  Val := JsonUnescapeStr(GetJsonStringValue(Json, 'SetupFilesPath'));
  if Length(Val) > 0 then
    SetupFilesPathEdit.Text := Val;

  { Modül durumlarını yükle }
  ChkEDefter.Checked := IsModuleEnabled(Json, 'e-Defter');
  ChkBeyanname.Checked := IsModuleEnabled(Json, 'Beyanname');

  { UI'da olmayan alanları koru }
  ExistingAutoLaunch := GetJsonBoolValue(Json, 'AutoLaunchAfterUpdate', True);
  ExistingAutoSelfUpdate := GetJsonBoolValue(Json, 'AutoSelfUpdate', True);
  ExistingCheckInterval := GetJsonIntValue(Json, 'CheckIntervalMinutes', 30);

  Val := JsonUnescapeStr(GetJsonStringValue(Json, 'CdnBaseUrl'));
  if Length(Val) > 0 then
    ExistingCdnBaseUrl := Val;

  Val := JsonUnescapeStr(GetJsonStringValue(Json, 'ProxyAddress'));
  if Length(Val) > 0 then
    ExistingProxyAddress := Val;

  ExistingHttpTimeout := GetJsonIntValue(Json, 'HttpTimeoutSeconds', 0);

  { Modül bazlı SetupArgs değerlerini oku }
  ExistingEDefterSetupArgs := JsonUnescapeStr(GetModuleSetupArgs(Json, 'e-Defter'));
  ExistingBeyannameSetupArgs := JsonUnescapeStr(GetModuleSetupArgs(Json, 'Beyanname'));

  Val := JsonUnescapeStr(GetModuleSetupArgs(Json, 'Client'));
  if Length(Val) > 0 then
    ClientSetupArgsEdit.Text := Val;

  Log(Format('Config yüklendi: %s %s, Modüller: e-Defter=%s Beyanname=%s', [MajorVersionCombo.Items[MajorVersionCombo.ItemIndex], ProductCombo.Items[ProductCombo.ItemIndex], BoolToStr(ChkEDefter.Checked), BoolToStr(ChkBeyanname.Checked)]));
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

  TopPos := 4;

  { Ana Sürüm Seçimi }
  LabelMajorVersion := TNewStaticText.Create(ConfigPage);
  LabelMajorVersion.Parent := ConfigPage.Surface;
  LabelMajorVersion.Caption := 'Ana Sürüm:';
  LabelMajorVersion.Top := TopPos;
  LabelMajorVersion.Left := 0;

  MajorVersionCombo := TNewComboBox.Create(ConfigPage);
  MajorVersionCombo.Parent := ConfigPage.Surface;
  MajorVersionCombo.Top := TopPos + 16;
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
  ProductCombo.Top := TopPos + 16;
  ProductCombo.Left := (ConfigPage.SurfaceWidth div 2) + 12;
  ProductCombo.Width := (ConfigPage.SurfaceWidth div 2) - 12;
  ProductCombo.Style := csDropDownList;
  ProductCombo.Items.Add('Jump');
  ProductCombo.Items.Add('Fly');
  ProductCombo.ItemIndex := 0;
  ProductCombo.OnChange := @OnProductChange;

  TopPos := TopPos + 42;

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
  UpdateModeCombo.Top := TopPos + 16;
  UpdateModeCombo.Left := 0;
  UpdateModeCombo.Width := ConfigPage.SurfaceWidth;
  UpdateModeCombo.Style := csDropDownList;
  UpdateModeCombo.Items.Add('Local');
  UpdateModeCombo.Items.Add('Online');
  UpdateModeCombo.Items.Add('Hybrid');
  UpdateModeCombo.ItemIndex := 0;  { Varsayılan: Local }

  TopPos := TopPos + 42;

  { Sunucu Paylaşım Yolu }
  LabelServer := TNewStaticText.Create(ConfigPage);
  LabelServer.Parent := ConfigPage.Surface;
  LabelServer.Caption := 'Sunucu Paylaşım Yolu (ör: \\SERVER\MikroV16xx):';
  LabelServer.Top := TopPos;
  LabelServer.Left := 0;

  ServerPathEdit := TNewEdit.Create(ConfigPage);
  ServerPathEdit.Parent := ConfigPage.Surface;
  ServerPathEdit.Top := TopPos + 16;
  ServerPathEdit.Left := 0;
  ServerPathEdit.Width := ConfigPage.SurfaceWidth;
  ServerPathEdit.Text := '\\SERVER\MikroV16xx';

  TopPos := TopPos + 42;

  { Terminal Kurulum Yolu }
  LabelLocal := TNewStaticText.Create(ConfigPage);
  LabelLocal.Parent := ConfigPage.Surface;
  LabelLocal.Caption := 'Terminal Kurulum Yolu (ör: C:\Mikro\v16xx):';
  LabelLocal.Top := TopPos;
  LabelLocal.Left := 0;

  LocalPathEdit := TNewEdit.Create(ConfigPage);
  LocalPathEdit.Parent := ConfigPage.Surface;
  LocalPathEdit.Top := TopPos + 16;
  LocalPathEdit.Left := 0;
  LocalPathEdit.Width := ConfigPage.SurfaceWidth;
  LocalPathEdit.Text := 'C:\Mikro\v16xx';

  TopPos := TopPos + 42;

  { Setup Dosyaları Yolu }
  LabelSetupPath := TNewStaticText.Create(ConfigPage);
  LabelSetupPath.Parent := ConfigPage.Surface;
  LabelSetupPath.Caption := 'Setup Dosyaları Yolu (ör: \\SERVER\MikroV16xx\CLIENT):';
  LabelSetupPath.Top := TopPos;
  LabelSetupPath.Left := 0;

  SetupFilesPathEdit := TNewEdit.Create(ConfigPage);
  SetupFilesPathEdit.Parent := ConfigPage.Surface;
  SetupFilesPathEdit.Top := TopPos + 16;
  SetupFilesPathEdit.Left := 0;
  SetupFilesPathEdit.Width := ConfigPage.SurfaceWidth;
  SetupFilesPathEdit.Text := '\\SERVER\MikroV16xx\CLIENT';

  TopPos := TopPos + 42;

  { Modül Seçimi }
  with TNewStaticText.Create(ConfigPage) do
  begin
    Parent := ConfigPage.Surface;
    Caption := 'Güncellenecek Modüller:';
    Top := TopPos;
    Left := 0;
  end;

  TopPos := TopPos + 18;

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

  TopPos := TopPos + 26;

  { Client Setup Ek Argümanlar }
  with TNewStaticText.Create(ConfigPage) do
  begin
    Parent := ConfigPage.Surface;
    Caption := 'Client Setup Argümanları (Inno Setup opsiyonları):';
    Top := TopPos;
    Left := 0;
  end;

  ClientSetupArgsEdit := TNewEdit.Create(ConfigPage);
  ClientSetupArgsEdit.Parent := ConfigPage.Surface;
  ClientSetupArgsEdit.Top := TopPos + 16;
  ClientSetupArgsEdit.Left := 0;
  ClientSetupArgsEdit.Width := ConfigPage.SurfaceWidth;
  { Varsayılan olarak ürüne uygun argümanlar }
  UpdateClientSetupArgs;

  { Mevcut config.json varsa UI'yi senkronize et }
  LoadExistingConfig;
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
    '      "Enabled": true,' + #13#10 +
    '      "SetupArgs": "' + JsonEscapeStr(ClientSetupArgsEdit.Text) + '"' + #13#10 +
    '    },' + #13#10 +
    '    {' + #13#10 +
    '      "Name": "e-Defter",' + #13#10 +
    '      "SetupFileName": "' + Prefix + '_' + Ver + '_eDefter_Setupx064.exe",' + #13#10 +
    '      "ExeFileName": "' + EDeftExe + '",' + #13#10 +
    '      "Enabled": ' + BoolToStr(ChkEDefter.Checked) + ',' + #13#10 +
    '      "SetupArgs": "' + JsonEscapeStr(ExistingEDefterSetupArgs) + '"' + #13#10 +
    '    },' + #13#10 +
    '    {' + #13#10 +
    '      "Name": "Beyanname",' + #13#10 +
    '      "SetupFileName": "' + Ver + '_BEYANNAME_Setupx064.exe",' + #13#10 +
    '      "ExeFileName": "BEYANNAME.EXE",' + #13#10 +
    '      "Enabled": ' + BoolToStr(ChkBeyanname.Checked) + ',' + #13#10 +
    '      "SetupArgs": "' + JsonEscapeStr(ExistingBeyannameSetupArgs) + '"' + #13#10 +
    '    }';
end;

function GenerateConfigJson: String;
var
  UpdateMode, ProxyLine: String;
begin
  UpdateMode := UpdateModeCombo.Items[UpdateModeCombo.ItemIndex];

  { ProxyAddress boş değilse JSON'a ekle }
  if Length(ExistingProxyAddress) > 0 then
    ProxyLine := '  "ProxyAddress": "' + JsonEscapeStr(ExistingProxyAddress) + '",' + #13#10
  else
    ProxyLine := '  "ProxyAddress": "",' + #13#10;

  Result :=
    '{' + #13#10 +
    '  "MajorVersion": "' + MajorVersionCombo.Items[MajorVersionCombo.ItemIndex] + '",' + #13#10 +
    '  "ProductName": "' + ProductCombo.Items[ProductCombo.ItemIndex] + '",' + #13#10 +
    '  "ServerSharePath": "' + JsonEscapeStr(ServerPathEdit.Text) + '",' + #13#10 +
    '  "LocalInstallPath": "' + JsonEscapeStr(LocalPathEdit.Text) + '",' + #13#10 +
    '  "SetupFilesPath": "' + JsonEscapeStr(SetupFilesPathEdit.Text) + '",' + #13#10 +
    '  "AutoLaunchAfterUpdate": ' + BoolToStr(ExistingAutoLaunch) + ',' + #13#10 +
    '  "AutoSelfUpdate": ' + BoolToStr(ExistingAutoSelfUpdate) + ',' + #13#10 +
    '  "CheckIntervalMinutes": ' + IntToStr(ExistingCheckInterval) + ',' + #13#10 +
    '  "UpdateMode": "' + UpdateMode + '",' + #13#10 +
    '  "CdnBaseUrl": "' + JsonEscapeStr(ExistingCdnBaseUrl) + '",' + #13#10 +
    ProxyLine +
    '  "HttpTimeoutSeconds": ' + IntToStr(ExistingHttpTimeout) + ',' + #13#10 +
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

  { Kullanıcının kurulum sırasında seçtiği ayarları her zaman yaz }
  JsonContent := GenerateConfigJson;
  SaveStringToFile(ConfigPath, JsonContent, False);
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
  RetryCount: Integer;
begin
  BinPath := ExpandConstant('{app}\Service\MikroUpdate.Service.exe');

  if IsServiceInstalled then
  begin
    { Upgrade: servisi durdur, sil, yeniden oluştur (dosyalar güncellenmiş olabilir) }
    Log('MikroUpdateService zaten mevcut — yeniden oluşturuluyor...');
    Exec('sc.exe', 'stop MikroUpdateService', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
    Log(Format('sc stop sonuç: %d', [ResultCode]));
    Sleep(2000);
    Exec('sc.exe', 'delete MikroUpdateService', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
    Log(Format('sc delete sonuç: %d', [ResultCode]));

    { Servis tamamen silinene kadar bekle (max 10 saniye) }
    RetryCount := 0;
    while IsServiceInstalled and (RetryCount < 10) do
    begin
      Log(Format('Servis hâlâ mevcut, bekleniyor... (%d/10)', [RetryCount + 1]));
      Sleep(1000);
      RetryCount := RetryCount + 1;
    end;

    if IsServiceInstalled then
      Log('UYARI: Servis 10 saniye sonra hâlâ silinmemiş!');
  end;

  { Servisi oluştur }
  if not Exec('sc.exe',
    Format('create MikroUpdateService binPath= "%s" start= auto DisplayName= "MikroUpdate Güncelleme Servisi"', [BinPath]),
    '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    Log(Format('sc create BAŞARISIZ (Exec hata): %d', [ResultCode]));
  end
  else
  begin
    Log(Format('sc create sonuç: %d', [ResultCode]));
  end;

  { Açıklamasını ayarla }
  Exec('sc.exe',
    'description MikroUpdateService "Mikro ERP otomatik güncelleme servisi"',
    '', SW_HIDE, ewWaitUntilTerminated, ResultCode);

  { Normal kullanıcıların servisi başlatıp durdurabilmesi için ACL ayarla }
  { SDDL: SY=SYSTEM full, BA=Admins full, AU=Authenticated Users start/stop/query/interrogate }
  if Exec('sc.exe',
    'sdset MikroUpdateService D:(A;;CCLCSWRPWPDTLOCRRC;;;SY)(A;;CCDCLCSWRPWPDTLOCRSDRCWDWO;;;BA)(A;;CCLCSWRPWPDTLOCRRC;;;AU)',
    '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    Log(Format('sc sdset sonuç: %d', [ResultCode]));
  end
  else
  begin
    Log(Format('sc sdset BAŞARISIZ: %d', [ResultCode]));
  end;

  { Çökme durumunda otomatik yeniden başlatma: 5sn / 10sn / 30sn }
  Exec('sc.exe',
    'failure MikroUpdateService reset= 60 actions= restart/5000/restart/10000/restart/30000',
    '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  Log(Format('sc failure sonuç: %d', [ResultCode]));

  { Servisi başlat }
  if not Exec('sc.exe', 'start MikroUpdateService', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    Log(Format('sc start BAŞARISIZ (Exec hata): %d', [ResultCode]));
  end
  else
  begin
    Log(Format('sc start sonuç: %d', [ResultCode]));
  end;

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
{  /NOPOSTLAUNCH=1: Servis üzerinden self-update yapıldığında       }
{  installer'dan app başlatmayı engeller (servis kendisi başlatır)  }
{ ============================================================ }

function ShouldPostInstallLaunch: Boolean;
begin
  Result := ExpandConstant('{param:NOPOSTLAUNCH|0}') <> '1';
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

  { Upgrade: Servisi durdur — dosyalar kilitli kalmasın }
  if IsServiceInstalled then
  begin
    Log('PrepareToInstall: MikroUpdateService durduruluyor...');
    Exec('sc.exe', 'stop MikroUpdateService', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
    Sleep(3000);
    Log(Format('PrepareToInstall: sc stop sonuç: %d', [ResultCode]));
  end;

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

# Kurulum Rehberi

## Gereksinimler

- **İşletim Sistemi:** Windows 10 / 11
- **Runtime:** [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/10.0)
- **Ağ:** Sunucu paylaşım klasörüne erişim (`\\sunucu\MikroVxx`)

## Inno Setup Kurulum Paketi

### Gereksinimler (Build)

- [Inno Setup 6](https://jrsoftware.org/isdl.php) — `ISCC.exe` PATH'te veya varsayılan dizinde
- .NET 10 SDK

### Installer Oluşturma

```powershell
cd Deployment
.\Build-Setup.ps1
```

Script otomatik olarak:
1. Her iki projeyi publish eder (framework-dependent)
2. Inno Setup Compiler (ISCC) ile EXE installer derler
3. Sonuç dosyasını `installer\` dizinine koyar

### Installer ile Kurulum

```powershell
# Standart kurulum (UI ile — özel yapılandırma sayfası dahil)
.\installer\MikroUpdate_Setup_1.9.0.exe

# Sessiz kurulum
.\installer\MikroUpdate_Setup_1.9.0.exe /VERYSILENT /SUPPRESSMSGBOXES

# Sessiz kaldırma
.\installer\unins000.exe /VERYSILENT
```

Kurulum sihirbazı şunları yapar:
- `C:\Program Files\MikroUpdate\Win\` — Tray uygulaması dosyaları
- `C:\Program Files\MikroUpdate\Service\` — Windows servisi dosyaları
- **Özel yapılandırma sayfası:**
  - Ana sürüm (V16/V17) ve ürün seçimi (Jump/Fly)
  - Sunucu yolu, terminal yolu, setup dosyaları yolu
  - **Modül seçimi** — Client (zorunlu), e-Defter (opsiyonel), Beyanname (opsiyonel)
- **MikroUpdateService** Windows servisini kaydeder ve başlatır (görev seçiliyse)
- Başlat Menüsü kısayolu oluşturur
- Windows Başlangıç kısayolu oluşturur (görev seçiliyse, `/auto` parametresi ile)
- `%ProgramData%\MikroUpdate\` dizin yapısını oluşturur
- `config.json` dosyasını özel sayfadaki değerlerle oluşturur (modül seçimine göre `Enabled` ayarlanır)

## Manuel Kurulum (Alternatif)

### Derleme

```bash
dotnet build MikroUpdate.slnx -c Release
```

Çıktı dizinleri:
- `MikroUpdate.Win\bin\Release\net10.0-windows\` — tray uygulaması
- `MikroUpdate.Service\bin\Release\net10.0\` — Windows servisi

## Kurulum

### 1. Dosyaları Kopyalama

```powershell
# Servis dosyaları
xcopy /E /Y "MikroUpdate.Service\bin\Release\net10.0\*" "C:\MikroUpdate\Service\"

# Tray uygulaması
xcopy /E /Y "MikroUpdate.Win\bin\Release\net10.0-windows\*" "C:\MikroUpdate\Win\"
```

### 2. Windows Service Kurulumu (Admin PowerShell)

```powershell
# Servis oluşturma
sc.exe create MikroUpdateService `
    binPath="C:\MikroUpdate\Service\MikroUpdate.Service.exe" `
    start=auto `
    DisplayName="MikroUpdate Güncelleme Servisi"

# Açıklama ekleme
sc.exe description MikroUpdateService "Mikro ERP otomatik güncelleme servisi — admin yetkili kurulum"

# Servis başlatma
sc.exe start MikroUpdateService

# Servis durumu kontrol
sc.exe query MikroUpdateService
```

> **Not:** Servis `LocalSystem` hesabıyla çalışır ve admin yetkisi gerektiren kurulum
> işlemlerini (süreç sonlandırma, dosya kopyalama, Inno Setup kurulumu) yönetir.

### 3. İlk Yapılandırma

Tray uygulamasını çalıştırın → **Ayarlar** butonuna tıklayın:

| Ayar | Açıklama | Jump Örneği | Fly Örneği |
|------|----------|-------------|------------|
| Ürün | Jump veya Fly | `Jump` | `Fly` |
| Sunucu Paylaşım Yolu | Ana makine EXE yolu | `\\SERVER\MikroV16xx` | `\\SERVER\MikroV17xx` |
| Terminal Kurulum Yolu | Terminal kurulum dizini | `C:\Mikro\v16xx` | `C:\Mikro\v17xx` |
| Setup Dosyası | Client setup dosya adı | `Jump_v16xx_Client_Setupx064.exe` | `Fly_v17xx_Client_Setupx064.exe` |

Kontrol edilen EXE dosyaları:
- Jump → `MikroJump.EXE` (sunucu ve terminal yollarında)
- Fly → `MikroFly.EXE` (sunucu ve terminal yollarında)

### 4. Otomatik Çalıştırma

Başlangıçta otomatik kontrol için:

```powershell
# Startup kısayolu oluşturma
$WshShell = New-Object -ComObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut("$env:APPDATA\Microsoft\Windows\Start Menu\Programs\Startup\MikroUpdate.lnk")
$Shortcut.TargetPath = "C:\MikroUpdate\Win\MikroUpdate.Win.exe"
$Shortcut.Arguments = "/auto"
$Shortcut.Save()
```

## Yapılandırma Dosyası

Konum: `%ProgramData%\MikroUpdate\config.json`

İlk çalıştırmada otomatik oluşturulur. Manuel düzenleme yerine uygulama içindeki
Ayarlar formunu kullanmanız önerilir. Ayar değişikliklerinde servis otomatik olarak
yeni yapılandırmayı yükler (`ReloadConfig` komutu).

## Log Dosyaları

Konum: `%ProgramData%\MikroUpdate\logs\`

- Günlük rotasyonlu dosyalar: `MikroUpdate_YYYY-MM-DD.log`
- Log seviyeleri: `INFO`, `OK`, `WARN`, `ERROR`
- UI log paneli ve dosya log'u eş zamanlı çalışır
- Hata teşhisi için log dosyalarını inceleyin

## Servis Yönetimi

```powershell
# Servis durumu
sc.exe query MikroUpdateService

# Servisi durdurma
sc.exe stop MikroUpdateService

# Servisi başlatma
sc.exe start MikroUpdateService

# Servisi kaldırma
sc.exe stop MikroUpdateService
sc.exe delete MikroUpdateService
```

## Toplu Dağıtım

Sessiz kurulum parametreleri ile dağıtım:

```powershell
# Sessiz kurulum (tüm varsayılan görevlerle)
MikroUpdate_Setup_1.6.0.exe /VERYSILENT /SUPPRESSMSGBOXES

# Sessiz kurulum — görev seçimi ile
MikroUpdate_Setup_1.6.0.exe /VERYSILENT /SUPPRESSMSGBOXES /TASKS="servicetask,startuptask"

# Sessiz kurulum — servis olmadan
MikroUpdate_Setup_1.6.0.exe /VERYSILENT /SUPPRESSMSGBOXES /TASKS="startuptask"
```
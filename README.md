# MikroUpdate

![Version](https://img.shields.io/badge/version-1.23.8-blue)
![.NET](https://img.shields.io/badge/.NET-10.0-purple)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey)

Mikro ERP (Jump / Fly) yazılımları için domain ortamında çalışan otomatik güncelleme sistemi.

## Genel Bakış

MikroUpdate, sunucu üzerindeki Mikro ERP versiyonunu terminal makineleriyle karşılaştırarak
otomatik sessiz kurulum gerçekleştirir. Domain ortamında admin yetkisi gerektiren kurulumları
Windows Service üzerinden yönetir.

## Mimari

```
┌─────────────────────┐     Named Pipe (IPC)    ┌───────────────────────────┐
│  MikroUpdate.Win    │◄───── PipeProtocol ─────►│  MikroUpdate.Service      │
│  (Tray App - User)  │   length-prefixed JSON   │  (Windows Service - SYSTEM)│
│                     │                          │                           │
│  • Versiyon durumu  │  Komutlar:               │  • Versiyon kontrolü      │
│  • Güncelleme UI    │  ├ CheckVersion          │  • Süreç yönetimi (kill)  │
│  • İşlem günlüğü   │  ├ RunUpdate             │  • Setup kopyalama        │
│  • Ayarlar formu    │  ├ GetStatus             │  • Sessiz kurulum (admin) │
│  • Tray menüsü      │  ├ ReloadConfig          │  • 30 dk periyodik kontrol│
│                     │  ├ DownloadUpdate        │  • UAC'sız self-update    │
│                     │  └ InstallSelfUpdate     │                           │
└─────────┬───────────┘                          └────────────┬──────────────┘
          │                                                   │
          └──────────── MikroUpdate.Shared ───────────────────┘
                    (UpdateConfig, PipeProtocol,
                     ServiceCommand, ServiceResponse)
```

### Çalışma Modları

| Mod | Koşul | Davranış |
|-----|-------|----------|
| **Servis modu** | MikroUpdate Service çalışıyor | Tüm işlemler pipe üzerinden servis tarafından yapılır (admin yetkili) |
| **Otomatik mod** | `/auto` parametresi | Sessiz kontrol → güncelleme → Mikro başlatma → çıkış |

## Hızlı Başlangıç

### Kurulum Paketi (Önerilen)

```powershell
# Installer oluşturma (Inno Setup gerekli)
cd Deployment
.\Build-Setup.ps1

# Kurulum (UI ile)
.\installer\MikroUpdate_Setup_1.19.0.exe

# Sessiz kurulum
.\installer\MikroUpdate_Setup_1.19.0.exe /VERYSILENT /SUPPRESSMSGBOXES
```

Installer otomatik olarak: dosya kopyalama, servis kaydı, kısayollar, ProgramData dizinleri ve ilk yapılandırma oluşturur.

> **Not:** Installer şu anda imzasız dağıtılmaktadır. SmartScreen uyarısı için [docs/CODE_SIGNING.md](docs/CODE_SIGNING.md) dosyasına bakın.

### Manuel Derleme

```bash
# Derleme
dotnet build MikroUpdate.slnx

# Tray uygulaması
MikroUpdate.Win.exe

# Otomatik mod (sessiz kontrol + güncelleme + Mikro başlatma)
MikroUpdate.Win.exe /auto
```

## Yapılandırma

Ayarlar `%ProgramData%\MikroUpdate\config.json` dosyasında saklanır:

```json
{
  "MajorVersion": "V16",
  "ProductName": "Jump",
  "ServerSharePath": "\\\\SERVER\\MikroV16xx",
  "LocalInstallPath": "C:\\Mikro\\v16xx",
  "SetupFilesPath": "\\\\SERVER\\MikroV16xx\\CLIENT",
  "AutoLaunchAfterUpdate": true,
  "CheckIntervalMinutes": 30,
  "Modules": [
    { "Name": "Client", "SetupFileName": "Jump_v16xx_Client_Setupx064.exe", "ExeFileName": "MikroJump.EXE", "Enabled": true },
    { "Name": "e-Defter", "SetupFileName": "Jump_v16xx_eDefter_Setupx064.exe", "ExeFileName": "myEDefterStandart.exe", "Enabled": true },
    { "Name": "Beyanname", "SetupFileName": "v16xx_BEYANNAME_Setupx064.exe", "ExeFileName": "BEYANNAME.EXE", "Enabled": true }
  ]
}
```

### Log Sistemi

İşlem logları `%ProgramData%\MikroUpdate\logs\` dizinine günlük rotasyonlu olarak yazılır:
- Dosya formatı: `MikroUpdate_YYYY-MM-DD.log`
- Log seviyeleri: `INFO`, `OK`, `WARN`, `ERROR`
- UI log paneli (RichTextBox) ve dosya log'u eş zamanlı çalışır

### Ürün / Modül / EXE Eşleşmeleri

| Sürüm | Ürün | Modül | EXE | Setup |
|-------|------|-------|-----|-------|
| V16 | Jump | Client | `MikroJump.EXE` | `Jump_v16xx_Client_Setupx064.exe` |
| V16 | Jump | e-Defter | `myEDefterStandart.exe` | `Jump_v16xx_eDefter_Setupx064.exe` |
| V16 | Jump | Beyanname | `BEYANNAME.EXE` | `v16xx_BEYANNAME_Setupx064.exe` |
| V17 | Fly | Client | `MikroFly.EXE` | `Fly_v17xx_Client_Setupx064.exe` |
| V17 | Fly | e-Defter | `MyeDefter.exe` | `Fly_v17xx_eDefter_Setupx064.exe` |
| V17 | Fly | Beyanname | `BEYANNAME.EXE` | `v17xx_BEYANNAME_Setupx064.exe` |

### Versiyon Kontrol Akışı

Her modül için sunucu ve terminal EXE dosyası karşılaştırılır:

```
Client:    Sunucu 16.39.5.46064 > Terminal 16.38.0.45000  →  ▲ Güncelle
e-Defter:  Sunucu 16.39.5.46064 = Terminal 16.39.5.46064  →  ✔ Güncel
Beyanname: Sunucu 16.40.0.00000 > Terminal 16.38.0.45000  →  ▲ Güncelle
```

## Gereksinimler

- Windows 10/11 (domain ortamı)
- .NET 10 Runtime
- Sunucu paylaşım erişimi (`\\sunucu\MikroVxx`)

## Lisans

Bu proje [MIT Lisansı](LICENSE) ile lisanslanmıştır.

## Gizlilik Politikası

Bu program, kullanıcı tarafından yapılandırılmadıkça hiçbir bilgiyi ağ üzerinden iletmez.
Detaylar için [PRIVACY.md](PRIVACY.md) dosyasına bakın.

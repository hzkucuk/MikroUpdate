# MikroUpdate

![Version](https://img.shields.io/badge/version-1.4.0-blue)
![.NET](https://img.shields.io/badge/.NET-10.0-purple)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey)

Mikro ERP (Jump / Fly) yazılımları için domain ortamında çalışan otomatik güncelleme sistemi.

## Genel Bakış

MikroUpdate, sunucu üzerindeki Mikro ERP versiyonunu terminal makineleriyle karşılaştırarak
otomatik sessiz kurulum gerçekleştirir. Domain ortamında admin yetkisi gerektiren kurulumları
Windows Service üzerinden yönetir. Servis çalışmadığında doğrudan mod ile de çalışabilir.

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
│  • Tray menüsü      │  └ ReloadConfig          │  • 30 dk periyodik kontrol│
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
| **Doğrudan mod** | Servis bulunamıyor | Tray app kendi yetkisiyle doğrudan güncelleme yapar (fallback) |
| **Otomatik mod** | `/auto` parametresi | Sessiz kontrol → güncelleme → Mikro başlatma → çıkış |

## Hızlı Başlangıç

```bash
# Derleme
dotnet build MikroUpdate.slnx

# Tray uygulaması
MikroUpdate.Win.exe

# Otomatik mod (sessiz kontrol + güncelleme + Mikro başlatma)
MikroUpdate.Win.exe /auto

# Servis kurulumu (admin PowerShell)
sc.exe create MikroUpdateService binPath="C:\MikroUpdate\MikroUpdate.Service.exe" start=auto
sc.exe start MikroUpdateService
```

## Yapılandırma

Ayarlar `%ProgramData%\MikroUpdate\config.json` dosyasında saklanır:

```json
{
  "ProductName": "Jump",
  "ServerSharePath": "\\\\SERVER\\MikroV16xx",
  "LocalInstallPath": "C:\\Mikro\\v16xx",
  "SetupFilesPath": "\\\\SERVER\\MikroV16xx\\CLIENT",
  "SetupFileName": "Jump_v16xx_Client_Setupx064.exe",
  "AutoLaunchAfterUpdate": true,
  "CheckIntervalMinutes": 30
}
```

### Log Sistemi

İşlem logları `%ProgramData%\MikroUpdate\logs\` dizinine günlük rotasyonlu olarak yazılır:
- Dosya formatı: `MikroUpdate_YYYY-MM-DD.log`
- Log seviyeleri: `INFO`, `OK`, `WARN`, `ERROR`
- UI log paneli (RichTextBox) ve dosya log'u eş zamanlı çalışır

### Ürün / EXE Eşleşmeleri

| Ürün | EXE | Tipik Sunucu Yolu |
|------|-----|-------------------|
| Jump (V16) | `MikroJump.EXE` | `\\SERVER\MikroV16xx` |
| Fly (V17) | `MikroFly.EXE` | `\\SERVER\MikroV17xx` |

### Versiyon Kontrol Akışı

```
Sunucu: \\SERVER\MikroV16xx\MikroJump.EXE  →  FileVersionInfo  →  16.39.5.46064
Terminal: C:\Mikro\v16xx\MikroJump.EXE      →  FileVersionInfo  →  16.38.0.45000
Sonuç: 16.38.0.45000 < 16.39.5.46064       →  Güncelleme mevcut!
```

## Gereksinimler

- Windows 10/11 (domain ortamı)
- .NET 10 Runtime
- Sunucu paylaşım erişimi (`\\sunucu\MikroVxx`)

## Lisans

Bu proje [MIT Lisansı](LICENSE) ile lisanslanmıştır.

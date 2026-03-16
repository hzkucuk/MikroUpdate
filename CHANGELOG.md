# Değişiklik Günlüğü

Tüm önemli değişiklikler bu dosyada belgelenir.
Format: [Semantic Versioning](https://semver.org/lang/tr/)

## [1.2.0] - 2025-07-11

### Eklenenler
- **Setup Dosyaları Yolu** ayar alanı — setup kurulum dosyalarının bulunduğu klasör yolu artık yapılandırılabilir
- `UpdateConfig.SetupFilesPath` özelliği eklendi (varsayılan: `\\SERVER\MikroV16xx\CLIENT`)
- `SettingsForm`'a "Setup Dosyaları Yolu" satırı eklendi (TextBox + klasör seçme butonu)

### Değişenler
- `ServerSetupFilePath` artık `SetupFilesPath + SetupFileName` üzerinden hesaplanıyor (eskiden sabit kodlu `CLIENT` alt klasörü)
- `ServerClientPath` özelliği kaldırıldı (artık `SetupFilesPath` ile değiştirildi)

### Etkilenen Dosyalar
- `MikroUpdate.Shared\Models\UpdateConfig.cs` (SetupFilesPath eklendi, ServerClientPath kaldırıldı)
- `MikroUpdate.Win\SettingsForm.Designer.cs` (yeni row: label + textbox + browse button)
- `MikroUpdate.Win\SettingsForm.cs` (yeni alan bağlantısı + browse handler)

## [1.1.1] - 2025-07-11

### Düzeltmeler
- **Tray icon görünmüyor hatası** — `NotifyIcon.Icon` atanmadığı için system tray'de ikon görünmüyordu
- Özel uygulama ikonu eklendi (`app.ico` — yeşil gradyan, beyaz "M" harfi, 16/32/48/256px)
- `.csproj` dosyasına `ApplicationIcon` tanımlandı; EXE ve form ikonu senkronize edildi

### Etkilenen Dosyalar
- `MikroUpdate.Win\app.ico` (yeni)
- `MikroUpdate.Win\MikroUpdate.Win.csproj` (ApplicationIcon eklendi)
- `MikroUpdate.Win\Form1.Designer.cs` (NotifyIcon.Icon = Icon)

## [1.1.0] - 2025-07-11

### Eklenenler
- **Windows Service mimarisi** — admin yetkileri gerektiren güncelleme işlemleri servis üzerinden çalışır
- **MikroUpdate.Shared** class library — ortak modeller, Named Pipe protokolü ve mesaj tipleri
- **MikroUpdate.Service** Worker Service — BackgroundService tabanlı Windows servisi
- **Named Pipe IPC** — tray ↔ servis arası length-prefixed JSON mesajlaşma (PipeProtocol)
- **PipeClient** — tray uygulamasından servise komut gönderme istemcisi
- **Servis/doğrudan mod fallback** — servis çalışmıyorsa doğrudan güncelleme yapabilme
- **Periyodik versiyon kontrolü** — servis 30 dakikada bir sunucudan versiyon kontrol eder
- **Ayar yeniden yükleme** — ayarlar değiştiğinde servise ReloadConfig komutu gönderilir

### Değişenler
- `UpdateConfig` modeli `MikroUpdate.Shared` projesine taşındı
- `Form1.cs` servis modu + doğrudan mod desteği eklendi
- Versiyon kontrolü asenkron hale getirildi (`CheckVersionsAsync`)
- Güncelleme akışı servis/doğrudan mod ayrımı ile yeniden yapılandırıldı

### Etkilenen Dosyalar
- `MikroUpdate.Shared\Models\UpdateConfig.cs` (taşındı)
- `MikroUpdate.Shared\PipeProtocol.cs` (yeni)
- `MikroUpdate.Shared\PipeConstants.cs` (yeni)
- `MikroUpdate.Shared\Messages\ServiceCommand.cs` (yeni)
- `MikroUpdate.Shared\Messages\ServiceResponse.cs` (yeni)
- `MikroUpdate.Service\UpdateWorker.cs` (yeni)
- `MikroUpdate.Service\Services\VersionService.cs` (yeni)
- `MikroUpdate.Service\Services\UpdateService.cs` (yeni)
- `MikroUpdate.Service\Services\ConfigService.cs` (yeni)
- `MikroUpdate.Service\Program.cs` (yeni)
- `MikroUpdate.Win\Services\PipeClient.cs` (yeni)
- `MikroUpdate.Win\Form1.cs` (değişti — servis entegrasyonu)
- `MikroUpdate.Win\MikroUpdate.Win.csproj` (Shared referansı eklendi)

## [1.0.0] - 2025-07-11

### Eklenenler
- Mikro ERP otomatik güncelleme sistemi temel yapısı
- Jump ve Fly ürün desteği (V16/V17)
- Sunucu paylaşımından setup kopyalama ve Inno Setup sessiz kurulum
- Versiyon karşılaştırma (FileVersionInfo)
- System tray desteği (NotifyIcon + ContextMenuStrip)
- Ayarlar formu (SettingsForm) — modal dialog
- Otomatik mod (`/auto` CLI switch)
- JSON yapılandırma dosyası (`%ProgramData%\MikroUpdate\config.json`)
- Dark mode desteği (SystemColorMode.System)

### Etkilenen Dosyalar
- `MikroUpdate.Win\Models\UpdateConfig.cs`
- `MikroUpdate.Win\Services\ConfigService.cs`
- `MikroUpdate.Win\Services\VersionService.cs`
- `MikroUpdate.Win\Services\UpdateService.cs`
- `MikroUpdate.Win\Form1.cs` / `Form1.Designer.cs`
- `MikroUpdate.Win\SettingsForm.cs` / `SettingsForm.Designer.cs`
- `MikroUpdate.Win\Program.cs`

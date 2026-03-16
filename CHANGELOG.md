# Değişiklik Günlüğü

Tüm önemli değişiklikler bu dosyada belgelenir.
Format: [Semantic Versioning](https://semver.org/lang/tr/)

## [1.6.0] - 2025-07-12

### Eklenenler
- **Inno Setup kurulum paketi** — WiX v5 MSI yerine Inno Setup EXE installer
- `Deployment\MikroUpdate.iss` Inno Setup scripti — özel yapılandırma sayfası (ürün seçimi, sunucu yolu, kurulum yolları)
- `Deployment\Build-Setup.ps1` build scripti — publish + ISCC derleme
- Kurulum sırasında servis kaydı ve başlatma (`sc.exe` ile)
- Özel sayfa: ürün seçimi (Jump/Fly), sunucu paylaşım yolu, terminal kurulum yolu, setup dosya yolu/adı
- Kurulum sırasında `config.json` otomatik oluşturma (özel sayfa değerleriyle)
- Kaldırma sırasında ProgramData temizliği onay dialogu
- Sessiz kurulum desteği (`/VERYSILENT /SUPPRESSMSGBOXES`)

### Kaldırılanlar
- `MikroUpdate.Setup` WiX v5 projesi kaldırıldı
- `Deployment\Build-MSI.ps1` kaldırıldı
- WiX Toolset bağımlılığı kaldırıldı

### Değişenler
- `INSTALL.md` Inno Setup kurulum talimatları ile güncellendi
- `README.md` hızlı başlangıç bölümü Inno Setup'a güncellendi

### Etkilenen Dosyalar
- `Deployment\MikroUpdate.iss` (yeni)
- `Deployment\Build-Setup.ps1` (yeni)
- `MikroUpdate.Setup\` (silindi)
- `Deployment\Build-MSI.ps1` (silindi)
- `MikroUpdate.slnx` (Setup projesi kaldırıldı)
- `.gitignore` (*.g.wxs kaldırıldı)
- `INSTALL.md`, `README.md`, `FEATURES.md` (güncellendi)

## [1.5.0] - 2025-07-11

### Eklenenler
- **MSI kurulum paketi** — WiX Toolset v5 ile MSI installer (v1.6.0'da Inno Setup ile değiştirildi)

### Etkilenen Dosyalar
- `MikroUpdate.Setup\` (v1.6.0'da kaldırıldı)
- `Deployment\Build-MSI.ps1` (v1.6.0'da kaldırıldı)

## [1.4.0] - 2025-07-11

### Eklenenler
- **Dosya tabanlı log sistemi** — tüm işlem logları `%ProgramData%\MikroUpdate\logs\` dizinine günlük rotasyonlu dosyalara yazılır (`FileLogService`)
- **Tray balloon bildirimleri** — güncelleme mevcut, güncelleme tamamlandı, hatalar ve bağlantı sorunları tray ikonundan bildirilir
- **Dinamik tray tooltip** — `NotifyIcon.Text` durum değişikliklerinde güncellenir (ör: "MikroUpdate — Güncel")
- **PipeClient hata raporlama** — `OnError` callback ile timeout ve IO hataları ayırt edilerek loglanır

### Değişenler
- **AppendLog ikili yazım** — UI log (RichTextBox) ve dosya log'u eş zamanlı çalışır
- **CheckVersionsDirect hata dayanıklılığı** — GetVersion çağrıları ayrı try/catch ile korunur, ağ/IO hataları yutulamaz
- **RunUpdateDirectAsync granüler hata yönetimi** — her güncelleme adımı (süreç kapatma, setup kopyalama, kurulum, versiyon kontrol, temizlik) ayrı try/catch ile sarılır
- **RunAutoModeAsync kapsamlı hata yönetimi** — yapılandırma yükleme, versiyon kontrol, güncelleme ve Mikro başlatma adımları try/catch ile korunur
- **Tray bildirimleri akıllı gösterim** — sadece form gizli/minimize durumundayken gösterilir

### Etkilenen Dosyalar
- `MikroUpdate.Win\Services\FileLogService.cs` (yeni)
- `MikroUpdate.Win\Services\PipeClient.cs` (OnError callback eklendi)
- `MikroUpdate.Win\Form1.cs` (hata yönetimi, tray bildirimleri, dosya log entegrasyonu)

## [1.3.0] - 2025-07-11

### Eklenenler
- **Kontrol aralığı yapılandırılabilir** — periyodik versiyon kontrol süresi artık UI'dan ayarlanabilir (1–1440 dk, varsayılan 30)
- `UpdateConfig.CheckIntervalMinutes` özelliği eklendi
- `SettingsForm`'a NumericUpDown kontrol aralığı alanı eklendi

### Değişenler
- **Form1 modern minimalist tasarım** — GroupBox'lar kaldırıldı, koyu tema (dark background), büyük versiyon yazıları, ince 4px progress bar, flat butonlar, yeşil vurgulu "Başlat" butonu
- **SettingsForm modern minimalist tasarım** — GroupBox kaldırıldı, hesaplanan yollar tek satır label'a sıkıştırıldı, koyu tema, flat butonlar, yeşil "Kaydet" butonu
- `UpdateWorker` artık `PipeConstants.CheckIntervalMinutes` yerine `_config.CheckIntervalMinutes` kullanıyor

### Etkilenen Dosyalar
- `MikroUpdate.Shared\Models\UpdateConfig.cs` (CheckIntervalMinutes eklendi)
- `MikroUpdate.Service\UpdateWorker.cs` (config-tabanlı interval)
- `MikroUpdate.Win\Form1.Designer.cs` (tamamen yeniden yazıldı)
- `MikroUpdate.Win\SettingsForm.Designer.cs` (tamamen yeniden yazıldı)
- `MikroUpdate.Win\SettingsForm.cs` (CheckInterval binding + minimalist computed paths)

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

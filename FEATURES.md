# Özellikler

## v1.1.0

### Windows Service Mimarisi
- **MikroUpdate.Service** — `LocalSystem` yetkisiyle çalışan Windows Service
- Domain ortamında admin yetkisi olmayan terminal makinelerinde kurulum yapabilme
- `sc.exe create` / `sc.exe start` ile kurulum ve yönetim
- Periyodik versiyon kontrolü (30 dakika aralıkla otomatik sunucu kontrolü)
- Yapılandırma yeniden yükleme desteği (`ReloadConfig` komutu)

### Named Pipe IPC (Inter-Process Communication)
- **PipeProtocol** — 4-byte length prefix + UTF-8 JSON mesajlaşma
- Maksimum 1 MB payload koruması
- Tray app → Servis komutları: `CheckVersion`, `RunUpdate`, `GetStatus`, `ReloadConfig`
- Servis → Tray app yanıtları: `ServiceResponse` (Success, Status, Message, Versions)
- 5 saniye bağlantı zaman aşımı

### Servis / Doğrudan Mod Fallback
- Uygulama başlangıcında servis erişilebilirliği otomatik algılanır
- **Servis modu**: Tüm işlemler admin yetkili servis üzerinden yapılır
- **Doğrudan mod**: Servis bulunamazsa tray app kendi yetkisiyle doğrudan güncelleme yapar
- Servis yanıt vermezse otomatik olarak doğrudan moda geçiş
- Ayar değişikliklerinde servise `ReloadConfig` komutu gönderilir

### Ortak Kütüphane (MikroUpdate.Shared)
- `UpdateConfig` modeli tüm projeler tarafından paylaşılır
- `PipeConstants` — pipe adı, bağlantı zaman aşımı, kontrol aralığı
- `ServiceCommand` / `ServiceResponse` — tip güvenli mesaj kontratları

## v1.0.0

### Versiyon Kontrolü
- Sunucu EXE dosyasının `FileVersionInfo` ile versiyonunu okuma
- Terminal ve sunucu versiyonlarını karşılaştırma
- Güncelleme gerekli/güncel durumunu renk kodlu gösterim

### Güncelleme İş Akışı
1. Versiyon karşılaştırma (sunucu vs. terminal)
2. Çalışan Mikro sürecini kapatma (`Process.Kill`)
3. Setup dosyasını sunucu paylaşımından kopyalama (`\\sunucu\MikroVxx\CLIENT\`)
4. Inno Setup sessiz kurulum (`/SP- /VERYSILENT /SUPPRESSMSGBOXES /NORESTART`)
5. Kurulum sonrası versiyon doğrulama
6. Geçici dosya temizliği
7. Otomatik Mikro başlatma (opsiyonel)

### Ürün Desteği
- **Jump** (V16) — `MikroJump.EXE`
- **Fly** (V17) — `MikroFly.EXE`

### System Tray
- Taskbar sağ tarafında (notification area) çalışma
- Çarpı (X) ile tray'e küçülme (kapanmaz)
- Minimize ile tray'e küçülme
- Sağ tık menüsü: Göster, Kontrol Et, Güncelle, Ayarlar, Çıkış
- Çift tıklama ile geri getirme
- Balloon tip bildirimleri

### Otomatik Mod
- `MikroUpdate.exe /auto` ile sessiz çalışma
- Versiyon kontrol → güncelleme → Mikro başlatma → otomatik kapanma

### Ayarlar Formu
- Ürün seçimi (Jump/Fly)
- Sunucu paylaşım yolu (FolderBrowserDialog)
- Terminal kurulum yolu (FolderBrowserDialog)
- Setup dosya adı
- Otomatik başlatma seçeneği
- Hesaplanan yolların canlı önizlemesi

### Yapılandırma
- JSON dosyası: `%ProgramData%\MikroUpdate\config.json`
- İlk çalıştırmada varsayılan ayarlarla oluşturulur

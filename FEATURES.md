# Özellikler

## v1.10.0

### Hakkında (About) Dialog Penceresi
- **Program bilgisi:** Uygulama adı, versiyon (assembly'den otomatik), açıklama
- **Geliştirici:** Hüseyin Küçük
- **Lisans:** MIT Lisansı
- **Teknolojiler:** .NET 10, WinForms, Worker Service, Named Pipes (IPC), Inno Setup
- **Bağlantılar:** GitHub repo ve e-posta linkleri (tıklanabilir)
- **Erişim:** Ana form buton barı + tray menüsü "Hakkında" öğesi

## v1.9.0

### Kurulum Sırasında Modül Seçimi
- **Installer modül seçimi:** Inno Setup sihirbazında Client, e-Defter, Beyanname checkbox'ları
- **Client zorunlu modül:** Her zaman seçili, kullanıcı tarafından devre dışı bırakılamaz
- **e-Defter ve Beyanname opsiyonel:** Varsayılan seçili değil, kullanıcı ihtiyacına göre işaretler
- **config.json entegrasyonu:** Seçim durumu `Enabled: true/false` olarak yapılandırma dosyasına yazılır

### V16/V17 Yol Otomatik Güncelleme
- **Sürüm geçişinde otomatik yol güncelleme:** Ana Sürüm (V16↔V17) değiştiğinde sunucu, terminal ve setup yolları otomatik güncellenir
- **Ayarlar Formu:** `UpdateVersionPaths()` ile anlık yol dönüşümü (v16xx↔v17xx, MikroV16xx↔MikroV17xx)
- **Installer:** Inno Setup özel sayfasında `OnMajorVersionChange` callback ile aynı davranış

### Varsayılan Ayarlar Düğmesi
- **"Varsayılan" butonu:** Tüm ayarları fabrika değerlerine sıfırlama (onay dialogu ile)
- **Sürüme duyarlı varsayılanlar:** Seçili V16/V17'ye göre dinamik yol ve modül oluşturma
- Ürün Jump'a, kontrol aralığı 30 dk'ya, otomatik başlat açık olarak sıfırlanır

## v1.7.0

### V16/V17 Çoklu Modül Desteği
- **Ana Sürüm Seçimi:** V16 ve V17 sürümleri ayrı yapılandırma ile desteklenir
- **Çoklu Modül Mimarisi:** Her ürün+sürüm kombinasyonu için Client, e-Defter ve Beyanname modülleri
- **Modül Bazlı Versiyon Kontrolü:** Her modülün terminal ve sunucu versiyonu ayrı kontrol edilir
- **Sıralı Güncelleme:** Güncellenmesi gereken modüller için setup dosyaları sırayla kopyalanır ve kurulur
- **Akıllı Varsayılanlar:** Ürün ve sürüm değiştiğinde modül listesi otomatik güncellenir
- **DataGridView ile Versiyon Durumu:** Modül bazlı Terminal/Sunucu/Durum gösterimi
- **Ayarlar Formu:** Ana Sürüm combo, modül düzenleme grid'i, Sıfırla butonu
- **Installer Desteği:** Inno Setup özel sayfasında ana sürüm ve ürün seçimi ile otomatik modül yapılandırması
- **Ayar Sonrası Otomatik Kontrol:** Ayarlar kaydedildikten sonra versiyon kontrolü otomatik tetiklenir
- **.NET 10 Runtime Kontrolü:** Installer kurulum öncesi .NET 10 Desktop Runtime'ı kontrol eder, yoksa otomatik indirir ve sessiz kurar

#### Modül Matrisi
| Sürüm | Ürün | Client Setup | e-Defter Setup | Beyanname Setup |
|-------|------|-------------|----------------|----------------|
| V16 | Jump | Jump_v16xx_Client_Setupx064.exe | Jump_v16xx_eDefter_Setupx064.exe | v16xx_BEYANNAME_Setupx064.exe |
| V16 | Fly | Fly_v16xx_Client_Setupx064.exe | Fly_v16xx_eDefter_Setupx064.exe | v16xx_BEYANNAME_Setupx064.exe |
| V17 | Jump | Jump_v17xx_Client_Setupx064.exe | Jump_v17xx_eDefter_Setupx064.exe | v17xx_BEYANNAME_Setupx064.exe |
| V17 | Fly | Fly_v17xx_Client_Setupx064.exe | Fly_v17xx_eDefter_Setupx064.exe | v17xx_BEYANNAME_Setupx064.exe |

## v1.6.0

### Inno Setup Kurulum Paketi
- WiX v5 MSI yerine Inno Setup EXE installer
- **Özel yapılandırma sayfası:** ürün seçimi (Jump/Fly), sunucu paylaşım yolu, terminal kurulum yolu, setup dosyaları yolu ve dosya adı
- Kurulum sırasında `config.json` otomatik oluşturma (seçilen değerlerle)
- Windows servisi kaydı ve başlatma (`sc.exe` ile, seçilebilir görev)
- Windows Başlangıç kısayolu (seçilebilir görev)
- Sessiz kurulum desteği (`/VERYSILENT /SUPPRESSMSGBOXES`)
- Kaldırma sırasında ProgramData temizliği onay dialogu
- `Deployment\Build-Setup.ps1` — tek komutla publish + installer derleme

## v1.4.0

### Dosya Tabanlı Log Sistemi
- Tüm işlem logları `%ProgramData%\MikroUpdate\logs\` dizinine yazılır
- Günlük rotasyonlu dosyalar (`MikroUpdate_YYYY-MM-DD.log`)
- Log seviyeleri: `INFO`, `OK`, `WARN`, `ERROR`
- Thread-safe yazım (Lock tabanlı senkronizasyon)
- Exception detay desteği (`Type: Message` formatı)
- UI log (RichTextBox) ve dosya log'u eş zamanlı çalışır (dual-write)

### Tray Balloon Bildirimleri
- Güncelleme mevcut → uyarı bildirimi (yeni sürüm numarası ile)
- Güncelleme tamamlandı → bilgi bildirimi
- Hata durumları → hata bildirimi (bağlantı, kurulum, versiyon)
- Akıllı gösterim: form görünür durumdayken bildirim bastırılır

### Dinamik Tray Tooltip
- `NotifyIcon.Text` durum değişikliklerinde güncellenir
- Format: "MikroUpdate — {durum}" (ör: "MikroUpdate — Güncel")

### Geliştirilmiş Hata Yönetimi
- **CheckVersionsDirect** — her GetVersion çağrısı ayrı try/catch ile korunur
- **RunUpdateDirectAsync** — 7 adımın her biri (süreç kapatma, setup kopyalama, kurulum, versiyon kontrol, temizlik) ayrı try/catch ile sarılır
- **RunAutoModeAsync** — yapılandırma, versiyon kontrol, güncelleme ve başlatma ayrı hata blokları
- **PipeClient** — timeout ve IO hataları ayrıştırılarak `OnError` callback ile raporlanır

## v1.3.0

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

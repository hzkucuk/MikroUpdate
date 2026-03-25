# Değişiklik Günlüğü

Tüm önemli değişiklikler bu dosyada belgelenir.
Format: [Semantic Versioning](https://semver.org/lang/tr/)

## [1.23.1] - 2026-03-25

### Temizlik
- **Do`ğ`rudan mod (fallback) kodu ve d`ö`k`ü`man referansları tamamen kaldırıldı**
  - `CheckVersionsDirectAsync` ve `RunUpdateDirectAsync` metotları silindi
  - `_versionService` field'ı kaldırıldı (artık kullanılmıyor)
  - Servis yanıt vermediğinde do`ğ`rudan moda ge`ç`i`ş` yerine hata mesajı g`ö`steriliyor
  - README, FEATURES, CHANGELOG d`ö`k`ü`manlarından do`ğ`rudan mod referansları temizlendi

## [1.23.0] - 2026-03-25

### Yeni Özellikler
- **UAC’sız self-update** — MikroUpdate güncellemesi artık Windows Service üzerinden sessizce yapılıyor, admin onayı (UAC prompt) görünmüyor
  - Tray app indirdiği installer yolunu pipe üzerinden servise gönderiyor (`InstallSelfUpdate` komutu)
  - Servis (SYSTEM yetkileriyle) installer’ı `/SILENT /SUPPRESSMSGBOXES /NOPOSTLAUNCH=1` ile başlatıyor
  - Installer tamamlandıktan sonra tray app, P/Invoke (`WTSQueryUserToken` + `CreateProcessAsUser`) ile kullanıcı masaüstü oturumunda yeniden başlatılıyor
  - Servis mevcut değilse self-update için eski yöntem (doğrudan installer + UAC) fallback olarak korunuyor
  - `ServiceCommand` sınıfına `Data` property ve `InstallSelfUpdate` enum değeri eklendi
  - `PipeClient`’a veri parametreli `SendCommandAsync` overload’ı eklendi
  - Inno Setup’a `/NOPOSTLAUNCH=1` parametre desteği ve `ShouldPostInstallLaunch` kontrol fonksiyonu eklendi

### Kaldırılanlar
- **Doğrudan mod (fallback) kaldırıldı** — Tüm güncelleme işlemleri artık Windows Service üzerinden yapılıyor
  - `CheckVersionsDirectAsync` ve `RunUpdateDirectAsync` metotları kaldırıldı
  - Servis çalışmıyorsa kullanıcıya bilgi mesajı gösterilir, güncelleme engellenir
  - README, FEATURES, INSTALL dökümanlarından doğrudan mod referansları temizlendi

## [1.22.2] - 2026-03-25

### Düzeltmeler
- **Servis durdurulduğunda tray menü durumu güncellenmiyor hatası düzeltildi**
  - `UpdateServiceStatus` içinde `ServiceController.Refresh()` eklenerek stale (önbellekli) durum sorunu giderildi
  - `RunScCommandAsync` artık `sc.exe` çıkış kodunu kontrol ediyor; yetki hatası gibi başarısızlıklar sessizce yutulmak yerine kullanıcıya bildiriliyor
  - `RunServiceCommandAsync` ve `TsmServiceRestart_Click` içinde `WaitForServiceStatusAsync` ile servisin hedef duruma geçmesi bekleniyor
  - Hata durumunda da `UpdateServiceStatus()` çağrılarak menünün her zaman güncel kalması sağlandı
  - `catch (InvalidOperationException)` yerine `catch (Exception)` ile `Win32Exception` gibi yakalanmayan hatalar da ele alındı

## [1.22.1] - 2025-07-22

### Düzeltmeler
- **Self-update yeni versiyon yüklenmiyor hatası düzeltildi** — Installer sessiz modda çalıştığında Restart Manager (RM) uygulamayı kapatamıyor, dosyalar kilitli kalıyordu
  - `OnFormClosing` artık self-update sırasında RM close isteklerine izin veriyor (`_selfUpdateInProgress` bayrağı)
  - `PrepareToInstall`’da servis dosya kopyalama öncesi durduruluyor (dosya kilidi önleme)
  - `RestartApplications=no` ile `[Run]` girişleriyle çift başlatma riski giderildi

## [1.22.0] - 2025-07-22

### Değiştirilenler
- **Başlat düğmesi kaldırıldı** — Güncelleme sonrası Mikro zaten otomatik başlatılıyor (`AutoLaunchAfterUpdate`), ayrı bir başlat düğmesine gerek kalmadı
  - `_btnLaunch` ("▶ Başlat") butonunun UI'dan ve tüm koddan kaldırıldı
  - `LaunchMikro()` metodu `LaunchMikroExe()` olarak yeniden adlandırıldı (sadece güncelleme sonrası otomatik başlatma için)
  - `SetUIBusy()` temizlendi

## [1.21.1] - 2025-07-22

### Düzeltmeler
- **Async performans iyileştirmesi** — UI thread donmasını önlemek için versiyon kontrol fonksiyonları asenkron yapıldı
  - `VersionService.GetVersion` → `GetVersionAsync` (Task.Run ile UNC ağ I/O)
  - `VersionService.GetModuleVersions` → `GetModuleVersionsAsync`
  - `VersionService.IsUpdateRequired` → `IsUpdateRequiredAsync`
  - `Form1.CheckVersionsDirect` → `CheckVersionsDirectAsync`
  - `RunUpdateDirectAsync` içindeki sync versiyon kontrolleri async'e çevrildi
  - Yavaş UNC ağ yollarında UI donma riski giderildi

## [1.21.0] - 2025-07-21

### Eklenenler
- **Kaynak türü bilgilendirme** — Modül tablosuna KAYNAK kolonu eklendi
  - Her modül için kaynak türü gösterilir: 🌐 CDN veya 📁 Yerel
  - Tooltip'lerde sunucu tam yolu (UNC path veya CDN URL) görünür
  - Terminal hücresinde yerel kurulum yolu tooltip olarak gösterilir
  - Header bilgisinde güncelleme modu gösterilir (🌐 Online / 🔀 Hybrid / 📁 Yerel)
  - ModuleVersionInfo'ya SourceType ve ServerPath alanları eklendi

## [1.20.0] - 2025-07-21

### Eklenenler
- **Modern indirme ilerleme paneli** — Double-buffered custom panel ile titremesiz, modern indirme göstergesi
  - Rounded gradient progress bar (GDI+ custom paint)
  - Modül adı, boyut, yüzde ve hız bilgisi tek panelde
  - Eski TLP + 4 label yaklaşımı kaldırıldı (layout flicker düzeltildi)
  - Self-update indirmesi de yeni panel ile entegre

## [1.19.2] - 2025-07-20

### Değişiklikler
- **Tray menü dikey versiyon sidebar** — Sol kenarda yeşil gradient sidebar üzerine uygulama adı ve versiyon dikey yazılır
  - Koyu tema uyumlu custom renderer (VersionSidebarRenderer)
  - Hover efekti sidebar alanına binmez
  - NotifyIcon tooltip'inde versiyon bilgisi gösterilir
- **SignPath.io code signing kaldırıldı** — Proje henüz yeterli topluluk görünürlüğüne sahip olmadığından başvuru reddedildi
  - Release workflow'dan imzalama referansları temizlendi
  - `docs/CODE_SIGNING.md` imzasız durum bilgisi ile güncellendi
  - OSS başvuru formu silindi

## [1.19.1] - 2025-07-20

### İyileştirmeler
- **SignPath otomatik imzalama pipeline testi** — Code signing altyapısının GitHub Actions üzerinden otomatik çalıştığını doğrulama amaçlı test release

## [1.19.0] - 2025-07-19

### Eklenenler
- **SignPath.io code signing entegrasyonu** — Windows SmartScreen uyarısını önlemek için installer dijital imzalama altyapısı
  - GitHub Actions workflow'una `sign-installer` job eklendi
  - `SIGNPATH_SIGNING_ENABLED` değişkeni ile koşullu aktifleştirme
  - Installer artifact olarak yükleniyor, SignPath imzalayıp release'e ekliyor
  - `docs/CODE_SIGNING.md` kurulum ve yapılandırma rehberi oluşturuldu

## [1.18.5] - 2025-07-19

### Düzeltmeler
- **Installer ayarları programa yansımıyor hatası giderildi** — `WriteConfigFile` prosedüründeki `if not FileExists` koşulu kaldırıldı; kurulum sihirbazında yapılan ayarlar (ürün, sürüm, yollar, modüller) artık her kurulumda `config.json` dosyasına yazılıyor

### İyileştirmeler
- **Installer mevcut ayarları okuyor** — Upgrade kurulumda `config.json` okunarak installer ekranı mevcut ayarlarla senkronize ediliyor
  - Ürün (Jump/Fly), sürüm (V16/V17), yollar, güncelleme modu ve modül durumları otomatik dolduruluyor
  - UI'da olmayan alanlar (ProxyAddress, HttpTimeoutSeconds, CheckIntervalMinutes, AutoLaunchAfterUpdate, CdnBaseUrl) korunuyor

## [1.18.4] - 2025-07-18

### Eklenenler
- **Tray menüsüne servis kontrolü eklendi** — MikroUpdateService durumu görüntüleme ve yönetim
  - Servis durumu göstergesi (Çalışıyor / Durduruldu / Kurulu değil)
  - Başlat / Durdur / Yeniden Başlat komutları
  - Menü açılırken durum otomatik güncelleniyor
  - `System.ServiceProcess.ServiceController` NuGet paketi eklendi

### Düzeltmeler
- **CS8602 nullable uyarıları giderildi** — `UpdateWorker.cs`'de `_onlineVersionService` ve `_downloadService` null guard eklendi

### İyileştirmeler
- **Copilot direktifine nullable kuralı eklendi** — CS8600–CS8604 uyarılarının önlenmesi için standart belirlendi

## [1.18.3] - 2025-07-18

### Düzeltmeler
- **Servis kurulumu zorunlu yapıldı** — MikroUpdateService artık her kurulumda otomatik kurulur
  - `[Tasks]` bölümünden servis task kaldırıldı (artık opsiyonel değil)
  - Servis kurulumu `CurStepChanged(ssPostInstall)` içinde Pascal Script ile yapılır
  - `sc create` komutu düzeltildi (`binPath=` sonrası boşluk eksikliği giderildi)
  - Upgrade kurulumda servis otomatik durdur/sil/yeniden oluştur mantığı eklendi
  - Çökme sonrası otomatik yeniden başlatma yapılandırması eklendi (5sn/10sn/30sn)
- **Servis bulunamadı uyarısı iyileştirildi** — Kullanıcıya hata mesajı ve yönlendirme gösteriliyor
- **Dark theme düğme görünürlüğü düzeltildi** — FlatStyle.Flat düğmelere belirgin renk/kenarlık eklendi
  - Header TLP AutoSize eklendi, FlowLayoutPanel MinimumSize ayarlandı
  - DataGridView kenarlık stili FixedSingle olarak değiştirildi

## [1.18.2] - 2025-07-18

### Düzeltmeler
- **Self-update installer uyumluluğu** — Self-update sonrası uygulama düzgün yeniden başlatılıyor
  - `PrepareToInstall`’dan `taskkill` kaldırıldı (Restart Manager ile çakışıyordu)
  - `RestartApplications=yes` ile upgrade sonrası uygulama yeniden başlatılıyor
  - Sessiz kurulum sonrası app otomatik başlatılıyor (`[Run]` postinstall `skipifnotsilent`)
  - App kendini kapatmıyor, installer `CloseApplications=force` ile kapatıyor
  - Installer argümanları `/SILENT /SUPPRESSMSGBOXES` olarak sadeleştirildi
  - Copilot direktiflerine Installer & Self-Update Yaşam Döngüsü kuralları eklendi

## [1.18.1] - 2025-07-18

### Düzeltmeler
- **Installer kaldırma temizliği** — Uninstall sonrası tray icon ve servis düzgün kaldırılıyor
  - `taskkill /F /IM MikroUpdate.exe` ile tray uygulaması kaldırma öncesi kapatılıyor
  - Servis stop → 3sn bekleme → servis delete sıralaması eklendi
  - Upgrade kurulumunda çalışan tray app otomatik kapatılıyor (`PrepareToInstall`)
  - `CloseApplications=force` ayarı ile Inno Setup yerleşik süreç kapatma aktifleştirildi

## [1.18.0] - 2025-07-18

### Eklenenler
- **Otomatik uygulama güncellemesi (Self-Update)** — MikroUpdate kendini güncelleyebiliyor
  - GitHub Releases API üzerinden yeni sürüm kontrolü
  - Uygulama başlangıcında arka planda otomatik kontrol
  - Tray menüsünde "Uygulama Güncellemesi" seçeneği
  - İlerleme çubuğuyla installer indirme
  - Sessiz kurulum ve uygulama yeniden başlatma

## [1.17.1] - 2025-07-18

### Düzeltmeler
- **Assembly adı düzeltmesi** — Exe adı `MikroUpdate.Win.exe` → `MikroUpdate.exe` olarak değiştirildi
  - Windows bildirimlerinde artık "MikroUpdate" görünüyor ("MikroUpdate.Win" yerine)
  - Installer exe referansı güncellendi

## [1.17.0] - 2025-07-18

### Kaldırılanlar
- **AI modülü tamamen kaldırıldı** — Gemini API entegrasyonu güvenilmez olduğu için projeden çıkarıldı
  - `GeminiService`, `AiVersionService`, `AiKeyManager` sınıfları silindi
  - `UpdateMode.AI` enum değeri kaldırıldı
  - `UpdateConfig` modelinden `GeminiApiKey` ve `MikroUpdatePageUrl` alanları kaldırıldı
  - SettingsForm'dan AI kontrolleri (API anahtarı, sayfa URL) kaldırıldı
  - Installer'dan AI modu seçeneği kaldırıldı
  - `System.Security.Cryptography.ProtectedData` NuGet bağımlılığı kaldırıldı
  - `MikroUpdate.Service.Tests` test projesi kaldırıldı

## [1.16.0] - 2025-07-17

### Eklenenler
- **GeminiService entegrasyon testleri** — xUnit test projesi (`MikroUpdate.Service.Tests`)
  - 8 entegrasyon testi: basit versiyon, çoklu versiyon, V17 filtreleme, prompt injection, gerçek sayfa, script temizleme, geçersiz API key, versiyon bulunamama
  - Rate limit toleransı: kota aşıldığında retry + açık bilgilendirme mesajı
  - `[Trait("Category", "Integration")]` ile CI filtresi

### Düzeltmeler
- **ParseVersionFromResponse regex düzeltmesi** — Gemini markdown/bold/backtick formatlaması artık tolere ediliyor
  - Eski: `Version.TryParse(line)` — tüm satırın versiyon olması gerekiyordu
  - Yeni: Regex `(\d+\.\d+\.\d+\.\d+)` ile satır içinden çıkarma, birden fazla eşleşmede en yüksek seçim
- **GeminiService dispose sahipliği** — `AiVersionService` artık sahip olmadığı `GeminiService`'i dispose etmiyor, double dispose önlendi

## [1.15.0] - 2025-07-17

### Eklenenler
- **Proxy ve timeout desteği** — Tüm HTTP servislerinde merkezi yapılandırma (Aşama 8)
  - `HttpClientFactory`: Merkezi HttpClient oluşturucu — proxy, timeout, SocketsHttpHandler tek noktada
  - `UpdateConfig`: `ProxyAddress` ve `HttpTimeoutSeconds` alanları
  - `SettingsForm`: Proxy adresi ve HTTP zaman aşımı UI alanları (Online/Hybrid/AI modlarında görünür)
  - `UpdateWorker`: Config reload'da HTTP servisleri yeniden oluşturulur (`InitializeHttpServices`)
- **İndirme retry mekanizması** — Exponential backoff ile otomatik tekrar deneme
  - 3 deneme, 2s/4s/8s bekleme süreleri
  - 4xx hatalarında (kalıcı) tekrar deneme atlanır
  - İlerleme bildiriminde deneme durumu gösterilir

### Değiştirilenler
- Tüm HTTP servisleri (`OnlineVersionService`, `DownloadService`, `GeminiService`, `AiVersionService`) `HttpClientFactory` üzerinden oluşturulur
- `UpdateWorker` servisleri lazy başlatır: config yüklenince `InitializeHttpServices()` çağrılır

## [1.14.0] - 2025-07-17

### Eklenenler
- **Gemini AI modu** — Yapay zeka destekli versiyon tespiti (Aşama 7)
  - `GeminiService`: Google Gemini API (gemini-2.0-flash) istemcisi, prompt injection koruması, HTML→düz metin dönüşümü
  - `AiVersionService`: Güncelleme sayfası indirme → Gemini ile versiyon çıkarma → CDN koduna dönüştürme
  - `AiKeyManager`: DPAPI (DataProtectionScope.LocalMachine) ile API anahtarı şifreleme/çözme
  - `UpdateConfig`: GeminiApiKey (şifreli) ve MikroUpdatePageUrl alanları
  - `SettingsForm`: AI modu UI — API anahtarı (maskelenmiş) ve güncelleme sayfası URL alanları, mod bazlı görünürlük
  - `UpdateWorker`: AI modu ayrı rota — AiVersionService üzerinden versiyon kontrolü, CDN kod seçimi

## [1.13.0] - 2025-07-16

### Eklenenler
- **Hybrid mod desteği** — Yerel sunucu → CDN fallback mekanizması
  - `HandleCheckVersionAsync`: Önce yerel sunucudan versiyon kontrolü, sunucu erişilemezse CDN probe
  - `HandleDownloadUpdateAsync`: Her modül için önce yerel sunucudan kopyalama, başarısızsa CDN indirme
  - Modül bazlı fallback — bazı modüller yerel, bazıları CDN'den alınabilir
  - Hata mesajlarında kaynak bilgisi (yerel sunucu ve CDN)

## [1.12.0] - 2025-07-16

### Eklenenler
- **Online versiyon kontrolü** — `OnlineVersionService`: CDN HTTP HEAD probe ile en güncel Mikro versiyonunu tespit eder
- **CDN indirme servisi** — `DownloadService`: HTTP ile setup dosyası indirme, ilerleme callback'i, hız hesaplama, geçici dosya yönetimi
- **Pipe progress streaming** — `DownloadUpdate` komutu ile canlı ilerleme bilgisi (yüzde, hız, durum metni) pipe üzerinden akış
- **Online güncelleme UI** — Form1'de CDN indirme ilerlemesi (ProgressBar yüzde, durum etiketi, tray bildirimi)
- **Mod yönlendirme** — `RunUpdateAsync` güncelleme moduna göre otomatik yönlendirme (Local → servis/doğrudan, Online → CDN, Hybrid/AI → CDN)
- **Mesaj tipleri** — `ServiceStatus.Downloading`, `DownloadProgressInfo`, `CommandType.DownloadUpdate`, `ServiceResponse.IsProgressMessage`
- **PipeClient progress** — `SendCommandWithProgressAsync` ile çoklu mesaj okuma (ara ilerleme + terminal yanıt)

## [1.11.0] - 2025-07-15

### Eklenenler
- **Güncelleme modu desteği** — `UpdateMode` enum: Local (varsayılan), Online, Hybrid, AI
  - `config.json`'a `UpdateMode` ve `CdnBaseUrl` alanları eklendi
  - Ayarlar formuna Güncelleme Modu ComboBox ve CDN URL TextBox eklendi
  - CDN alanları sadece Online/Hybrid/AI modlarında görünür
  - Installer'a Güncelleme Modu seçimi eklendi (varsayılan: Local)
- **CDN versiyon kodlama yardımcısı** — `CdnHelper` sınıfı: encode/decode/URL oluşturma/probe aday üretici
- **Ürün/modül dosya adı matrisi** — `MikroProductMatrix` sınıfı: V16/V17 Jump/Fly setup ve exe dosya adı lookup

### Etkilenen Dosyalar
- `MikroUpdate.Shared/Models/UpdateMode.cs` (yeni)
- `MikroUpdate.Shared/Helpers/CdnHelper.cs` (yeni)
- `MikroUpdate.Shared/Helpers/MikroProductMatrix.cs` (yeni)
- `MikroUpdate.Shared/Models/UpdateConfig.cs` (değişti)
- `MikroUpdate.Win/SettingsForm.cs` (değişti)
- `MikroUpdate.Win/SettingsForm.Designer.cs` (değişti)
- `Deployment/MikroUpdate.iss` (değişti)

## [1.10.1] - 2025-07-15

### Düzeltmeler
- **Yanıltıcı "Güncel" durumu düzeltildi** — Sunucu erişilemez olduğunda durum artık "Güncel" yerine "Sunucu erişilemiyor" (turuncu) olarak gösteriliyor
  - Servis modu (`CheckVersionsViaServiceAsync`) artık `ServerVersion` null olan modülleri doğru algılıyor

### Etkilenen Dosyalar
- `MikroUpdate.Win/Form1.cs` (değişti)

## [1.10.0] - 2025-07-15

### Eklenenler
- **Hakkında (About) dialog penceresi** — program bilgisi, versiyon, geliştirici, lisans, teknolojiler ve bağlantılar
  - Versiyon bilgisi assembly'den otomatik okunur
  - GitHub ve e-posta linkleri tıklanabilir
  - Ana form buton barına ve tray menüsüne "Hakkında" öğesi eklendi

### Etkilenen Dosyalar
- `MikroUpdate.Win/AboutForm.cs` (yeni)
- `MikroUpdate.Win/AboutForm.Designer.cs` (yeni)
- `MikroUpdate.Win/Form1.cs` (değişti)
- `MikroUpdate.Win/Form1.Designer.cs` (değişti)

## [1.9.0] - 2025-07-15

### Eklenenler
- **Kurulum sırasında modül seçimi** — Inno Setup sihirbazında Client, e-Defter, Beyanname checkbox'ları ile modül seçimi
  - Client zorunlu modül (her zaman seçili, devre dışı bırakılamaz)
  - e-Defter ve Beyanname varsayılan seçili değil, kullanıcı isteğe bağlı işaretler
  - Seçim `config.json`'a `Enabled: true/false` olarak yansır
- **V16/V17 yol otomatik güncelleme** — sürüm değiştirildiğinde tüm yol alanları (sunucu, terminal, setup) otomatik güncellenir
- **Varsayılan ayarlar düğmesi** — ayarlar formunda "Varsayılan" butonu ile tüm alanları fabrika değerlerine sıfırlama
- Varsayılan ayarlar seçili sürüme (V16/V17) göre dinamik yol oluşturma

### Değişenler
- SettingsForm: `UpdateVersionPaths()` metodu — V16↔V17 geçişinde yol alanlarını otomatik günceller
- SettingsForm: `BtnDefaults_Click` — seçili MajorVersion'a göre dinamik varsayılan yollar
- Inno Setup: `OnMajorVersionChange` callback — installer'da V16↔V17 geçişinde yolları günceller
- Inno Setup: Modül seçim checkbox'ları ve `BoolToStr` helper fonksiyonu eklendi
- Inno Setup: `GetModulesJson` — checkbox durumlarına göre `Enabled` alanını dinamik üretir

### Etkilenen Dosyalar
- `MikroUpdate.Win/SettingsForm.cs` (değişti)
- `MikroUpdate.Win/SettingsForm.Designer.cs` (değişti)
- `Deployment/MikroUpdate.iss` (değişti)
- `.github/copilot-instructions.md` (değişti)

## [1.7.0] - 2025-07-13

### Eklenenler
- **V16/V17 çoklu modül desteği** — ana sürüm (V16/V17) ve ürün (Jump/Fly) bazlı yapılandırma
- `UpdateModule` model sınıfı — her modül kendi setup dosyası ve EXE dosyasını tanımlar
- `ModuleVersionInfo` — modül bazlı versiyon bilgisi (terminal/sunucu/güncelleme durumu)
- Modül matrisi: Client, e-Defter, Beyanname modülleri her ürün+sürüm kombinasyonu için otomatik oluşturulur
- Form1: DataGridView ile modül bazlı versiyon durumu gösterimi (MODÜL/TERMINAL/SUNUCU/DURUM)
- SettingsForm: Ana Sürüm (V16/V17) combo, modül DataGridView (aktif/pasif, setup/exe düzenleme), Sıfırla butonu
- Sıralı çoklu modül güncelleme — her modül için ayrı setup kopyalama ve sessiz kurulum
- Inno Setup: Ana sürüm ve ürün seçimine göre otomatik modül listesi oluşturma
- `UpdateConfig.EnsureModules()` — boş modül listesi için varsayılan modülleri otomatik doldurur
- `UpdateConfig.GetDefaultModules()` — ürün+sürüm bazlı fabrika metodu

### Değişenler
- `UpdateConfig`: `SetupFileName` → `List<UpdateModule> Modules`, `MajorVersion` eklendi
- `ServiceResponse`: `List<ModuleVersionInfo> ModuleVersions` eklendi
- `VersionService` (Win+Service): çoklu modül versiyon kontrolü (`GetModuleVersions`)
- `UpdateWorker`: sıralı çoklu modül güncelleme, modül bazlı süreç kapatma
- Form1.Designer: 3 sütunlu status paneli → DataGridView + başlık paneli
- SettingsForm.Designer: tekli setup alanı → modül grid + ana sürüm combo
- Inno Setup: `SetupFileName` alanı kaldırıldı, `MajorVersion` combo eklendi, modül JSON üretimi
- Ayarlar kaydedildikten sonra otomatik versiyon kontrolü başlatılır
- Inno Setup: .NET 10 Desktop Runtime yüklü değilse otomatik indirme ve sessiz kurulum (`PrepareToInstall`)

### Etkilenen Dosyalar
- `MikroUpdate.Shared/Models/UpdateModule.cs` (yeni)
- `MikroUpdate.Shared/Models/UpdateConfig.cs` (değişti)
- `MikroUpdate.Shared/Messages/ServiceResponse.cs` (değişti)
- `MikroUpdate.Win/Services/VersionService.cs` (değişti)
- `MikroUpdate.Win/Form1.cs` (değişti)
- `MikroUpdate.Win/Form1.Designer.cs` (değişti)
- `MikroUpdate.Win/SettingsForm.cs` (değişti)
- `MikroUpdate.Win/SettingsForm.Designer.cs` (değişti)
- `MikroUpdate.Service/Services/VersionService.cs` (değişti)
- `MikroUpdate.Service/UpdateWorker.cs` (değişti)
- `Deployment/MikroUpdate.iss` (değişti)

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

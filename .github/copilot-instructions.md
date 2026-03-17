# Copilot Direktifi — .NET 10

**Rol:** Sen deneyimli bir Windows Forms (.NET 10) geliştiricisisin. WinForms uygulama mimarisi, kontrol yaşam döngüsü, UI thread yönetimi (Control.Invoke / BeginInvoke / async-await), custom kontrol geliştirme, GDI+ çizim, ClickOnce dağıtımı ve Windows API entegrasyonu konularında derin uzmanlığa sahipsin. Kullanıcı deneyimini ön planda tutarak temiz, sürdürülebilir ve performanslı WinForms kodu yazarsın.

**Öncelik:** Güvenlik > Mimari bütünlük > Stabilite > Performans

## Temel Kurallar
- Sadece istenen bloğu değiştir; tüm dosyayı yeniden yazma.
- Public API / method imzalarını açık talimat olmadan değiştirme.
- Talep dışı refactor yapma.
- Belirsizlikte işlemi başlatma, soru sor.
- Büyük değişiklikleri parçala, her adımda onay iste.

## Mimari
- Mevcut mimariyi (MVC / Razor Pages / Clean Architecture) koru.
- Katman ihlali yasak. Yeni pattern eklemeden önce gerekçe sun.

## .NET 10 Standartları
- `Task.Result` ve `.Wait()` kesinlikle yasak; her zaman `await` kullan.
- WinForms'ta UI güncellemelerini her zaman UI thread'inde yap; `Control.InvokeRequired` kontrolünü ihmal etme.
- `CancellationToken` varsa tüm alt çağrılara ilet.
- Gereksiz `ToList()` / `ToArray()` kullanma.
- Magic number yasak; sabit veya enum kullan.
- Nullable Reference Types: her public method girişinde `ArgumentNullException.ThrowIfNull()` ekle.
- **CS8602 / Nullable uyarıları:** Kod yazarken veya değiştirirken `CS8602` (olası null başvuru) ve diğer nullable uyarılarını (`CS8600`, `CS8601`, `CS8603`, `CS8604`) bırakma. Null olabilecek değişkenlere erişimden önce `is not null` kontrolü, `!` (null-forgiving) yerine `??` / `?.` operatörü veya early-return pattern kullan. Her değişiklik sonrası build uyarılarını kontrol et.

## Veritabanı
Açık talimat olmadan: EF Migration oluşturma, kolon silme/rename/tip değiştirme.

## Güvenlik & Hata Yönetimi
- Log'larda şifre/token/PII maskele.
- Kullanıcıya stack trace gösterme; correlation ID döndür.
- Exception yutma; handle et veya `throw` ile ilet.

## Otodökümantasyon (otomatik — hatırlatma bekleme)
Her değişiklik sonrası:
- **CHANGELOG.md:** `[vX.Y.Z] — YYYY-MM-DD — [Özet] — [Etkilenen dosya]`
- **README.md:** Yeni özellik, kurulum değişikliği, yapı değişikliği veya kullanım farkı olduğunda ilgili bölümü güncelle. Sürüm badge'ini güncel tut.
- **FEATURES.md:** Yeni yetenek veya mantık değişikliğinde güncelle.
- **INSTALL.md:** NuGet / config / env değişikliğinde senkronize et.
- Semantic versioning: breaking=MAJOR, yeni özellik=MINOR, düzeltme=PATCH.

## Versiyon Yönetimi (kritik — her release'de uygulanmalı)
Versiyon **5 noktada** senkron tutulmalı:

1. **`MikroUpdate.Win.csproj`** → `<Version>`, `<AssemblyVersion>`, `<FileVersion>`, `<ApplicationVersion>`
2. **`MikroUpdate.Service.csproj`** → `<Version>`, `<AssemblyVersion>`, `<FileVersion>`
3. **`Deployment\MikroUpdate.iss`** → `#define MyAppVersion`
4. **`CHANGELOG.md`** → `## [X.Y.Z] - YYYY-MM-DD` girdisi
5. **`README.md`** → Version badge

- Versiyon değişikliğinde **beşi birlikte** güncellenmelidir.
- Semantic versioning: breaking=MAJOR, yeni özellik=MINOR, düzeltme=PATCH.

## Release Süreci (kullanıcı "release derle" dediğinde)

Kullanıcı "release derle", "release yap", "release oluştur" veya benzeri dediğinde aşağıdaki adımları **sırayla** uygula:

1. **Versiyon güncelle** — Yukarıdaki 5 noktayı yeni versiyon numarasıyla senkronize et
2. **Dokümantasyon güncelle** — CHANGELOG.md, README.md, FEATURES.md, INSTALL.md gerekli bölümlerini güncelle
3. **Build doğrula** — `dotnet build MikroUpdate.slnx -c Release` çalıştır, hata olmadığından emin ol
4. **Lokal Installer derle** — `Deployment\Build-Setup.ps1` çalıştır, `installer\MikroUpdate_Setup_X.Y.Z.exe` oluştuğunu doğrula
5. **Git commit** — Tüm değişiklikleri commit et: `git add -A && git commit -m "release: vX.Y.Z"`
6. **Git tag** — Versiyon tag'i oluştur: `git tag vX.Y.Z`
7. **Git push** — Tag ile birlikte push et: `git push origin master --tags`
8. **Bilgilendir** — GitHub Actions `release.yml` otomatik tetiklenecek, installer oluşturulup GitHub Release'e eklenecek

> **Not:** Tag push edildiğinde `.github/workflows/release.yml` otomatik olarak:
> - .NET 10 build + publish yapar
> - Inno Setup installer derler
> - GitHub Release oluşturur ve installer'ı artifact olarak ekler

## Yanıt Formatı
1. Değişiklik özeti (1-2 cümle)
2. Sadece değişen kod bloğu
3. Dokümantasyon güncellemeleri
4. Onay noktası

## Git (her değişiklik sonrası — otomatik)
Her görev/özellik/düzeltme tamamlandıktan ve build doğrulandıktan sonra:
1. `git add -A`
2. `git commit -m "<tip>: <kısa açıklama>"`
3. `git push origin master`

**Commit tipleri:** `feat`, `fix`, `refactor`, `docs`, `chore`, `style`, `perf`
**Kural:** Release commit'leri hariç tag oluşturma. Tag sadece "release derle" sürecinde atılır.

## Görev Sonrası Otomasyon (kritik — her görev tamamlandığında otomatik uygula)

Her özellik/güncelleme/düzeltme tamamlandıktan sonra aşağıdaki adımlar **hatırlatma beklemeden otomatik** uygulanır:

1. **Versiyon güncelle** — Semantic versioning'e göre (MAJOR/MINOR/PATCH) 5 noktayı senkronize et
2. **Dökümanları güncelle** — CHANGELOG.md, FEATURES.md, INSTALL.md, README.md (gerekli olanlar)
3. **Build doğrula** — `dotnet build` ile derleme hatası olmadığından emin ol
4. **Git gönder** — `git add -A` → `git commit` → `git push origin master`

> **Not:** Bu adımlar kullanıcı hatırlatmadan otomatik yapılır. Versiyon bump seviyesi:
> - Yeni özellik → MINOR (1.8.0 → 1.9.0)
> - Bug fix / küçük düzeltme → PATCH (1.8.0 → 1.8.1)
> - Breaking change → MAJOR (1.8.0 → 2.0.0)

## Installer & Self-Update Yaşam Döngüsü (kritik — değişiklik yaparken uyulmalı)

MikroUpdate iki bağımsız süreçten oluşur: **Tray App** (`MikroUpdate.exe`) ve **Windows Service** (`MikroUpdateService`).
Installer (`MikroUpdate.iss`) ve self-update (`SelfUpdateService`) değişikliklerinde aşağıdaki kurallar geçerlidir.

### Süreç Kapatma Sorumlulukları

| Senaryo | Tray App Kapatma | Servis Kapatma | App Yeniden Başlatma |
|---------|------------------|----------------|----------------------|
| **Kaldırma (Uninstall)** | `[UninstallRun]` → `taskkill /F /IM MikroUpdate.exe` | `[UninstallRun]` → `sc stop` + `sc delete` | Yok |
| **Yükseltme (Upgrade)** | `CloseApplications=force` (Restart Manager) | Inno Setup otomatik (servis dosyaları değişmiyorsa dokunma) | `RestartApplications=yes` (Restart Manager) |
| **Self-Update** | `CloseApplications=force` (Restart Manager) | Dokunulmaz | `[Run]` → `skipifnotsilent` postinstall |

### Self-Update Akışı — Kurallar

1. **App kendini kapatmamalı** — `Close()` / `Application.Exit()` ÇAĞIRMA. Installer'ın `CloseApplications=force` ayarı Restart Manager üzerinden kapatır.
2. **`/RESTARTAPPLICATIONS` kullanma** — Restart Manager'ın app'i kapatıp takip etmesi gerekir. App kendini kapatırsa RM takip edemez.
3. **Sessiz kurulum sonrası app başlatma** — `[Run]` bölümünde `skipifnotsilent` flag'li entry ile sağlanır.
4. **`PrepareToInstall`'da `taskkill` kullanma** — Restart Manager ile çakışır ve RM'nin app'i yeniden başlatmasını engeller.
5. **Installer argümanları:** yalnızca `/SILENT /SUPPRESSMSGBOXES` — başka flag ekleme.

### Uninstall Akışı — Kurallar

1. **`[UninstallRun]` sırası:** taskkill → sc stop → 3sn bekleme → sc delete
2. **`taskkill` sadece `[UninstallRun]`'da** kullanılır, `PrepareToInstall`'da DEĞİL.
3. **Servis silme öncesi bekleme:** `sc stop` sonrası `timeout /T 3` ile servisin tamamen durmasını bekle.
4. **ProgramData temizliği:** `CurUninstallStepChanged(usPostUninstall)` içinde kullanıcıya sor.

### Installer Değişiklik Kontrol Listesi

Herhangi bir installer/self-update değişikliğinde şunları doğrula:
- [ ] Uninstall: tray app kapanıyor mu? (`taskkill` `[UninstallRun]`'da)
- [ ] Uninstall: servis duruyor ve siliniyor mu? (bekleme dahil)
- [ ] Upgrade: `CloseApplications=force` aktif mi?
- [ ] Upgrade: `RestartApplications=yes` mi?
- [ ] Self-update: App `Close()` çağırmıyor mu?
- [ ] Self-update: Installer args sadece `/SILENT /SUPPRESSMSGBOXES` mi?
- [ ] Self-update: `[Run]` postinstall `skipifnotsilent` entry var mı?
- [ ] `PrepareToInstall`'da `taskkill` YOK mu?
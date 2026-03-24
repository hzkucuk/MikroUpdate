# Gizlilik Politikası / Privacy Policy

> 🇹🇷 [Türkçe](#türkçe) | 🇬🇧 [English](#english)

---

## Türkçe

### Genel Bakış

MikroUpdate, Mikro ERP (Jump / Fly) yazılımları için tasarlanmış açık kaynaklı bir otomatik güncelleme sistemidir. Kullanıcı gizliliğine saygı gösterir ve **kişisel veri toplamaz, saklamaz veya üçüncü taraflara iletmez.**

### Ağ Bağlantıları

MikroUpdate aşağıdaki durumlarda ağ bağlantısı kurar. Tüm bağlantılar **yalnızca kullanıcı veya sistem yöneticisi tarafından yapılandırılan ayarlara göre** gerçekleşir:

| Özellik | Bağlantı Hedefi | Amaç | Tetikleyen |
|---------|-----------------|------|------------|
| **Yerel güncelleme** | Yerel ağdaki sunucu yolu (UNC path) | Mikro ERP güncelleme dosyalarını kontrol etme ve kopyalama | Kullanıcı yapılandırması (config.json) |
| **Online güncelleme** | CDN adresi (yapılandırılabilir) | HTTP HEAD isteği ile güncel sürüm kontrolü ve dosya indirme | Kullanıcı "Online" veya "Hybrid" modu seçtiğinde |
| **Otomatik uygulama güncellemesi** | `api.github.com` | MikroUpdate'in kendi yeni sürümünü kontrol etme | Uygulama başlangıcında (GitHub Releases API) |
| **Güncelleme indirme** | `github.com` | MikroUpdate installer dosyasını indirme | Kullanıcı güncellemeyi onayladığında |

### Toplanan Veriler

**Hiçbir kişisel veri toplanmaz.** Detaylı olarak:

- ❌ Kullanıcı kimlik bilgileri (isim, e-posta, IP adresi) toplanmaz
- ❌ Kullanım istatistikleri veya analitik verisi gönderilmez
- ❌ Telemetri veya hata raporlama sistemi bulunmaz
- ❌ Çerez (cookie) veya izleme mekanizması kullanılmaz
- ❌ Üçüncü taraf reklam veya analitik servisi entegre değildir

### Yerel Depolama

MikroUpdate aşağıdaki verileri **yalnızca yerel makinede** saklar:

- `%ProgramData%\MikroUpdate\config.json` — Kullanıcı yapılandırma ayarları
- `%ProgramData%\MikroUpdate\Logs\` — Uygulama ve servis log dosyaları

Log dosyaları yalnızca hata ayıklama amacıyla kullanılır ve hiçbir yere iletilmez.

### Üçüncü Taraf Hizmetleri

| Hizmet | Kullanım Amacı | Gizlilik Politikası |
|--------|----------------|---------------------|
| **GitHub API** | Uygulama sürüm kontrolü | [GitHub Privacy Statement](https://docs.github.com/en/site-policy/privacy-policies/github-general-privacy-statement) |
| **GitHub Releases** | Installer indirme | [GitHub Privacy Statement](https://docs.github.com/en/site-policy/privacy-policies/github-general-privacy-statement) |

GitHub'a yapılan istekler standart HTTP istekleridir ve GitHub'ın kendi gizlilik politikasına tabidir.

### İletişim

Gizlilik ile ilgili sorularınız için: [GitHub Issues](https://github.com/hzkucuk/MikroUpdate/issues)

---

## English

### Overview

MikroUpdate is an open-source automated update system designed for Mikro ERP (Jump / Fly) software. It respects user privacy and **does not collect, store, or transmit any personal data.**

### Network Connections

MikroUpdate establishes network connections in the following scenarios. All connections occur **only based on settings configured by the user or system administrator:**

| Feature | Connection Target | Purpose | Trigger |
|---------|-------------------|---------|---------|
| **Local update** | Server path on local network (UNC path) | Check and copy Mikro ERP update files | User configuration (config.json) |
| **Online update** | CDN address (configurable) | HTTP HEAD request for version check and file download | When user selects "Online" or "Hybrid" mode |
| **Application self-update** | `api.github.com` | Check for new MikroUpdate versions | On application startup (GitHub Releases API) |
| **Update download** | `github.com` | Download MikroUpdate installer | When user approves the update |

### Data Collection

**No personal data is collected.** Specifically:

- ❌ No user identity information (name, email, IP address) is collected
- ❌ No usage statistics or analytics data is sent
- ❌ No telemetry or crash reporting system is included
- ❌ No cookies or tracking mechanisms are used
- ❌ No third-party advertising or analytics services are integrated

### Local Storage

MikroUpdate stores the following data **only on the local machine:**

- `%ProgramData%\MikroUpdate\config.json` — User configuration settings
- `%ProgramData%\MikroUpdate\Logs\` — Application and service log files

Log files are used solely for debugging purposes and are never transmitted anywhere.

### Third-Party Services

| Service | Usage Purpose | Privacy Policy |
|---------|---------------|----------------|
| **GitHub API** | Application version check | [GitHub Privacy Statement](https://docs.github.com/en/site-policy/privacy-policies/github-general-privacy-statement) |
| **GitHub Releases** | Installer download | [GitHub Privacy Statement](https://docs.github.com/en/site-policy/privacy-policies/github-general-privacy-statement) |

Requests to GitHub are standard HTTP requests and are subject to GitHub's own privacy policy.

### Contact

For privacy-related questions: [GitHub Issues](https://github.com/hzkucuk/MikroUpdate/issues)

---

*Son güncelleme / Last updated: 2025-07-20*

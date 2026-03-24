# Code Signing — SignPath.io Entegrasyonu

MikroUpdate installer'ı Windows SmartScreen uyarısını önlemek için dijital olarak imzalanır.
Açık kaynak projeler için [SignPath.io](https://signpath.io) ücretsiz code signing sertifikası sağlar.

## Kurulum Adımları

### 1. SignPath.io Başvurusu

1. **https://signpath.io/open-source** adresine git
2. **"Apply for Open Source"** butonuna tıkla
3. GitHub repo URL'ini gir: `https://github.com/hzkucuk/MikroUpdate`
4. Başvuruyu tamamla ve onay bekle (genellikle birkaç gün)

### 2. SignPath Yapılandırması (onay sonrası)

SignPath panelinde:

1. **Organization ID**'yi not al
2. **Project** oluştur: `MikroUpdate`
3. **Artifact Configuration** oluştur: `installer`
   - Artifact türü: **PE (Portable Executable)**
   - Dosya deseni: `MikroUpdate_Setup_*.exe`
4. **Signing Policy** oluştur: `release-signing`
   - Sertifika tipi: **Public Trust** (SmartScreen için)
5. **CI User** → API Token oluştur

### 3. GitHub Repository Ayarları

GitHub repo → **Settings** → **Secrets and variables** → **Actions**:

#### Secrets
| Secret | Değer |
|--------|-------|
| `SIGNPATH_API_TOKEN` | SignPath'ten aldığın API token |

#### Variables
| Variable | Değer |
|----------|-------|
| `SIGNPATH_ORGANIZATION_ID` | SignPath organization ID |
| `SIGNPATH_SIGNING_ENABLED` | `true` (imzalamayı aktif eder) |

### 4. İmzalamayı Aktif Et

`SIGNPATH_SIGNING_ENABLED` değişkenini `true` yaptığında, her tag push'unda:

1. `build-and-release` job → installer derler, artifact yükler, release oluşturur
2. `sign-installer` job → SignPath'e imzalama isteği gönderir
3. İmzalı installer otomatik olarak GitHub Release'e eklenir

### Akış Diyagramı

```
Tag push (v*)
     │
     ▼
┌─────────────────────┐
│  build-and-release   │
│  ├─ .NET build       │
│  ├─ Inno Setup       │
│  ├─ Upload artifact  │
│  └─ Create release   │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│  sign-installer      │ ← SIGNPATH_SIGNING_ENABLED=true ise
│  ├─ SignPath request │
│  ├─ Wait for signing │
│  └─ Upload signed    │
└─────────────────────┘
```

## SSS

**S: SignPath başvurusu onaylanmadan ne olur?**
İmzalama job'ı `SIGNPATH_SIGNING_ENABLED` değişkeni `true` olmadıkça çalışmaz.
Mevcut release akışı aynen devam eder.

**S: SmartScreen uyarısı ne zaman kalkar?**
İmzalı installer'ı kullanıcılar indirip çalıştırdıkça SmartScreen reputasyonu otomatik artar.
EV sertifikası ile anında kalkar, OV sertifikası ile birkaç gün sürebilir.

**S: Maliyet?**
SignPath.io açık kaynak projeler için tamamen ücretsizdir.

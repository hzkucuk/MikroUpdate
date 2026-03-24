# Code Signing — SignPath.io Manuel İmzalama

MikroUpdate installer'ı Windows SmartScreen uyarısını önlemek için dijital olarak imzalanır.
Açık kaynak projeler için [SignPath.io](https://signpath.io) ücretsiz code signing sertifikası sağlar.

> **Not:** SignPath.io ücretsiz planı yalnızca manuel imzalamayı destekler.
> CI/CD entegrasyonu (GitHub Actions otomatik imzalama) için "Open Source Code Signing" programına
> ayrıca başvuru yapılması gerekir: https://signpath.io/open-source

## SignPath Bilgileri

| Alan | Değer |
|------|-------|
| Organization | Zafer Bilgisayar |
| Project slug | `MikroUpdate` |
| Signing policy | `MikroUpdate` |
| Artifact config | `Initial version` (DEFAULT) |
| Certificate | MikroUpdate (X.509 certificate) |
| Repository URL | https://github.com/hzkucuk/MikroUpdate |

## Manuel İmzalama Adımları

Her release sonrası installer'ı manuel olarak imzalamak için:

1. **GitHub Release'den installer'ı indirin**
   - GitHub Actions otomatik olarak `MikroUpdate_Setup_X.Y.Z.exe` dosyasını release'e ekler

2. **SignPath.io'da imzalama isteği oluşturun**
   - [app.signpath.io](https://app.signpath.io) → **Dashboard** → **Sign artifact** butonuna tıklayın
   - **Project:** MikroUpdate
   - **Signing policy:** MikroUpdate
   - **Artifact configuration:** Initial version
   - İndirdiğiniz `.exe` dosyasını yükleyin

3. **İmzalı dosyayı indirin**
   - İstek tamamlandığında (Status: ✅ Completed) imzalı installer'ı indirin

4. **GitHub Release'e imzalı sürümü ekleyin** (opsiyonel)
   - Release sayfasını düzenleyip imzalı `.exe`'yi ekleyebilirsiniz

## Release Akışı

```
Tag push (v*)
     │
     ▼
┌─────────────────────┐
│  GitHub Actions      │
│  ├─ .NET build       │
│  ├─ Inno Setup       │
│  ├─ Upload artifact  │
│  └─ Create release   │  ← İmzasız installer
└──────────┬──────────┘
           │
           ▼ (manuel)
┌─────────────────────┐
│  SignPath.io         │
│  ├─ Upload exe       │
│  ├─ Sign artifact    │
│  └─ Download signed  │  ← İmzalı installer
└─────────────────────┘
```

## SSS

**S: SmartScreen uyarısı ne zaman kalkar?**
İmzalı installer'ı kullanıcılar indirip çalıştırdıkça SmartScreen reputasyonu otomatik artar.
EV sertifikası ile anında kalkar, OV sertifikası ile birkaç gün sürebilir.

**S: Otomatik imzalama mümkün mü?**
SignPath.io "Open Source Code Signing" programına kabul edilirse CI entegrasyonu aktif edilebilir.
Başvuru: https://signpath.io/open-source

**S: Maliyet?**
SignPath.io açık kaynak projeler için tamamen ücretsizdir.

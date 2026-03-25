# Code Signing

MikroUpdate installer'ı şu anda **imzasız** olarak dağıtılmaktadır.
Windows SmartScreen ilk çalıştırmada uyarı gösterebilir.

## Durum

| Alan | Değer |
|------|-------|
| İmza durumu | ❌ İmzasız |
| SmartScreen | İlk çalıştırmada uyarı gösterir |
| Planlanan çözüm | Proje yeterli topluluk görünürlüğüne ulaştığında code signing eklenecek |

## SmartScreen Uyarısı

Kullanıcılar installer'ı çalıştırdığında "Windows protected your PC" uyarısı görebilir:

1. **"More info"** bağlantısına tıklayın
2. **"Run anyway"** butonuna tıklayın

Bu uyarı yalnızca ilk çalıştırmada görünür.

## Gelecek Planı

Proje yeterli topluluk büyüklüğüne (GitHub stars, forks, bağımsız referanslar) ulaştığında
code signing sertifikası alınarak SmartScreen uyarısı kaldırılacaktır.

Değerlendirilen seçenekler:
- [Azure Trusted Signing](https://learn.microsoft.com/azure/trusted-signing/) (~$10/ay)
- [SignPath.io Foundation](https://signpath.org) (açık kaynak, ücretsiz — topluluk gereksinimi var)

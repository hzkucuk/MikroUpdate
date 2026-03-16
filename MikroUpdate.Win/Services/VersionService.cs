using System.Diagnostics;

using MikroUpdate.Win.Models;

namespace MikroUpdate.Win.Services;

/// <summary>
/// Mikro ERP versiyon kontrol servisi.
/// Sunucu ve terminal EXE dosyalarının FileVersion bilgisini karşılaştırır.
/// </summary>
public sealed class VersionService
{
    /// <summary>
    /// Belirtilen EXE dosyasının versiyonunu okur.
    /// </summary>
    /// <returns>Versiyon bilgisi veya dosya bulunamazsa null.</returns>
    public Version? GetVersion(string exePath)
    {
        if (string.IsNullOrWhiteSpace(exePath) || !File.Exists(exePath))
        {
            return null;
        }

        FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(exePath);

        if (string.IsNullOrWhiteSpace(versionInfo.FileVersion))
        {
            return null;
        }

        return Version.TryParse(versionInfo.FileVersion, out Version? version) ? version : null;
    }

    /// <summary>
    /// Terminal ve sunucu versiyonlarını karşılaştırır.
    /// </summary>
    /// <returns>Güncelleme gerekli ise true.</returns>
    public bool IsUpdateRequired(UpdateConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        Version? localVersion = GetVersion(config.LocalExePath);
        Version? serverVersion = GetVersion(config.ServerExePath);

        // Sunucu versiyonu okunamazsa güncelleme yapılamaz
        if (serverVersion is null)
        {
            return false;
        }

        // Yerel kurulum yoksa güncelleme gerekli
        if (localVersion is null)
        {
            return true;
        }

        return localVersion < serverVersion;
    }
}

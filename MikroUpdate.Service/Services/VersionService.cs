using System.Diagnostics;

namespace MikroUpdate.Service.Services;

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
        ArgumentNullException.ThrowIfNull(exePath);

        if (!File.Exists(exePath))
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
}

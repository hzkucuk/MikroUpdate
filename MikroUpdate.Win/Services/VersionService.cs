using System.Diagnostics;

using MikroUpdate.Shared.Messages;
using MikroUpdate.Shared.Models;

namespace MikroUpdate.Win.Services;

/// <summary>
/// Mikro ERP versiyon kontrol servisi.
/// Sunucu ve terminal EXE dosyalarının FileVersion bilgisini karşılaştırır.
/// Çoklu modül desteği ile her modül için ayrı versiyon kontrolü yapar.
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
    /// Tüm aktif modüller için versiyon bilgilerini toplar.
    /// </summary>
    public List<ModuleVersionInfo> GetModuleVersions(UpdateConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        List<ModuleVersionInfo> results = [];

        foreach (UpdateModule module in config.EnabledModules)
        {
            string localPath = Path.Combine(config.LocalInstallPath, module.ExeFileName);
            string serverPath = Path.Combine(config.ServerSharePath, module.ExeFileName);

            Version? localVersion = GetVersion(localPath);
            Version? serverVersion = GetVersion(serverPath);

            bool updateRequired = serverVersion is not null
                && (localVersion is null || localVersion < serverVersion);

            results.Add(new ModuleVersionInfo
            {
                ModuleName = module.Name,
                LocalVersion = localVersion?.ToString(),
                ServerVersion = serverVersion?.ToString(),
                UpdateRequired = updateRequired
            });
        }

        return results;
    }

    /// <summary>
    /// Herhangi bir aktif modülde güncelleme gerekli mi kontrol eder.
    /// </summary>
    /// <returns>En az bir modülde güncelleme gerekli ise true.</returns>
    public bool IsUpdateRequired(UpdateConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        List<ModuleVersionInfo> versions = GetModuleVersions(config);

        return versions.Exists(v => v.UpdateRequired);
    }
}

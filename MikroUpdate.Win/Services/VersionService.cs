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
    /// Belirtilen EXE dosyasının versiyonunu asenkron olarak okur.
    /// UNC ağ yollarında UI thread'i bloke etmemek için Task.Run kullanır.
    /// </summary>
    /// <returns>Versiyon bilgisi veya dosya bulunamazsa null.</returns>
    public async Task<Version?> GetVersionAsync(string exePath)
    {
        if (string.IsNullOrWhiteSpace(exePath))
        {
            return null;
        }

        return await Task.Run(() =>
        {
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
        }).ConfigureAwait(false);
    }

    /// <summary>
    /// Tüm aktif modüller için versiyon bilgilerini asenkron olarak toplar.
    /// </summary>
    public async Task<List<ModuleVersionInfo>> GetModuleVersionsAsync(UpdateConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        List<ModuleVersionInfo> results = [];

        foreach (UpdateModule module in config.EnabledModules)
        {
            string localPath = Path.Combine(config.LocalInstallPath, module.ExeFileName);
            string serverPath = Path.Combine(config.ServerSharePath, module.ExeFileName);

            Version? localVersion = await GetVersionAsync(localPath).ConfigureAwait(false);
            Version? serverVersion = await GetVersionAsync(serverPath).ConfigureAwait(false);

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
    /// Herhangi bir aktif modülde güncelleme gerekli mi asenkron kontrol eder.
    /// </summary>
    /// <returns>En az bir modülde güncelleme gerekli ise true.</returns>
    public async Task<bool> IsUpdateRequiredAsync(UpdateConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        List<ModuleVersionInfo> versions = await GetModuleVersionsAsync(config).ConfigureAwait(false);

        return versions.Exists(v => v.UpdateRequired);
    }
}

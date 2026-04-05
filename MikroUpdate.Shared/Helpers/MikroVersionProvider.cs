using System.Reflection;
using System.Text.Json;

using MikroUpdate.Shared.Models;

namespace MikroUpdate.Shared.Helpers;

/// <summary>
/// Mikro ERP sürüm kataloğunu yükler ve erişim sağlar.
/// <para>
/// Öncelik sırası:
/// 1. ProgramData\MikroUpdate\mikro-versions.json (kullanıcı/admin tarafından düzenlenebilir)
/// 2. Gömülü varsayılan kaynak (embedded resource)
/// </para>
/// V18, C20 vb. yeni sürümler eklendiğinde derleme gerektirmez;
/// sadece JSON dosyası güncellenir.
/// </summary>
public sealed class MikroVersionProvider
{
    private const string ExternalFileName = "mikro-versions.json";
    private const string EmbeddedResourceName = "MikroUpdate.Shared.mikro-versions.json";

    private static readonly string ExternalFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "MikroUpdate",
        ExternalFileName);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    /// <summary>Varsayılan CDN URL pattern'i. Versiyon tanımında boşsa bu kullanılır.</summary>
    private const string DefaultCdnUrlPattern = "{cdnBase}/{cdnFolder}/{cdnCode}/{setupFile}";

    private readonly Lazy<MikroVersionCatalog> _catalog;

    public MikroVersionProvider()
    {
        _catalog = new Lazy<MikroVersionCatalog>(LoadCatalog);
    }

    /// <summary>Yüklenen sürüm kataloğu.</summary>
    public MikroVersionCatalog Catalog => _catalog.Value;

    /// <summary>
    /// Belirtilen sürüm tanımını döner. Bulunamazsa null.
    /// </summary>
    public MikroVersionDefinition? GetVersion(string versionName) =>
        Catalog.FindVersion(versionName);

    /// <summary>
    /// Tüm sürüm adlarını döner (ör: ["V16", "V17"]).
    /// </summary>
    public IReadOnlyList<string> GetVersionNames() =>
        Catalog.GetVersionNames();

    /// <summary>
    /// Belirtilen sürümdeki ürün adlarını döner (ör: ["Jump", "Fly"]).
    /// </summary>
    public IReadOnlyList<string> GetProductNames(string versionName) =>
        Catalog.GetProductNames(versionName);

    /// <summary>
    /// Sürüm tanımındaki versiyon tag'ini döner (ör: "V16" → "v16xx").
    /// Bulunamazsa "v16xx" fallback döner.
    /// </summary>
    public string GetVersionTag(string majorVersion)
    {
        MikroVersionDefinition? def = GetVersion(majorVersion);
        return !string.IsNullOrEmpty(def?.VersionTag) ? def.VersionTag : "v16xx";
    }

    /// <summary>
    /// Sürüm tanımındaki CDN klasörünü döner (ör: "V16" → "v16").
    /// Bulunamazsa "v16" fallback döner.
    /// </summary>
    public string GetCdnFolder(string majorVersion)
    {
        MikroVersionDefinition? def = GetVersion(majorVersion);
        return !string.IsNullOrEmpty(def?.CdnFolder) ? def.CdnFolder : "v16";
    }

    /// <summary>
    /// CDN URL'sini oluşturur. Versiyon tanımında pattern varsa onu kullanır.
    /// </summary>
    public string BuildCdnUrl(string cdnBaseUrl, string majorVersion, string cdnCode, string setupFileName)
    {
        MikroVersionDefinition? def = GetVersion(majorVersion);
        string cdnFolder = !string.IsNullOrEmpty(def?.CdnFolder) ? def.CdnFolder : "v16";
        string pattern = !string.IsNullOrEmpty(def?.CdnUrlPattern) ? def.CdnUrlPattern : DefaultCdnUrlPattern;

        return pattern
            .Replace("{cdnBase}", cdnBaseUrl.TrimEnd('/'))
            .Replace("{cdnFolder}", cdnFolder)
            .Replace("{cdnCode}", cdnCode)
            .Replace("{setupFile}", setupFileName);
    }

    /// <summary>
    /// Belirtilen ürün, sürüm ve modül için setup dosya adını döner.
    /// </summary>
    public string? GetSetupFileName(string productName, string majorVersion, string moduleName)
    {
        MikroVersionDefinition? verDef = GetVersion(majorVersion);
        MikroProductDefinition? prodDef = verDef?.FindProduct(productName);

        string versionTag = !string.IsNullOrEmpty(verDef?.VersionTag) ? verDef.VersionTag : "v16xx";
        string prefix = !string.IsNullOrEmpty(prodDef?.Prefix) ? prodDef.Prefix : "Jump";

        return moduleName.ToUpperInvariant() switch
        {
            "CLIENT" => $"{prefix}_{versionTag}_Client_Setupx064.exe",
            "E-DEFTER" => $"{prefix}_{versionTag}_eDefter_Setupx064.exe",
            "BEYANNAME" => $"{versionTag}_BEYANNAME_Setupx064.exe",
            _ => null
        };
    }

    /// <summary>
    /// Belirtilen ürün ve modül için versiyon kontrolü yapılacak EXE dosya adını döner.
    /// </summary>
    public string? GetExeFileName(string productName, string majorVersion, string moduleName)
    {
        MikroVersionDefinition? verDef = GetVersion(majorVersion);
        MikroProductDefinition? prodDef = verDef?.FindProduct(productName);

        return moduleName.ToUpperInvariant() switch
        {
            "CLIENT" => !string.IsNullOrEmpty(prodDef?.ClientExe) ? prodDef.ClientExe : "MikroJump.EXE",
            "E-DEFTER" => !string.IsNullOrEmpty(prodDef?.EDefterExe) ? prodDef.EDefterExe : "myEDefterStandart.exe",
            "BEYANNAME" => !string.IsNullOrEmpty(verDef?.BeyannameExe) ? verDef.BeyannameExe : "BEYANNAME.EXE",
            _ => null
        };
    }

    /// <summary>
    /// Belirtilen ürün ve sürüm için varsayılan modül listesi oluşturur.
    /// </summary>
    public List<UpdateModule> GetDefaultModules(string productName, string majorVersion)
    {
        MikroVersionDefinition? verDef = GetVersion(majorVersion);
        MikroProductDefinition? prodDef = verDef?.FindProduct(productName);

        string versionTag = !string.IsNullOrEmpty(verDef?.VersionTag) ? verDef.VersionTag : "v16xx";
        string prefix = !string.IsNullOrEmpty(prodDef?.Prefix) ? prodDef.Prefix : "Jump";
        string productComponent = prefix.Equals("Fly", StringComparison.OrdinalIgnoreCase) ? "mikrofly" : "mikrojump";
        string clientExe = !string.IsNullOrEmpty(prodDef?.ClientExe) ? prodDef.ClientExe : "MikroJump.EXE";
        string eDefterExe = !string.IsNullOrEmpty(prodDef?.EDefterExe) ? prodDef.EDefterExe : "myEDefterStandart.exe";
        string beyannameExe = !string.IsNullOrEmpty(verDef?.BeyannameExe) ? verDef.BeyannameExe : "BEYANNAME.EXE";

        return
        [
            new UpdateModule
            {
                Name = "Client",
                SetupFileName = $"{prefix}_{versionTag}_Client_Setupx064.exe",
                ExeFileName = clientExe,
                Enabled = true,
                SetupArgs = $"/LANG=tr /TYPE=custom /COMPONENTS=\"main,main\\efatura,main\\tuik,main\\kep,{productComponent}\" /TASKS=\"desktopicon\""
            },
            new UpdateModule
            {
                Name = "e-Defter",
                SetupFileName = $"{prefix}_{versionTag}_eDefter_Setupx064.exe",
                ExeFileName = eDefterExe,
                Enabled = true
            },
            new UpdateModule
            {
                Name = "Beyanname",
                SetupFileName = $"{versionTag}_BEYANNAME_Setupx064.exe",
                ExeFileName = beyannameExe,
                Enabled = true
            }
        ];
    }

    /// <summary>
    /// Harici JSON dosyasının tam yolunu döner.
    /// </summary>
    public static string GetExternalFilePath() => ExternalFilePath;

    private static MikroVersionCatalog LoadCatalog()
    {
        // 1. Harici dosya varsa onu kullan
        if (File.Exists(ExternalFilePath))
        {
            try
            {
                string json = File.ReadAllText(ExternalFilePath);
                MikroVersionCatalog? catalog = JsonSerializer.Deserialize<MikroVersionCatalog>(json, JsonOptions);

                if (catalog?.Versions.Count > 0)
                {
                    return catalog;
                }
            }
            catch (JsonException)
            {
                // Harici dosya bozuksa embedded'a düş
            }
            catch (IOException)
            {
                // Dosya erişim hatası varsa embedded'a düş
            }
        }

        // 2. Gömülü kaynak
        return LoadEmbeddedCatalog();
    }

    private static MikroVersionCatalog LoadEmbeddedCatalog()
    {
        Assembly assembly = typeof(MikroVersionProvider).Assembly;

        using Stream? stream = assembly.GetManifestResourceStream(EmbeddedResourceName);

        if (stream is null)
        {
            return CreateHardcodedFallback();
        }

        try
        {
            MikroVersionCatalog? catalog = JsonSerializer.Deserialize<MikroVersionCatalog>(stream, JsonOptions);
            return catalog?.Versions.Count > 0 ? catalog : CreateHardcodedFallback();
        }
        catch (JsonException)
        {
            return CreateHardcodedFallback();
        }
    }

    /// <summary>
    /// Hem harici hem de embedded kaynak başarısız olursa son çare hardcoded fallback.
    /// </summary>
    private static MikroVersionCatalog CreateHardcodedFallback()
    {
        return new MikroVersionCatalog
        {
            Versions =
            [
                new MikroVersionDefinition
                {
                    Name = "V16",
                    VersionTag = "v16xx",
                    CdnFolder = "v16",
                    DefaultServerShare = @"\\SERVER\MikroV16xx",
                    DefaultLocalPath = @"C:\Mikro\v16xx",
                    Products =
                    [
                        new MikroProductDefinition { Name = "Jump", Prefix = "Jump", ClientExe = "MikroJump.EXE", EDefterExe = "myEDefterStandart.exe" },
                        new MikroProductDefinition { Name = "Fly", Prefix = "Fly", ClientExe = "MikroFly.EXE", EDefterExe = "MyeDefter.exe" }
                    ]
                },
                new MikroVersionDefinition
                {
                    Name = "V17",
                    VersionTag = "v17xx",
                    CdnFolder = "v17",
                    DefaultServerShare = @"\\SERVER\MikroV17xx",
                    DefaultLocalPath = @"C:\Mikro\v17xx",
                    Products =
                    [
                        new MikroProductDefinition { Name = "Jump", Prefix = "Jump", ClientExe = "MikroJump.EXE", EDefterExe = "myEDefterStandart.exe" },
                        new MikroProductDefinition { Name = "Fly", Prefix = "Fly", ClientExe = "MikroFly.EXE", EDefterExe = "MyeDefter.exe" }
                    ]
                }
            ]
        };
    }
}

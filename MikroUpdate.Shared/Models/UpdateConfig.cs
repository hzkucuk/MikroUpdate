using System.Text.Json.Serialization;

namespace MikroUpdate.Shared.Models;

/// <summary>
/// Mikro ERP güncelleme yapılandırma ayarları.
/// V16/V17 ana sürüm ve Jump/Fly ürün kombinasyonuna göre çoklu modül desteği sağlar.
/// </summary>
public sealed class UpdateConfig
{
    /// <summary>Ana sürüm: V16 veya V17.</summary>
    public string MajorVersion { get; set; } = "V16";

    /// <summary>Ürün adı: Jump veya Fly.</summary>
    public string ProductName { get; set; } = "Jump";

    /// <summary>Sunucu paylaşım yolu (ör: \\SERVER\MikroV16xx).</summary>
    public string ServerSharePath { get; set; } = @"\\SERVER\MikroV16xx";

    /// <summary>Terminal kurulum yolu (ör: C:\Mikro\v16xx).</summary>
    public string LocalInstallPath { get; set; } = @"C:\Mikro\v16xx";

    /// <summary>Setup dosyalarının bulunduğu klasör yolu (ör: \\SERVER\MikroV16xx\CLIENT).</summary>
    public string SetupFilesPath { get; set; } = @"\\SERVER\MikroV16xx\CLIENT";

    /// <summary>Güncelleme sonrası Mikro'yu otomatik başlat.</summary>
    public bool AutoLaunchAfterUpdate { get; set; } = true;

    /// <summary>MikroUpdate yeni sürümlerini otomatik indir ve kur. Varsayılan: true.</summary>
    public bool AutoSelfUpdate { get; set; } = true;

    /// <summary>Periyodik versiyon kontrol aralığı (dakika). Varsayılan: 30.</summary>
    public int CheckIntervalMinutes { get; set; } = 30;

    /// <summary>Güncelleme modu: Local (varsayılan), Online veya Hybrid.</summary>
    public UpdateMode UpdateMode { get; set; } = UpdateMode.Local;

    /// <summary>CDN temel URL'si. Online/Hybrid modlarında kullanılır.</summary>
    public string CdnBaseUrl { get; set; } = "https://cdn-mikro.atros.com.tr/mikro";

    /// <summary>HTTP proxy adresi (ör: "http://proxy:8080"). Boş ise proxy kullanılmaz.</summary>
    public string ProxyAddress { get; set; } = string.Empty;

    /// <summary>HTTP istek zaman aşımı (saniye). 0 ise varsayılan değerler kullanılır.</summary>
    public int HttpTimeoutSeconds { get; set; }

    /// <summary>Güncelleme modülleri (Client, e-Defter, Beyanname).</summary>
    public List<UpdateModule> Modules { get; set; } = [];

    /// <summary>Aktif modül listesi (Enabled = true olanlar).</summary>
    [JsonIgnore]
    public IReadOnlyList<UpdateModule> EnabledModules =>
        Modules.Where(m => m.Enabled).ToList();

    /// <summary>Ana ürün EXE dosyası adı (Client modülünden alınır).</summary>
    [JsonIgnore]
    public string ExeFileName =>
        Modules.FirstOrDefault(m => m.Name.Equals("Client", StringComparison.OrdinalIgnoreCase))?.ExeFileName
        ?? (ProductName.Equals("Fly", StringComparison.OrdinalIgnoreCase) ? "MikroFly.EXE" : "MikroJump.EXE");

    /// <summary>Terminal'deki ana EXE tam yolu.</summary>
    [JsonIgnore]
    public string LocalExePath => Path.Combine(LocalInstallPath, ExeFileName);

    /// <summary>Sunucudaki ana EXE tam yolu (versiyon referansı için).</summary>
    [JsonIgnore]
    public string ServerExePath => Path.Combine(ServerSharePath, ExeFileName);

    /// <summary>
    /// Modül listesi boşsa varsayılan modüllerle doldurur.
    /// ConfigService.Load() sonrası çağrılmalıdır.
    /// </summary>
    public void EnsureModules()
    {
        if (Modules.Count == 0)
        {
            Modules = GetDefaultModules(ProductName, MajorVersion);
        }
    }

    /// <summary>
    /// Ürün ve ana sürüme göre varsayılan modül listesi oluşturur.
    /// </summary>
    public static List<UpdateModule> GetDefaultModules(string productName, string majorVersion)
    {
        ArgumentNullException.ThrowIfNull(productName);
        ArgumentNullException.ThrowIfNull(majorVersion);

        bool isFly = productName.Equals("Fly", StringComparison.OrdinalIgnoreCase);
        string ver = majorVersion.Equals("V17", StringComparison.OrdinalIgnoreCase) ? "v17xx" : "v16xx";
        string productPrefix = isFly ? "Fly" : "Jump";

        return
        [
            new UpdateModule
            {
                Name = "Client",
                SetupFileName = $"{productPrefix}_{ver}_Client_Setupx064.exe",
                ExeFileName = isFly ? "MikroFly.EXE" : "MikroJump.EXE",
                Enabled = true
            },
            new UpdateModule
            {
                Name = "e-Defter",
                SetupFileName = $"{productPrefix}_{ver}_eDefter_Setupx064.exe",
                ExeFileName = isFly ? "MyeDefter.exe" : "myEDefterStandart.exe",
                Enabled = true
            },
            new UpdateModule
            {
                Name = "Beyanname",
                SetupFileName = $"{ver}_BEYANNAME_Setupx064.exe",
                ExeFileName = "BEYANNAME.EXE",
                Enabled = true
            }
        ];
    }
}

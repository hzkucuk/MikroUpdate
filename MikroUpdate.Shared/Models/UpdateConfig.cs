using System.Text.Json.Serialization;

using MikroUpdate.Shared.Helpers;

namespace MikroUpdate.Shared.Models;

/// <summary>
/// Mikro ERP güncelleme yapılandırma ayarları.
/// Sürüm/ürün bilgileri <see cref="MikroVersionProvider"/> üzerinden JSON'dan okunur.
/// </summary>
public sealed class UpdateConfig
{
    private static readonly MikroVersionProvider VersionProvider = new();

    /// <summary>Ana sürüm (ör: V16, V17). Yeni sürümler mikro-versions.json ile eklenir.</summary>
    public string MajorVersion { get; set; } = "V16";

    /// <summary>Ürün adı: Jump veya Fly.</summary>
    public string ProductName { get; set; } = "Jump";

    /// <summary>Sunucu paylaşım yolu (ör: \\SERVER\MikroV16xx).</summary>
    public string ServerSharePath { get; set; } = @"\\SERVER\MikroV16xx";

    /// <summary>
    /// Yerel ağ (UNC) erişim modu.
    /// Direct: Servis hesabının mevcut kimliğiyle erişir (domain/gMSA).
    /// Credential: Kaydedilen kullanıcı bilgisiyle erişir (workgroup senaryosu).
    /// </summary>
    public NetworkAccessMode NetworkAccessMode { get; set; } = NetworkAccessMode.Direct;

    /// <summary>
    /// Credential modunda UNC erişimi için kullanıcı adı (ör: SERVER\mikroupdate_ro).
    /// </summary>
    public string ServerUsername { get; set; } = string.Empty;

    /// <summary>
    /// Credential modunda UNC erişimi için DPAPI ile korunmuş parola (Base64).
    /// </summary>
    public string EncryptedServerPassword { get; set; } = string.Empty;

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

    /// <summary>Ana ürün EXE dosyası adı (Client modülünden alınır, fallback: JSON tanımından).</summary>
    [JsonIgnore]
    public string ExeFileName =>
        Modules.FirstOrDefault(m => m.Name.Equals("Client", StringComparison.OrdinalIgnoreCase))?.ExeFileName
        ?? VersionProvider.GetExeFileName(ProductName, MajorVersion, "Client")
        ?? "MikroJump.EXE";

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
    /// Sürüm/ürün bilgileri JSON tanımından okunur.
    /// </summary>
    public static List<UpdateModule> GetDefaultModules(string productName, string majorVersion)
    {
        ArgumentNullException.ThrowIfNull(productName);
        ArgumentNullException.ThrowIfNull(majorVersion);

        return VersionProvider.GetDefaultModules(productName, majorVersion);
    }
}

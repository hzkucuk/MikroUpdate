using System.Text.Json.Serialization;

namespace MikroUpdate.Win.Models;

/// <summary>
/// Mikro ERP güncelleme yapılandırma ayarları.
/// </summary>
public sealed class UpdateConfig
{
    /// <summary>Ürün adı: Jump veya Fly.</summary>
    public string ProductName { get; set; } = "Jump";

    /// <summary>Sunucu paylaşım yolu (ör: \\SERVER\MikroV16xx).</summary>
    public string ServerSharePath { get; set; } = @"\\SERVER\MikroV16xx";

    /// <summary>Terminal kurulum yolu (ör: C:\Mikro\v16xx).</summary>
    public string LocalInstallPath { get; set; } = @"C:\Mikro\v16xx";

    /// <summary>Client setup dosyası adı (ör: Jump_v16xx_Client_Setupx064.exe).</summary>
    public string SetupFileName { get; set; } = "Jump_v16xx_Client_Setupx064.exe";

    /// <summary>Güncelleme sonrası Mikro'yu otomatik başlat.</summary>
    public bool AutoLaunchAfterUpdate { get; set; } = true;

    /// <summary>Mikro ana EXE dosyası adı (ürüne göre otomatik belirlenir).</summary>
    [JsonIgnore]
    public string ExeFileName => ProductName.Equals("Fly", StringComparison.OrdinalIgnoreCase)
        ? "MikroFly.EXE"
        : "MikroJump.EXE";

    /// <summary>Terminal'deki EXE tam yolu.</summary>
    [JsonIgnore]
    public string LocalExePath => Path.Combine(LocalInstallPath, ExeFileName);

    /// <summary>Sunucudaki EXE tam yolu (versiyon referansı için).</summary>
    [JsonIgnore]
    public string ServerExePath => Path.Combine(ServerSharePath, ExeFileName);

    /// <summary>Sunucudaki CLIENT klasörü yolu.</summary>
    [JsonIgnore]
    public string ServerClientPath => Path.Combine(ServerSharePath, "CLIENT");

    /// <summary>Sunucudaki setup dosyasının tam yolu.</summary>
    [JsonIgnore]
    public string ServerSetupFilePath => Path.Combine(ServerClientPath, SetupFileName);
}

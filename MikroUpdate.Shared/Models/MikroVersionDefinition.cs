namespace MikroUpdate.Shared.Models;

/// <summary>
/// Mikro ERP ana sürüm tanımı (V16, V17 vb.).
/// JSON'dan deserialize edilir; derleme gerektirmeden yeni sürümler eklenebilir.
/// </summary>
public sealed class MikroVersionDefinition
{
    /// <summary>Sürüm adı (ör: "V16", "V17").</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Versiyon tag'i, setup dosya adlarında kullanılır (ör: "v16xx", "v17xx").</summary>
    public string VersionTag { get; set; } = string.Empty;

    /// <summary>CDN klasör adı (ör: "v16", "v17").</summary>
    public string CdnFolder { get; set; } = string.Empty;

    /// <summary>
    /// CDN URL pattern'i. Placeholder'lar: {cdnBase}, {cdnFolder}, {cdnCode}, {setupFile}.
    /// Boş ise varsayılan pattern kullanılır: "{cdnBase}/{cdnFolder}/{cdnCode}/{setupFile}".
    /// </summary>
    public string CdnUrlPattern { get; set; } = string.Empty;

    /// <summary>
    /// Mikro sürüm güncellemeleri sayfası URL'si.
    /// En son CDN kodunu web scraping ile tespit etmek için kullanılır.
    /// Boş ise HEAD probe'a fallback yapılır.
    /// </summary>
    public string ReleaseNotesUrl { get; set; } = string.Empty;

    /// <summary>Varsayılan sunucu paylaşım yolu (ör: "\\\\SERVER\\MikroV16xx").</summary>
    public string DefaultServerShare { get; set; } = string.Empty;

    /// <summary>Varsayılan terminal kurulum yolu (ör: "C:\\Mikro\\v16xx").</summary>
    public string DefaultLocalPath { get; set; } = string.Empty;

    /// <summary>Beyanname EXE dosya adı (ör: "BEYANNAME.EXE"). Tüm ürünlerde ortaktır.</summary>
    public string BeyannameExe { get; set; } = "BEYANNAME.EXE";

    /// <summary>
    /// Setup dosya adı pattern'leri. Placeholder'lar: {prefix}, {versionTag}.
    /// JSON'dan derleme gerektirmeden özelleştirilebilir.
    /// Boş ise varsayılan pattern'ler kullanılır.
    /// </summary>
    public MikroSetupPatterns SetupPatterns { get; set; } = new();

    /// <summary>Bu sürüm için desteklenen ürün tanımları.</summary>
    public List<MikroProductDefinition> Products { get; set; } = [];

    /// <summary>
    /// Belirtilen ürün adıyla eşleşen ürün tanımını döner.
    /// </summary>
    public MikroProductDefinition? FindProduct(string productName)
    {
        return Products.FirstOrDefault(p =>
            p.Name.Equals(productName, StringComparison.OrdinalIgnoreCase));
    }
}

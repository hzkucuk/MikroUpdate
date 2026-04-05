namespace MikroUpdate.Shared.Helpers;

/// <summary>
/// Mikro ERP ürün/sürüm kombinasyonuna göre setup ve exe dosya adı matrisi.
/// <para>
/// Beyanname her zaman ortaktır (ürün prefix'i yoktur).
/// Client ve eDefter ürün prefix'i alır (Jump/Fly).
/// Sürüm bilgileri <see cref="MikroVersionProvider"/> üzerinden JSON'dan okunur.
/// </para>
/// </summary>
public static class MikroProductMatrix
{
    private static readonly MikroVersionProvider Provider = new();

    /// <summary>
    /// Belirtilen ürün, sürüm ve modül için CDN setup dosya adını döner.
    /// </summary>
    /// <param name="productName">Ürün adı (ör: "Jump", "Fly").</param>
    /// <param name="majorVersion">Ana sürüm (ör: "V16", "V17").</param>
    /// <param name="moduleName">Modül adı: "Client", "e-Defter" veya "Beyanname".</param>
    /// <returns>Setup dosya adı veya bilinmeyen modülde null.</returns>
    public static string? GetSetupFileName(string productName, string majorVersion, string moduleName)
    {
        ArgumentNullException.ThrowIfNull(productName);
        ArgumentNullException.ThrowIfNull(majorVersion);
        ArgumentNullException.ThrowIfNull(moduleName);

        return Provider.GetSetupFileName(productName, majorVersion, moduleName);
    }

    /// <summary>
    /// Belirtilen ürün ve modül için versiyon kontrolü yapılacak EXE dosya adını döner.
    /// </summary>
    /// <param name="productName">Ürün adı (ör: "Jump", "Fly").</param>
    /// <param name="majorVersion">Ana sürüm (ör: "V16", "V17").</param>
    /// <param name="moduleName">Modül adı: "Client", "e-Defter" veya "Beyanname".</param>
    /// <returns>EXE dosya adı veya bilinmeyen modülde null.</returns>
    public static string? GetExeFileName(string productName, string majorVersion, string moduleName)
    {
        ArgumentNullException.ThrowIfNull(productName);
        ArgumentNullException.ThrowIfNull(majorVersion);
        ArgumentNullException.ThrowIfNull(moduleName);

        return Provider.GetExeFileName(productName, majorVersion, moduleName);
    }

    /// <summary>
    /// Ana sürüme göre versiyon tag'ini döner (ör: "V16" → "v16xx").
    /// </summary>
    public static string GetVersionTag(string majorVersion) =>
        Provider.GetVersionTag(majorVersion);

    /// <summary>
    /// Ürün adına göre prefix döner (ör: "Fly" → "Fly", diğer → "Jump").
    /// </summary>
    public static string GetProductPrefix(string productName) =>
        productName.Equals("Fly", StringComparison.OrdinalIgnoreCase) ? "Fly" : "Jump";
}

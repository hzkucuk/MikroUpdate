namespace MikroUpdate.Shared.Helpers;

/// <summary>
/// Mikro ERP ürün/sürüm kombinasyonuna göre setup ve exe dosya adı matrisi.
/// <para>
/// Beyanname her zaman ortaktır (ürün prefix'i yoktur).
/// Client ve eDefter ürün prefix'i alır (Jump/Fly).
/// </para>
/// </summary>
public static class MikroProductMatrix
{
    /// <summary>
    /// Belirtilen ürün, sürüm ve modül için CDN setup dosya adını döner.
    /// </summary>
    /// <param name="productName">Ürün adı: "Jump" veya "Fly".</param>
    /// <param name="majorVersion">Ana sürüm: "V16" veya "V17".</param>
    /// <param name="moduleName">Modül adı: "Client", "e-Defter" veya "Beyanname".</param>
    /// <returns>Setup dosya adı veya bilinmeyen modülde null.</returns>
    public static string? GetSetupFileName(string productName, string majorVersion, string moduleName)
    {
        ArgumentNullException.ThrowIfNull(productName);
        ArgumentNullException.ThrowIfNull(majorVersion);
        ArgumentNullException.ThrowIfNull(moduleName);

        string ver = GetVersionTag(majorVersion);
        string prefix = GetProductPrefix(productName);

        return moduleName.ToUpperInvariant() switch
        {
            "CLIENT" => $"{prefix}_{ver}_Client_Setupx064.exe",
            "E-DEFTER" => $"{prefix}_{ver}_eDefter_Setupx064.exe",
            "BEYANNAME" => $"{ver}_BEYANNAME_Setupx064.exe",
            _ => null
        };
    }

    /// <summary>
    /// Belirtilen ürün ve modül için versiyon kontrolü yapılacak EXE dosya adını döner.
    /// </summary>
    /// <param name="productName">Ürün adı: "Jump" veya "Fly".</param>
    /// <param name="moduleName">Modül adı: "Client", "e-Defter" veya "Beyanname".</param>
    /// <returns>EXE dosya adı veya bilinmeyen modülde null.</returns>
    public static string? GetExeFileName(string productName, string moduleName)
    {
        ArgumentNullException.ThrowIfNull(productName);
        ArgumentNullException.ThrowIfNull(moduleName);

        bool isFly = productName.Equals("Fly", StringComparison.OrdinalIgnoreCase);

        return moduleName.ToUpperInvariant() switch
        {
            "CLIENT" => isFly ? "MikroFly.EXE" : "MikroJump.EXE",
            "E-DEFTER" => isFly ? "MyeDefter.exe" : "myEDefterStandart.exe",
            "BEYANNAME" => "BEYANNAME.EXE",
            _ => null
        };
    }

    /// <summary>
    /// Ana sürüme göre versiyon tag'ini döner (ör: "v16xx", "v17xx").
    /// </summary>
    private static string GetVersionTag(string majorVersion) =>
        majorVersion.Equals("V17", StringComparison.OrdinalIgnoreCase) ? "v17xx" : "v16xx";

    /// <summary>
    /// Ürün adına göre prefix döner (ör: "Jump", "Fly").
    /// </summary>
    private static string GetProductPrefix(string productName) =>
        productName.Equals("Fly", StringComparison.OrdinalIgnoreCase) ? "Fly" : "Jump";
}

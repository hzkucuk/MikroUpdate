namespace MikroUpdate.Shared.Models;

/// <summary>
/// Setup dosya adı pattern'leri. Placeholder'lar: {prefix}, {versionTag}.
/// JSON'dan derleme gerektirmeden özelleştirilebilir.
/// <para>
/// Örnek: "{prefix}_{versionTag}_Client_Setupx064.exe" → "Jump_v16xx_Client_Setupx064.exe"
/// </para>
/// </summary>
public sealed class MikroSetupPatterns
{
    /// <summary>Varsayılan Client setup pattern'i.</summary>
    public const string DefaultClient = "{prefix}_{versionTag}_Client_Setupx064.exe";

    /// <summary>Varsayılan e-Defter setup pattern'i.</summary>
    public const string DefaultEDefter = "{prefix}_{versionTag}_eDefter_Setupx064.exe";

    /// <summary>Varsayılan Beyanname setup pattern'i.</summary>
    public const string DefaultBeyanname = "{versionTag}_BEYANNAME_Setupx064.exe";

    /// <summary>Client setup dosya adı pattern'i (ör: "{prefix}_{versionTag}_Client_Setupx064.exe").</summary>
    public string Client { get; set; } = DefaultClient;

    /// <summary>e-Defter setup dosya adı pattern'i (ör: "{prefix}_{versionTag}_eDefter_Setupx064.exe").</summary>
    public string EDefter { get; set; } = DefaultEDefter;

    /// <summary>Beyanname setup dosya adı pattern'i (ör: "{versionTag}_BEYANNAME_Setupx064.exe").</summary>
    public string Beyanname { get; set; } = DefaultBeyanname;

    /// <summary>
    /// Pattern'deki placeholder'ları gerçek değerlerle değiştirir.
    /// </summary>
    public static string Resolve(string pattern, string prefix, string versionTag)
    {
        return pattern
            .Replace("{prefix}", prefix)
            .Replace("{versionTag}", versionTag);
    }
}

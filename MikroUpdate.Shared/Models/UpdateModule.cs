namespace MikroUpdate.Shared.Models;

/// <summary>
/// Mikro ERP güncelleme modülü tanımı.
/// Her modül bir setup dosyası ve versiyon kontrolü yapılacak EXE içerir.
/// </summary>
public sealed class UpdateModule
{
    /// <summary>Modül adı (ör: Client, e-Defter, Beyanname).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Setup dosyası adı (ör: Jump_v16xx_Client_Setupx064.exe).</summary>
    public string SetupFileName { get; set; } = string.Empty;

    /// <summary>Versiyon kontrolü yapılacak EXE dosyası adı (ör: MikroJump.EXE).</summary>
    public string ExeFileName { get; set; } = string.Empty;

    /// <summary>Modül güncelleme için aktif mi.</summary>
    public bool Enabled { get; set; } = true;
}

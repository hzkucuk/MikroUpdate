namespace MikroUpdate.Shared.Models;

/// <summary>
/// Mikro ERP ürün tanımı (Jump, Fly vb.).
/// JSON'dan deserialize edilir; derleme gerektirmeden yeni ürünler eklenebilir.
/// </summary>
public sealed class MikroProductDefinition
{
    /// <summary>Ürün adı (ör: "Jump", "Fly").</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Setup dosya adı prefix'i (ör: "Jump", "Fly").</summary>
    public string Prefix { get; set; } = string.Empty;

    /// <summary>Client modülü EXE dosya adı (ör: "MikroJump.EXE").</summary>
    public string ClientExe { get; set; } = string.Empty;

    /// <summary>e-Defter modülü EXE dosya adı (ör: "myEDefterStandart.exe").</summary>
    public string EDefterExe { get; set; } = string.Empty;
}

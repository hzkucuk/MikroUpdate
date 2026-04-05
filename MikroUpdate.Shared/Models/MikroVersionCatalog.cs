namespace MikroUpdate.Shared.Models;

/// <summary>
/// Mikro ERP sürüm kataloğu. mikro-versions.json dosyasının kök modeli.
/// Derleme gerektirmeden yeni sürüm/ürün eklemeyi sağlar.
/// </summary>
public sealed class MikroVersionCatalog
{
    /// <summary>Tüm desteklenen sürüm tanımları.</summary>
    public List<MikroVersionDefinition> Versions { get; set; } = [];

    /// <summary>
    /// Belirtilen sürüm adıyla eşleşen tanımı döner (ör: "V16", "V17").
    /// </summary>
    public MikroVersionDefinition? FindVersion(string versionName)
    {
        return Versions.FirstOrDefault(v =>
            v.Name.Equals(versionName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Tüm sürüm adlarını döner (ComboBox doldurma vb. için).
    /// </summary>
    public IReadOnlyList<string> GetVersionNames() =>
        Versions.Select(v => v.Name).ToList();

    /// <summary>
    /// Belirtilen sürümdeki tüm ürün adlarını döner.
    /// </summary>
    public IReadOnlyList<string> GetProductNames(string versionName)
    {
        MikroVersionDefinition? version = FindVersion(versionName);
        return version?.Products.Select(p => p.Name).ToList() ?? [];
    }
}

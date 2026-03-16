using System.Text.Json;

using MikroUpdate.Shared.Models;

namespace MikroUpdate.Service.Services;

/// <summary>
/// Yapılandırma dosyasını okuma/yazma servisi.
/// Ayarlar ProgramData\MikroUpdate\config.json dosyasında saklanır.
/// </summary>
public sealed class ConfigService
{
    private static readonly string ConfigDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "MikroUpdate");

    private static readonly string ConfigFilePath = Path.Combine(ConfigDirectory, "config.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Yapılandırma dosyasını yükler. Dosya yoksa varsayılan ayarlarla döner.
    /// </summary>
    public UpdateConfig Load()
    {
        if (!File.Exists(ConfigFilePath))
        {
            return new UpdateConfig();
        }

        string json = File.ReadAllText(ConfigFilePath);
        return JsonSerializer.Deserialize<UpdateConfig>(json, JsonOptions) ?? new UpdateConfig();
    }
}

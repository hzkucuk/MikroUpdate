using System.Text.Json;

using MikroUpdate.Shared.Models;

namespace MikroUpdate.Win.Services;

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
    /// JSON hatası varsa backslash escape sorununu tamir edip yeniden dener.
    /// </summary>
    public UpdateConfig Load()
    {
        if (!File.Exists(ConfigFilePath))
        {
            return new UpdateConfig();
        }

        string json = File.ReadAllText(ConfigFilePath);

        try
        {
            return JsonSerializer.Deserialize<UpdateConfig>(json, JsonOptions) ?? new UpdateConfig();
        }
        catch (JsonException)
        {
            // ISS installer yol değerlerindeki backslash'ları JSON-escape etmeden yazmış olabilir.
            // Tüm \ karakterlerini \\ ile değiştirerek tamir et.
            string repaired = json.Replace(@"\", @"\\");

            try
            {
                UpdateConfig config = JsonSerializer.Deserialize<UpdateConfig>(repaired, JsonOptions)
                    ?? new UpdateConfig();

                // Tamir edilen config'i kalıcı olarak düzgün JSON formatında kaydet
                Save(config);

                return config;
            }
            catch (JsonException)
            {
                return new UpdateConfig();
            }
        }
    }

    /// <summary>
    /// Yapılandırma dosyasını kaydeder.
    /// </summary>
    public void Save(UpdateConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        Directory.CreateDirectory(ConfigDirectory);
        string json = JsonSerializer.Serialize(config, JsonOptions);
        File.WriteAllText(ConfigFilePath, json);
    }

    /// <summary>
    /// Yapılandırma dosyasının tam yolunu döner.
    /// </summary>
    public static string GetConfigFilePath() => ConfigFilePath;
}

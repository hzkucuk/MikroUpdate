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
            // Bu yalnızca deserialize başarısız olduğunda çalışır, zaten geçerli JSON'ları etkilemez.
            string repaired = json.Replace(@"\", @"\\");

            try
            {
                UpdateConfig config = JsonSerializer.Deserialize<UpdateConfig>(repaired, JsonOptions)
                    ?? new UpdateConfig();

                // Tamir edilen config'i kalıcı olarak düzgün JSON formatında kaydet
                SaveRepaired(config);

                return config;
            }
            catch (JsonException)
            {
                // Tamir de başarısız — varsayılan config ile devam et
                return new UpdateConfig();
            }
        }
    }

    /// <summary>
    /// Tamir edilen config'i System.Text.Json ile düzgün formatta geri yazar.
    /// Böylece bir sonraki okumada sorun tekrarlamaz.
    /// </summary>
    private static void SaveRepaired(UpdateConfig config)
    {
        try
        {
            Directory.CreateDirectory(ConfigDirectory);
            string json = JsonSerializer.Serialize(config, JsonOptions);
            File.WriteAllText(ConfigFilePath, json);
        }
        catch
        {
            // Yazma hatası — sessizce geç, en azından config bellekte doğru
        }
    }
}

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
    private static readonly string BackupFilePath = Path.Combine(ConfigDirectory, "config.backup.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Yapılandırma dosyasını yükler. Dosya yoksa veya bozuksa backup'tan geri yükler.
    /// Hiçbiri yoksa varsayılan ayarlarla döner.
    /// </summary>
    public UpdateConfig Load()
    {
        UpdateConfig? config = TryLoadFromFile(ConfigFilePath);

        if (config is not null)
        {
            return config;
        }

        // Ana dosya okunamadı — backup'tan dene
        config = TryLoadFromFile(BackupFilePath);

        if (config is not null)
        {
            // Backup'tan başarıyla yüklendi — ana dosyayı geri yaz
            WriteFile(ConfigFilePath, config);

            return config;
        }

        return new UpdateConfig();
    }

    /// <summary>
    /// Yapılandırma dosyasını kaydeder. Kaydetmeden önce mevcut dosyayı yedekler.
    /// </summary>
    public void Save(UpdateConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        Directory.CreateDirectory(ConfigDirectory);

        // Mevcut config'i yedekle
        if (File.Exists(ConfigFilePath))
        {
            try
            {
                File.Copy(ConfigFilePath, BackupFilePath, overwrite: true);
            }
            catch
            {
                // Yedekleme başarısız — kaydetmeye devam et
            }
        }

        WriteFile(ConfigFilePath, config);
    }

    /// <summary>
    /// Yapılandırma dosyasının tam yolunu döner.
    /// </summary>
    public static string GetConfigFilePath() => ConfigFilePath;

    /// <summary>
    /// Belirtilen dosyadan config okumayı dener. Başarısızsa null döner.
    /// </summary>
    private static UpdateConfig? TryLoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return null;
        }

        string json = File.ReadAllText(filePath);

        try
        {
            return JsonSerializer.Deserialize<UpdateConfig>(json, JsonOptions);
        }
        catch (JsonException)
        {
            // Backslash escape tamir denemesi
            string repaired = json.Replace(@"\", @"\\");

            try
            {
                UpdateConfig? config = JsonSerializer.Deserialize<UpdateConfig>(repaired, JsonOptions);

                if (config is not null)
                {
                    WriteFile(filePath, config);
                }

                return config;
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }

    private static void WriteFile(string filePath, UpdateConfig config)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            string json = JsonSerializer.Serialize(config, JsonOptions);
            File.WriteAllText(filePath, json);
        }
        catch
        {
            // Yazma hatası — sessizce geç
        }
    }
}

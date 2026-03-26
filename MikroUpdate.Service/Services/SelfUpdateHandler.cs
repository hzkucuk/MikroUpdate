using MikroUpdate.Service.Services;

namespace MikroUpdate.Service;

/// <summary>
/// Self-update sonrası tray app yeniden başlatma işlemlerini yönetir.
/// Restart flag dosyası üzerinden servis ↔ installer koordinasyonu sağlar.
/// </summary>
internal sealed class SelfUpdateHandler(ILogger logger)
{
    /// <summary>
    /// ProgramData altındaki restart flag dosyasının yolu.
    /// Self-update installer tamamlandıktan sonra tray app'i yeniden başlatmak için kullanılır.
    /// </summary>
    internal static string RestartFlagPath =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "MikroUpdate", "pending_restart.flag");

    /// <summary>
    /// Restart flag dosyasını yazar. Installer tamamlandıktan sonra tray app yolunu saklar.
    /// </summary>
    internal async Task WriteFlagAsync(string trayAppPath, CancellationToken cancellationToken)
    {
        string flagDir = Path.GetDirectoryName(RestartFlagPath)!;

        if (!Directory.Exists(flagDir))
        {
            Directory.CreateDirectory(flagDir);
        }

        await File.WriteAllTextAsync(RestartFlagPath, trayAppPath, cancellationToken)
            .ConfigureAwait(false);

        logger.LogInformation("Restart flag yazıldı: {FlagPath}", RestartFlagPath);
    }

    /// <summary>
    /// Servis başlangıcında bekleyen restart flag'i kontrol eder.
    /// Installer tamamlandıktan sonra servis yeniden başlatıldıysa tray app'i kullanıcı oturumunda başlatır.
    /// Installer hâlâ çalışıyor olabileceği için gecikme ve yeniden deneme mekanizması içerir.
    /// </summary>
    internal async Task CheckPendingAppRestartAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (!File.Exists(RestartFlagPath))
            {
                logger.LogDebug("Bekleyen restart flag yok, devam ediliyor.");

                return;
            }

            string flagContent = File.ReadAllText(RestartFlagPath).Trim();
            logger.LogInformation(
                "Bekleyen restart flag bulundu: {FlagPath}, İçerik: {Content}",
                RestartFlagPath, flagContent);

            // Installer hâlâ çalışıyor olabilir — sistemin oturmasını bekle
            logger.LogInformation("Installer'ın tamamlanması bekleniyor (5 saniye)...");
            await Task.Delay(5000, cancellationToken).ConfigureAwait(false);

            // Retry mekanizması: kullanıcı oturumu token'ı henüz hazır olmayabilir
            const int maxRetries = 3;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                logger.LogInformation(
                    "Tray app başlatma denemesi {Attempt}/{MaxRetries}",
                    attempt, maxRetries);

                bool launched = LaunchTrayAppInUserSession(flagContent);

                if (launched)
                {
                    logger.LogInformation(
                        "Tray app başarıyla başlatıldı (deneme {Attempt}/{MaxRetries}).",
                        attempt, maxRetries);

                    return;
                }

                if (attempt < maxRetries)
                {
                    int delaySeconds = attempt * 5;
                    logger.LogWarning(
                        "Tray app başlatılamadı (deneme {Attempt}/{MaxRetries}), {Delay} saniye sonra tekrar denenecek.",
                        attempt, maxRetries, delaySeconds);

                    await Task.Delay(delaySeconds * 1000, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    logger.LogError(
                        "Tray app {MaxRetries} denemede de başlatılamadı.", maxRetries);
                }
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Tray app restart kontrolü iptal edildi.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Bekleyen restart kontrolü hatası.");
        }
        finally
        {
            TryDeleteRestartFlag();
        }
    }

    /// <summary>
    /// Tray app'i aktif kullanıcı oturumunda başlatır.
    /// </summary>
    /// <param name="flagContent">Flag dosyasından okunan tray app yolu.</param>
    /// <returns>Başlatma başarılı ise true.</returns>
    internal bool LaunchTrayAppInUserSession(string flagContent)
    {
        string trayAppPath = flagContent;

        if (string.IsNullOrWhiteSpace(trayAppPath) || !File.Exists(trayAppPath))
        {
            // Fallback: servis dizininden tahmin et
            string serviceDir = AppContext.BaseDirectory;
            trayAppPath = Path.GetFullPath(Path.Combine(serviceDir, "..", "Win", "MikroUpdate.exe"));
            logger.LogInformation(
                "Flag içeriği geçersiz, fallback yol kullanılıyor: {Path}", trayAppPath);
        }

        if (!File.Exists(trayAppPath))
        {
            logger.LogError("Tray app bulunamadı: {Path}", trayAppPath);

            return false;
        }

        logger.LogInformation("Tray app başlatılıyor: {Path}", trayAppPath);

        return UserSessionLauncher.LaunchInUserSession(trayAppPath, logger);
    }

    /// <summary>
    /// Restart flag dosyasını güvenli şekilde siler.
    /// </summary>
    internal void TryDeleteRestartFlag()
    {
        try
        {
            if (File.Exists(RestartFlagPath))
            {
                File.Delete(RestartFlagPath);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Restart flag dosyası silinemedi: {Path}", RestartFlagPath);
        }
    }
}

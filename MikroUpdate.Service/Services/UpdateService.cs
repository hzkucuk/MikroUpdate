using System.Diagnostics;

namespace MikroUpdate.Service.Services;

/// <summary>
/// Mikro ERP güncelleme kurulum servisi.
/// Sunucu paylaşımından kopyalama ve Inno Setup sessiz kurulum işlemlerini yönetir.
/// Windows Service (LocalSystem) olarak çalıştığında admin yetkisi gerekli işlemleri gerçekleştirir.
/// </summary>
public sealed class UpdateService
{
    private static readonly string TempDirectory = Path.Combine(Path.GetTempPath(), "MikroUpdate");

    /// <summary>
    /// Mikro ERP sürecini sonlandırır.
    /// </summary>
    /// <returns>Sonlandırılan süreç sayısı.</returns>
    public int KillMikroProcess(string exeFileName)
    {
        ArgumentNullException.ThrowIfNull(exeFileName);

        string processName = Path.GetFileNameWithoutExtension(exeFileName);
        Process[] processes = Process.GetProcessesByName(processName);
        int killed = 0;

        foreach (Process process in processes)
        {
            try
            {
                process.Kill(entireProcessTree: true);
                process.WaitForExit(5000);
                killed++;
            }
            catch (InvalidOperationException)
            {
                // Süreç zaten kapanmış olabilir
            }
            finally
            {
                process.Dispose();
            }
        }

        return killed;
    }

    /// <summary>
    /// Setup dosyasını sunucu paylaşımından geçici dizine kopyalar.
    /// </summary>
    /// <returns>Kopyalanan dosyanın geçici yolu veya sunucuda bulunamazsa null.</returns>
    public string? CopySetupFromServer(string serverSetupPath)
    {
        ArgumentNullException.ThrowIfNull(serverSetupPath);

        if (!File.Exists(serverSetupPath))
        {
            return null;
        }

        Directory.CreateDirectory(TempDirectory);
        string tempFile = Path.Combine(TempDirectory, Path.GetFileName(serverSetupPath));

        File.Copy(serverSetupPath, tempFile, overwrite: true);

        return tempFile;
    }

    /// <summary>
    /// Sessiz kurulum çalıştırır (Inno Setup).
    /// /SP- /VERYSILENT /SUPPRESSMSGBOXES /NORESTART parametreleriyle çalışır.
    /// </summary>
    /// <returns>Kurulum sürecinin çıkış kodu (0 = başarılı).</returns>
    public async Task<int> RunSilentInstallAsync(
        string setupFilePath,
        string installDirectory,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(setupFilePath);
        ArgumentNullException.ThrowIfNull(installDirectory);

        string arguments = $"/SP- /DIR=\"{installDirectory}\" /VERYSILENT /SUPPRESSMSGBOXES /NORESTART";

        using Process process = new()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = setupFilePath,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };

        process.Start();
        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

        return process.ExitCode;
    }

    /// <summary>
    /// Geçici dizini temizler.
    /// </summary>
    public static void CleanupTempFiles()
    {
        if (Directory.Exists(TempDirectory))
        {
            try
            {
                Directory.Delete(TempDirectory, recursive: true);
            }
            catch (IOException)
            {
                // Dosyalar kullanımda olabilir
            }
        }
    }
}

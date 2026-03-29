namespace MikroUpdate.Win;

internal static class Program
{
    /// <summary>Uygulama genelinde tek örnek garantisi için Mutex adı.</summary>
    private const string MutexName = "Global\\MikroUpdate_SingleInstance_21CEB31C";

    /// <summary>Başlatma diagnostik log dosyası yolu.</summary>
    private static readonly string DiagLogPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "MikroUpdate", "logs", "trayapp_launch.log");

    /// <summary>
    /// MikroUpdate giriş noktası.
    /// Kullanım: MikroUpdate.exe [/auto] [/minimized]
    ///   /auto      : Sessiz modda versiyon kontrol, güncelleme ve Mikro başlatma — tray'de kalır.
    ///   /minimized : Tray'e küçültülmüş başlat (form görünmez).
    ///   (parametresiz) : Ayar ve güncelleme arayüzünü açar.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
        // Diagnostik: self-update sonrası başlatma kontrolü
        WriteDiagLog($"Main STARTED — Args: [{string.Join(", ", args)}], PID: {Environment.ProcessId}, Session: {Environment.GetEnvironmentVariable("SESSIONNAME") ?? "?"}, User: {Environment.UserName}");

        using Mutex mutex = new(true, MutexName, out bool isNewInstance);

        if (!isNewInstance)
        {
            WriteDiagLog("MUTEX CONFLICT — başka bir MikroUpdate instance çalışıyor, çıkılıyor.");

            return;
        }

        WriteDiagLog("Mutex alındı, uygulama başlatılıyor...");

        ApplicationConfiguration.Initialize();
        Application.SetColorMode(SystemColorMode.System);

        bool autoMode = args.Any(a =>
            a.Equals("/auto", StringComparison.OrdinalIgnoreCase) ||
            a.Equals("--auto", StringComparison.OrdinalIgnoreCase));

        bool startMinimized = autoMode || args.Any(a =>
            a.Equals("/minimized", StringComparison.OrdinalIgnoreCase) ||
            a.Equals("--minimized", StringComparison.OrdinalIgnoreCase));

        Application.Run(new Form1(autoMode, startMinimized));

        WriteDiagLog("Application.Run tamamlandı, çıkılıyor.");
    }

    /// <summary>
    /// Dosya tabanlı diagnostik log yazar. Tüm hatalar yutulur — asla crash'e neden olmaz.
    /// </summary>
    private static void WriteDiagLog(string message)
    {
        try
        {
            string dir = Path.GetDirectoryName(DiagLogPath)!;

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.AppendAllText(DiagLogPath,
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}{Environment.NewLine}");
        }
        catch
        {
            // Diagnostik log asla uygulamayı engellemez
        }
    }
}

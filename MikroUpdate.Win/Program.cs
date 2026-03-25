namespace MikroUpdate.Win;

internal static class Program
{
    /// <summary>Uygulama genelinde tek örnek garantisi için Mutex adı.</summary>
    private const string MutexName = "Global\\MikroUpdate_SingleInstance_21CEB31C";

    /// <summary>
    /// MikroUpdate giriş noktası.
    /// Kullanım: MikroUpdate.exe [/auto]
    ///   /auto : Sessiz modda versiyon kontrol, güncelleme ve Mikro başlatma.
    ///   (parametresiz) : Ayar ve güncelleme arayüzünü açar.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
        using Mutex mutex = new(true, MutexName, out bool isNewInstance);

        if (!isNewInstance)
        {
            // Zaten çalışan bir örnek var — çift tray icon'u önle
            return;
        }

        ApplicationConfiguration.Initialize();
        Application.SetColorMode(SystemColorMode.System);

        bool autoMode = args.Any(a =>
            a.Equals("/auto", StringComparison.OrdinalIgnoreCase) ||
            a.Equals("--auto", StringComparison.OrdinalIgnoreCase));

        Application.Run(new Form1(autoMode));
    }
}

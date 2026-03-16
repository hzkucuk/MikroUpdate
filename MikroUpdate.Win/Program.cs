namespace MikroUpdate.Win;

internal static class Program
{
    /// <summary>
    /// MikroUpdate giriş noktası.
    /// Kullanım: MikroUpdate.exe [/auto]
    ///   /auto : Sessiz modda versiyon kontrol, güncelleme ve Mikro başlatma.
    ///   (parametresiz) : Ayar ve güncelleme arayüzünü açar.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        Application.SetColorMode(SystemColorMode.System);

        bool autoMode = args.Any(a =>
            a.Equals("/auto", StringComparison.OrdinalIgnoreCase) ||
            a.Equals("--auto", StringComparison.OrdinalIgnoreCase));

        Application.Run(new Form1(autoMode));
    }
}

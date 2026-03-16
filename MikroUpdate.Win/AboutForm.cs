using System.Diagnostics;
using System.Reflection;

namespace MikroUpdate.Win;

/// <summary>
/// Program hakkında bilgi dialog penceresi.
/// Versiyon, geliştirici, lisans, teknoloji ve bağlantı bilgilerini gösterir.
/// </summary>
public partial class AboutForm : Form
{
    private const string GitHubUrl = "https://github.com/hzkucuk/MikroUpdate";
    private const string EmailAddress = "hzkucuk@gmail.com";

    public AboutForm()
    {
        InitializeComponent();

        string version = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString()
            ?? "—";

        // +commitHash kısmını kes (ör: "1.9.0+abc123" → "1.9.0")
        int plusIndex = version.IndexOf('+');

        if (plusIndex > 0)
        {
            version = version[..plusIndex];
        }

        _lblVersion.Text = $"v{version}";
    }

    private void LnkGitHub_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
    {
        OpenUrl(GitHubUrl);
    }

    private void LnkEmail_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
    {
        OpenUrl($"mailto:{EmailAddress}");
    }

    private static void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch
        {
            // Tarayıcı açılamadı — sessizce devam et
        }
    }
}

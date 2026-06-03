using System.ComponentModel;
using System.Runtime.InteropServices;

using MikroUpdate.Shared.Models;
using MikroUpdate.Shared.Security;

namespace MikroUpdate.Service.Services;

/// <summary>
/// Credential modunda UNC paylaşımına geçici ağ oturumu açar.
/// </summary>
internal sealed class NetworkShareAccessScope : IDisposable
{
    private readonly string? _remoteName;
    private readonly bool _connected;

    private NetworkShareAccessScope()
    {
    }

    private NetworkShareAccessScope(string remoteName)
    {
        _remoteName = remoteName;
        _connected = true;
    }

    /// <summary>
    /// Gerekiyorsa hedef UNC yolu için kimlikli ağ oturumu açar.
    /// </summary>
    public static NetworkShareAccessScope OpenIfRequired(UpdateConfig config, string targetPath)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(targetPath);

        if (config.NetworkAccessMode != NetworkAccessMode.Credential || !targetPath.StartsWith(@"\\", StringComparison.Ordinal))
        {
            return new NetworkShareAccessScope();
        }

        if (string.IsNullOrWhiteSpace(config.ServerUsername))
        {
            throw new InvalidOperationException("Credential modunda sunucu kullanıcı adı boş olamaz.");
        }

        string? password = NetworkCredentialProtector.Unprotect(config.EncryptedServerPassword);

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("Credential modunda geçerli sunucu parolası çözülemedi.");
        }

        string remoteName = GetShareRoot(targetPath);
        NetResource resource = new()
        {
            Scope = 0,
            ResourceType = 1,
            DisplayType = 0,
            Usage = 0,
            LocalName = null,
            RemoteName = remoteName,
            Comment = null,
            Provider = null
        };

        int result = WNetAddConnection2(resource, password, config.ServerUsername, 0);

        if (result != 0)
        {
            throw new IOException(
                $"UNC bağlantısı açılamadı: {remoteName} (Win32: {result})",
                new Win32Exception(result));
        }

        return new NetworkShareAccessScope(remoteName);
    }

    public void Dispose()
    {
        if (!_connected || string.IsNullOrWhiteSpace(_remoteName))
        {
            return;
        }

        _ = WNetCancelConnection2(_remoteName, 0, true);
    }

    private static string GetShareRoot(string uncPath)
    {
        string path = uncPath.Trim();

        if (!path.StartsWith(@"\\", StringComparison.Ordinal))
        {
            throw new ArgumentException("UNC yolu bekleniyor.", nameof(uncPath));
        }

        string withoutPrefix = path[2..];
        int firstSeparator = withoutPrefix.IndexOf('\\');

        if (firstSeparator < 0)
        {
            throw new ArgumentException("UNC yolu geçersiz: paylaşım adı bulunamadı.", nameof(uncPath));
        }

        int secondSeparator = withoutPrefix.IndexOf('\\', firstSeparator + 1);

        return secondSeparator < 0
            ? path
            : $@"\\{withoutPrefix[..secondSeparator]}";
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct NetResource
    {
        public int Scope;
        public int ResourceType;
        public int DisplayType;
        public int Usage;
        public string? LocalName;
        public string? RemoteName;
        public string? Comment;
        public string? Provider;
    }

    [DllImport("mpr.dll", CharSet = CharSet.Unicode)]
    private static extern int WNetAddConnection2(
        [In] NetResource netResource,
        [In] string password,
        [In] string username,
        [In] int flags);

    [DllImport("mpr.dll", CharSet = CharSet.Unicode)]
    private static extern int WNetCancelConnection2(
        [In] string name,
        [In] int flags,
        [In] bool force);
}

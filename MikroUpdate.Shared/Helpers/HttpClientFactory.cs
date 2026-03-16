using System.Net;

namespace MikroUpdate.Shared.Helpers;

/// <summary>
/// Merkezi HttpClient oluşturucu.
/// Proxy, timeout ve ortak SocketsHttpHandler ayarlarını tek noktada yönetir.
/// Tüm HTTP servisleri bu factory üzerinden HttpClient almalıdır.
/// </summary>
public static class HttpClientFactory
{
    private const string DefaultUserAgent = "MikroUpdate/1.0";

    /// <summary>
    /// Yapılandırılmış bir <see cref="HttpClient"/> oluşturur.
    /// </summary>
    /// <param name="proxyAddress">Proxy adresi (ör: "http://proxy:8080"). Boş/null ise proxy kullanılmaz.</param>
    /// <param name="timeoutSeconds">HTTP istek zaman aşımı (saniye). 0 veya negatif ise varsayılan kullanılır.</param>
    /// <param name="defaultTimeoutSeconds">Varsayılan zaman aşımı (saniye). <paramref name="timeoutSeconds"/> geçersizse bu değer kullanılır.</param>
    /// <param name="connectTimeoutSeconds">Bağlantı kurma zaman aşımı (saniye).</param>
    public static HttpClient Create(
        string? proxyAddress = null,
        int timeoutSeconds = 0,
        int defaultTimeoutSeconds = 30,
        int connectTimeoutSeconds = 10)
    {
        SocketsHttpHandler handler = new()
        {
            ConnectTimeout = TimeSpan.FromSeconds(connectTimeoutSeconds),
            PooledConnectionLifetime = TimeSpan.FromMinutes(5)
        };

        // Proxy yapılandırması
        if (!string.IsNullOrWhiteSpace(proxyAddress))
        {
            handler.Proxy = new WebProxy(proxyAddress)
            {
                BypassProxyOnLocal = true
            };
            handler.UseProxy = true;
        }

        int effectiveTimeout = timeoutSeconds > 0 ? timeoutSeconds : defaultTimeoutSeconds;

        HttpClient client = new(handler)
        {
            Timeout = TimeSpan.FromSeconds(effectiveTimeout)
        };

        client.DefaultRequestHeaders.UserAgent.ParseAdd(DefaultUserAgent);

        return client;
    }

    /// <summary>
    /// İndirme işlemi için optimize edilmiş HttpClient oluşturur (uzun timeout).
    /// </summary>
    /// <param name="proxyAddress">Proxy adresi.</param>
    /// <param name="timeoutSeconds">Yapılandırılmış zaman aşımı. 0 ise varsayılan 30 dk.</param>
    public static HttpClient CreateForDownload(
        string? proxyAddress = null,
        int timeoutSeconds = 0)
    {
        return Create(
            proxyAddress,
            timeoutSeconds,
            defaultTimeoutSeconds: 1800, // 30 dk varsayılan
            connectTimeoutSeconds: 15);
    }
}

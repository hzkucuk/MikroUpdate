using System.IO.Pipes;

using MikroUpdate.Shared;
using MikroUpdate.Shared.Messages;

namespace MikroUpdate.Win.Services;

/// <summary>
/// Named Pipe istemcisi.
/// MikroUpdate Windows Service ile haberleşmeyi sağlar.
/// Tekli yanıt ve ilerleme akışı (progress streaming) destekler.
/// </summary>
public sealed class PipeClient
{
    /// <summary>
    /// Hata bildirim callback'i. Ayarlandığında pipe hataları bu callback ile raporlanır.
    /// </summary>
    public Action<string>? OnError { get; set; }

    /// <summary>
    /// Servise komut gönderir ve yanıt alır.
    /// </summary>
    /// <returns>Servis yanıtı veya bağlantı hatası durumunda null.</returns>
    public async Task<ServiceResponse?> SendCommandAsync(
        CommandType command,
        CancellationToken cancellationToken = default)
    {
        return await SendCommandAsync(command, data: null, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Servise ek veri ile birlikte komut gönderir ve yanıt alır.
    /// </summary>
    /// <param name="command">Gönderilecek komut türü.</param>
    /// <param name="data">Komut ile birlikte gönderilecek ek veri (ör. dosya yolu).</param>
    /// <param name="cancellationToken">İptal tokenı.</param>
    /// <returns>Servis yanıtı veya bağlantı hatası durumunda null.</returns>
    public async Task<ServiceResponse?> SendCommandAsync(
        CommandType command,
        string? data,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using NamedPipeClientStream pipeClient = new(
                ".",
                PipeConstants.PipeName,
                PipeDirection.InOut,
                PipeOptions.Asynchronous);

            await pipeClient.ConnectAsync(
                PipeConstants.ConnectionTimeoutMs, cancellationToken).ConfigureAwait(false);

            // Komutu gönder (length-prefixed)
            ServiceCommand serviceCommand = new() { Command = command, Data = data };
            await PipeProtocol.WriteMessageAsync(pipeClient, serviceCommand, cancellationToken)
                .ConfigureAwait(false);

            // Yanıtı oku (length-prefixed)
            ServiceResponse? response = await PipeProtocol.ReadMessageAsync<ServiceResponse>(
                pipeClient, cancellationToken).ConfigureAwait(false);

            return response;
        }
        catch (TimeoutException)
        {
            OnError?.Invoke($"Servis bağlantı zaman aşımı ({PipeConstants.ConnectionTimeoutMs}ms) — komut: {command}");

            return null;
        }
        catch (IOException ex)
        {
            OnError?.Invoke($"Servis pipe IO hatası — komut: {command} | {ex.Message}");

            return null;
        }
    }

    /// <summary>
    /// Servise komut gönderir ve ilerleme akışı ile birden fazla yanıt okur.
    /// Her ara mesaj (IsProgressMessage=true) geldiğinde <paramref name="onProgress"/> callback'i çağrılır.
    /// Son mesaj (IsProgressMessage=false) ile döner.
    /// </summary>
    /// <param name="command">Gönderilecek komut türü.</param>
    /// <param name="onProgress">Ara ilerleme mesajları için callback.</param>
    /// <param name="cancellationToken">İptal tokenı.</param>
    /// <returns>Son (terminal) servis yanıtı veya bağlantı hatası durumunda null.</returns>
    public async Task<ServiceResponse?> SendCommandWithProgressAsync(
        CommandType command,
        Action<ServiceResponse> onProgress,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(onProgress);

        try
        {
            await using NamedPipeClientStream pipeClient = new(
                ".",
                PipeConstants.PipeName,
                PipeDirection.InOut,
                PipeOptions.Asynchronous);

            await pipeClient.ConnectAsync(
                PipeConstants.ConnectionTimeoutMs, cancellationToken).ConfigureAwait(false);

            // Komutu gönder
            ServiceCommand serviceCommand = new() { Command = command };
            await PipeProtocol.WriteMessageAsync(pipeClient, serviceCommand, cancellationToken)
                .ConfigureAwait(false);

            // İlerleme mesajlarını oku (loop until terminal response)
            while (true)
            {
                ServiceResponse? response = await PipeProtocol.ReadMessageAsync<ServiceResponse>(
                    pipeClient, cancellationToken).ConfigureAwait(false);

                if (response is null)
                {
                    OnError?.Invoke($"Servis pipe bağlantısı kapandı — komut: {command}");

                    return null;
                }

                // Ara ilerleme mesajı
                if (response.IsProgressMessage)
                {
                    onProgress(response);

                    continue;
                }

                // Terminal yanıt
                return response;
            }
        }
        catch (TimeoutException)
        {
            OnError?.Invoke($"Servis bağlantı zaman aşımı ({PipeConstants.ConnectionTimeoutMs}ms) — komut: {command}");

            return null;
        }
        catch (IOException ex)
        {
            OnError?.Invoke($"Servis pipe IO hatası — komut: {command} | {ex.Message}");

            return null;
        }
    }

    /// <summary>
    /// Servisin çalışıp çalışmadığını kontrol eder.
    /// </summary>
    public async Task<bool> IsServiceRunningAsync(CancellationToken cancellationToken = default)
    {
        ServiceResponse? response = await SendCommandAsync(
            CommandType.GetStatus, cancellationToken).ConfigureAwait(false);

        return response is not null;
    }
}

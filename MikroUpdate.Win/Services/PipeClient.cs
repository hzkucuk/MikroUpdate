using System.IO.Pipes;

using MikroUpdate.Shared;
using MikroUpdate.Shared.Messages;

namespace MikroUpdate.Win.Services;

/// <summary>
/// Named Pipe istemcisi.
/// MikroUpdate Windows Service ile haberleşmeyi sağlar.
/// </summary>
public sealed class PipeClient
{
    /// <summary>
    /// Servise komut gönderir ve yanıt alır.
    /// </summary>
    /// <returns>Servis yanıtı veya bağlantı hatası durumunda null.</returns>
    public async Task<ServiceResponse?> SendCommandAsync(
        CommandType command,
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
            ServiceCommand serviceCommand = new() { Command = command };
            await PipeProtocol.WriteMessageAsync(pipeClient, serviceCommand, cancellationToken)
                .ConfigureAwait(false);

            // Yanıtı oku (length-prefixed)
            ServiceResponse? response = await PipeProtocol.ReadMessageAsync<ServiceResponse>(
                pipeClient, cancellationToken).ConfigureAwait(false);

            return response;
        }
        catch (TimeoutException)
        {
            return null;
        }
        catch (IOException)
        {
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

using System.Buffers;
using System.Text.Json;

namespace MikroUpdate.Shared;

/// <summary>
/// Named Pipe üzerinden uzunluk önekli (length-prefixed) JSON mesaj iletişimi.
/// Protokol: [4-byte uzunluk (Int32 LE)] + [UTF-8 JSON payload]
/// </summary>
public static class PipeProtocol
{
    /// <summary>
    /// Mesajı uzunluk öneki ile stream'e yazar.
    /// </summary>
    public static async Task WriteMessageAsync<T>(
        Stream stream,
        T message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(message);

        byte[] payload = JsonSerializer.SerializeToUtf8Bytes(message);
        byte[] lengthPrefix = BitConverter.GetBytes(payload.Length);

        await stream.WriteAsync(lengthPrefix, cancellationToken).ConfigureAwait(false);
        await stream.WriteAsync(payload, cancellationToken).ConfigureAwait(false);
        await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Stream'den uzunluk önekli mesajı okur.
    /// </summary>
    public static async Task<T?> ReadMessageAsync<T>(
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        // 4-byte uzunluk oku
        byte[] lengthBuffer = new byte[4];
        int bytesRead = await ReadExactAsync(stream, lengthBuffer, cancellationToken).ConfigureAwait(false);

        if (bytesRead < 4)
        {
            return default;
        }

        int payloadLength = BitConverter.ToInt32(lengthBuffer, 0);

        if (payloadLength <= 0 || payloadLength > 1024 * 1024) // Max 1MB
        {
            return default;
        }

        // Payload oku
        byte[] payload = new byte[payloadLength];
        bytesRead = await ReadExactAsync(stream, payload, cancellationToken).ConfigureAwait(false);

        if (bytesRead < payloadLength)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(payload);
    }

    /// <summary>
    /// Stream'den tam olarak belirtilen sayıda byte okur.
    /// </summary>
    private static async Task<int> ReadExactAsync(
        Stream stream,
        byte[] buffer,
        CancellationToken cancellationToken)
    {
        int totalRead = 0;

        while (totalRead < buffer.Length)
        {
            int read = await stream.ReadAsync(
                buffer.AsMemory(totalRead, buffer.Length - totalRead),
                cancellationToken).ConfigureAwait(false);

            if (read == 0)
            {
                break; // Stream kapandı
            }

            totalRead += read;
        }

        return totalRead;
    }
}

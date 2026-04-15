using System.Buffers.Binary;

namespace MikroUpdate.Service.Services;

/// <summary>
/// HTTP Range request'leri kullanarak uzak PE (Portable Executable) dosyasının
/// <c>VS_FIXEDFILEINFO</c> kaydından FileVersion bilgisini okur.
/// Tüm dosyayı indirmeden yalnızca gerekli byte aralıklarını çeker (toplam ~10-20 KB).
/// </summary>
/// <remarks>
/// PE yapısı:
/// <list type="number">
///   <item>DOS Header (offset 0x3C → PE signature offset)</item>
///   <item>PE Signature + COFF Header + Optional Header → Section Table</item>
///   <item>Section Table → <c>.rsrc</c> section'ın dosyadaki offset'i</item>
///   <item>Resource Directory → <c>RT_VERSION</c> (type=16) resource'unun offset'i</item>
///   <item><c>VS_FIXEDFILEINFO</c> struct → FileVersionMS / FileVersionLS</item>
/// </list>
/// </remarks>
internal sealed class PeVersionReader
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;

    /// <summary>VS_FIXEDFILEINFO signature: 0xFEEF04BD.</summary>
    private const uint VsFixedFileInfoSignature = 0xFEEF04BD;

    /// <summary>RT_VERSION resource type ID.</summary>
    private const int RtVersion = 16;

    public PeVersionReader(HttpClient httpClient, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(logger);
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Belirtilen URL'deki PE dosyasının FileVersion bilgisini HTTP Range request'leri ile okur.
    /// </summary>
    /// <param name="url">PE dosyasının tam CDN URL'si.</param>
    /// <param name="cancellationToken">İptal token'ı.</param>
    /// <returns>FileVersion veya okunamazsa null.</returns>
    public async Task<Version?> GetFileVersionAsync(string url, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(url);

        try
        {
            // 1. DOS Header + PE Header + Section Table — ilk 4 KB yeterli
            byte[]? headerBytes = await ReadRangeAsync(url, 0, 4096, cancellationToken).ConfigureAwait(false);

            if (headerBytes is null || headerBytes.Length < 64)
            {
                _logger.LogDebug("PE header okunamadı: {Url}", url);
                return null;
            }

            // DOS Header — e_lfanew (PE signature offset) at 0x3C
            int peOffset = BinaryPrimitives.ReadInt32LittleEndian(headerBytes.AsSpan(0x3C));

            if (peOffset < 0 || peOffset + 24 > headerBytes.Length)
            {
                _logger.LogDebug("Geçersiz PE offset: {Offset}", peOffset);
                return null;
            }

            // PE Signature check: "PE\0\0"
            uint peSignature = BinaryPrimitives.ReadUInt32LittleEndian(headerBytes.AsSpan(peOffset));

            if (peSignature != 0x00004550)
            {
                _logger.LogDebug("Geçersiz PE signature: 0x{Sig:X8}", peSignature);
                return null;
            }

            // COFF Header: NumberOfSections at peOffset+6, SizeOfOptionalHeader at peOffset+20
            ushort numberOfSections = BinaryPrimitives.ReadUInt16LittleEndian(headerBytes.AsSpan(peOffset + 6));
            ushort sizeOfOptionalHeader = BinaryPrimitives.ReadUInt16LittleEndian(headerBytes.AsSpan(peOffset + 20));

            // Section table starts after Optional Header
            int sectionTableOffset = peOffset + 24 + sizeOfOptionalHeader;

            _logger.LogDebug("PE parse: peOffset={PeOffset}, sections={Sections}, optHdrSize={OptSize}, sectionTableAt={SecTable}",
                peOffset, numberOfSections, sizeOfOptionalHeader, sectionTableOffset);

            // Her section entry 40 byte — .rsrc section'ı bul
            int rsrcVirtualAddress = 0;
            int rsrcFileOffset = 0;
            int rsrcSize = 0;

            for (int i = 0; i < numberOfSections; i++)
            {
                int entryOffset = sectionTableOffset + (i * 40);

                // Section table headerBytes dışına taşarsa daha fazla veri oku
                if (entryOffset + 40 > headerBytes.Length)
                {
                    int neededSize = sectionTableOffset + (numberOfSections * 40);
                    headerBytes = await ReadRangeAsync(url, 0, neededSize, cancellationToken).ConfigureAwait(false);

                    if (headerBytes is null || entryOffset + 40 > headerBytes.Length)
                    {
                        _logger.LogDebug("Section table okunamadı, gerekli boyut: {Size}", neededSize);
                        return null;
                    }
                }

                // Section name: ilk 8 byte (ASCII, null-padded)
                ReadOnlySpan<byte> nameBytes = headerBytes.AsSpan(entryOffset, 8);

                if (nameBytes[0] == (byte)'.' &&
                    nameBytes[1] == (byte)'r' &&
                    nameBytes[2] == (byte)'s' &&
                    nameBytes[3] == (byte)'r' &&
                    nameBytes[4] == (byte)'c')
                {
                    rsrcVirtualAddress = BinaryPrimitives.ReadInt32LittleEndian(headerBytes.AsSpan(entryOffset + 12));
                    rsrcSize = BinaryPrimitives.ReadInt32LittleEndian(headerBytes.AsSpan(entryOffset + 16));
                    rsrcFileOffset = BinaryPrimitives.ReadInt32LittleEndian(headerBytes.AsSpan(entryOffset + 20));
                    break;
                }
            }

            if (rsrcFileOffset == 0)
            {
                _logger.LogDebug(".rsrc section bulunamadı: {Url}", url);
                return null;
            }

            _logger.LogDebug(".rsrc section bulundu: VA=0x{VA:X}, fileOffset=0x{FO:X}, size={Size}",
                rsrcVirtualAddress, rsrcFileOffset, rsrcSize);

            // 2. .rsrc section'ın başından yeterli miktarda oku
            // RT_VERSION resource genellikle .rsrc section'ın başlarında bulunur
            // İlk 8 KB genellikle yeterli (version info küçük bir resource)
            int rsrcReadSize = Math.Min(rsrcSize, 8192);
            byte[]? rsrcBytes = await ReadRangeAsync(url, rsrcFileOffset, rsrcReadSize, cancellationToken)
                .ConfigureAwait(false);

            if (rsrcBytes is null || rsrcBytes.Length < 16)
            {
                _logger.LogDebug(".rsrc section okunamadı: {Url}", url);
                return null;
            }

            _logger.LogDebug(".rsrc okundu: {BytesRead} byte", rsrcBytes.Length);

            // 3. Resource directory'den RT_VERSION resource'unun offset'ini bul
            int? versionOffset = FindVersionResourceOffset(rsrcBytes, rsrcVirtualAddress, rsrcFileOffset);

            if (versionOffset is null)
            {
                // Fallback: VS_FIXEDFILEINFO signature'ı brute-force ara
                _logger.LogDebug("RT_VERSION resource bulunamadı, signature scan deneniyor...");
                Version? scanResult = FindVersionBySignatureScan(rsrcBytes);
                _logger.LogDebug("Signature scan sonucu: {Result}", scanResult?.ToString() ?? "null");
                return scanResult;
            }

            int localOffset = versionOffset.Value - rsrcFileOffset;

            if (localOffset < 0 || localOffset + 200 > rsrcBytes.Length)
            {
                // Daha fazla veri gerekebilir — genişletilmiş okuma
                int extendedSize = Math.Min(rsrcSize, localOffset + 4096);
                rsrcBytes = await ReadRangeAsync(url, rsrcFileOffset, extendedSize, cancellationToken)
                    .ConfigureAwait(false);

                if (rsrcBytes is null || localOffset + 52 > rsrcBytes.Length)
                {
                    return null;
                }
            }

            // VS_VERSION_INFO içinde VS_FIXEDFILEINFO'yı bul
            return FindVersionBySignatureScan(rsrcBytes.AsSpan(localOffset));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogDebug("PE version okuma hatası ({Url}): {Error}", url, ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Resource directory yapısını parse ederek RT_VERSION resource'unun dosya offset'ini bulur.
    /// </summary>
    private static int? FindVersionResourceOffset(
        byte[] rsrcBytes, int rsrcVirtualAddress, int rsrcFileOffset)
    {
        if (rsrcBytes.Length < 16)
        {
            return null;
        }

        // Resource Directory Table: NumberOfNamedEntries (offset 12), NumberOfIdEntries (offset 14)
        int namedEntries = BinaryPrimitives.ReadUInt16LittleEndian(rsrcBytes.AsSpan(12));
        int idEntries = BinaryPrimitives.ReadUInt16LittleEndian(rsrcBytes.AsSpan(14));
        int totalEntries = namedEntries + idEntries;

        // Her entry 8 byte: Name/ID (4) + OffsetToData (4)
        for (int i = 0; i < totalEntries; i++)
        {
            int entryOffset = 16 + (i * 8);

            if (entryOffset + 8 > rsrcBytes.Length)
            {
                return null;
            }

            uint nameOrId = BinaryPrimitives.ReadUInt32LittleEndian(rsrcBytes.AsSpan(entryOffset));
            uint dataOffset = BinaryPrimitives.ReadUInt32LittleEndian(rsrcBytes.AsSpan(entryOffset + 4));

            // RT_VERSION = 16
            if (nameOrId != RtVersion)
            {
                continue;
            }

            // High bit set = subdirectory
            if ((dataOffset & 0x80000000) == 0)
            {
                continue;
            }

            int subDirOffset = (int)(dataOffset & 0x7FFFFFFF);

            // Navigate to leaf (2 more levels: language-neutral → leaf data entry)
            return NavigateToLeaf(rsrcBytes, subDirOffset, rsrcVirtualAddress, rsrcFileOffset);
        }

        return null;
    }

    /// <summary>
    /// Resource sub-directory'lerden geçerek leaf data entry'nin dosya offset'ini bulur.
    /// </summary>
    private static int? NavigateToLeaf(
        byte[] rsrcBytes, int dirOffset, int rsrcVirtualAddress, int rsrcFileOffset)
    {
        // Max 3 seviye (type → name → language) — 2 seviye daha iner
        for (int level = 0; level < 2; level++)
        {
            if (dirOffset + 16 > rsrcBytes.Length)
            {
                return null;
            }

            int named = BinaryPrimitives.ReadUInt16LittleEndian(rsrcBytes.AsSpan(dirOffset + 12));
            int id = BinaryPrimitives.ReadUInt16LittleEndian(rsrcBytes.AsSpan(dirOffset + 14));

            if (named + id == 0)
            {
                return null;
            }

            // İlk entry'yi al
            int firstEntryOffset = dirOffset + 16;

            if (firstEntryOffset + 8 > rsrcBytes.Length)
            {
                return null;
            }

            uint entryData = BinaryPrimitives.ReadUInt32LittleEndian(rsrcBytes.AsSpan(firstEntryOffset + 4));

            if ((entryData & 0x80000000) != 0)
            {
                // Subdirectory — devam et
                dirOffset = (int)(entryData & 0x7FFFFFFF);
            }
            else
            {
                // Leaf data entry
                int leafOffset = (int)entryData;

                if (leafOffset + 16 > rsrcBytes.Length)
                {
                    return null;
                }

                // Data Entry: OffsetToData (RVA, 4 bytes), Size (4 bytes)
                uint dataRva = BinaryPrimitives.ReadUInt32LittleEndian(rsrcBytes.AsSpan(leafOffset));

                // RVA → file offset
                return (int)(dataRva - rsrcVirtualAddress) + rsrcFileOffset;
            }
        }

        // Son seviye — leaf olmalı
        if (dirOffset + 16 > rsrcBytes.Length)
        {
            return null;
        }

        int lastNamed = BinaryPrimitives.ReadUInt16LittleEndian(rsrcBytes.AsSpan(dirOffset + 12));
        int lastId = BinaryPrimitives.ReadUInt16LittleEndian(rsrcBytes.AsSpan(dirOffset + 14));

        if (lastNamed + lastId == 0)
        {
            return null;
        }

        int lastEntry = dirOffset + 16;

        if (lastEntry + 8 > rsrcBytes.Length)
        {
            return null;
        }

        uint lastData = BinaryPrimitives.ReadUInt32LittleEndian(rsrcBytes.AsSpan(lastEntry + 4));

        if ((lastData & 0x80000000) != 0)
        {
            return null; // Hâlâ subdirectory — çok derin
        }

        int finalLeaf = (int)lastData;

        if (finalLeaf + 8 > rsrcBytes.Length)
        {
            return null;
        }

        uint finalRva = BinaryPrimitives.ReadUInt32LittleEndian(rsrcBytes.AsSpan(finalLeaf));

        return (int)(finalRva - rsrcVirtualAddress) + rsrcFileOffset;
    }

    /// <summary>
    /// Verilen byte aralığında <c>VS_FIXEDFILEINFO</c> signature'ını (0xFEEF04BD) arar
    /// ve FileVersion değerlerini çıkarır.
    /// </summary>
    private static Version? FindVersionBySignatureScan(ReadOnlySpan<byte> data)
    {
        // VS_FIXEDFILEINFO: signature (4) + structVersion (4) + fileVersionMS (4) + fileVersionLS (4)
        // Signature = 0xFEEF04BD
        for (int i = 0; i <= data.Length - 52; i++)
        {
            uint sig = BinaryPrimitives.ReadUInt32LittleEndian(data[i..]);

            if (sig != VsFixedFileInfoSignature)
            {
                continue;
            }

            // fileVersionMS at offset +8, fileVersionLS at offset +12
            uint fileVersionMs = BinaryPrimitives.ReadUInt32LittleEndian(data[(i + 8)..]);
            uint fileVersionLs = BinaryPrimitives.ReadUInt32LittleEndian(data[(i + 12)..]);

            int major = (int)(fileVersionMs >> 16);
            int minor = (int)(fileVersionMs & 0xFFFF);
            int build = (int)(fileVersionLs >> 16);
            int revision = (int)(fileVersionLs & 0xFFFF);

            return new Version(major, minor, build, revision);
        }

        return null;
    }

    /// <summary>
    /// HTTP Range request ile belirtilen byte aralığını okur.
    /// </summary>
    private async Task<byte[]?> ReadRangeAsync(
        string url, long offset, int length, CancellationToken cancellationToken)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, url);
        request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(offset, offset + length - 1);

        using HttpResponseMessage response = await _httpClient.SendAsync(
            request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

        if (response.StatusCode is not (System.Net.HttpStatusCode.PartialContent or System.Net.HttpStatusCode.OK))
        {
            _logger.LogDebug("HTTP Range isteği başarısız: {StatusCode} ({Url})", response.StatusCode, url);
            return null;
        }

        byte[] data = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogDebug("HTTP Range yanıt: status={Status}, istenen={Requested}, alınan={Received}, url={Url}",
            response.StatusCode, length, data.Length, url);

        // OK (200) döndüyse sunucu Range desteklemiyor — tüm dosyayı göndermiş olabilir
        // Bu durumda çok büyük veri gelirse iptal et
        if (response.StatusCode == System.Net.HttpStatusCode.OK && data.Length > length * 10)
        {
            _logger.LogDebug("Sunucu Range desteklemiyor, çok büyük yanıt: {Size} byte", data.Length);
            return null;
        }

        return data;
    }
}

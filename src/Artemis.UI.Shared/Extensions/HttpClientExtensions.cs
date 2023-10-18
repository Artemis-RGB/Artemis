using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Shared.Utilities;

namespace Artemis.UI.Shared.Extensions;

/// <summary>
///     Provides extension methods for the <see cref="HttpClient" /> type.
/// </summary>
public static class HttpClientProgressExtensions
{
    /// <summary>
    /// Send a GET request to the specified Uri with an HTTP completion option and a cancellation token as an asynchronous operation.
    /// </summary>
    /// <param name="client">The HTTP client to use.</param>
    /// <param name="requestUrl">The Uri the request is sent to.</param>
    /// <param name="destination">The destination stream.</param>
    /// <param name="progress">The progress instance to use for progress indication.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    public static async Task DownloadDataAsync(this HttpClient client, string requestUrl, Stream destination, IProgress<StreamProgress>? progress, CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await client.GetAsync(requestUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        long? contentLength = response.Content.Headers.ContentLength;
        await using Stream download = await response.Content.ReadAsStreamAsync(cancellationToken);
        // no progress... no contentLength... very sad
        if (progress is null || !contentLength.HasValue)
        {
            await download.CopyToAsync(destination, cancellationToken);
            return;
        }

        // Such progress and contentLength much reporting Wow!
        await download.CopyToAsync(destination, 81920, progress, contentLength, cancellationToken);
    }

    private static async Task CopyToAsync(this Stream source, Stream destination, int bufferSize, IProgress<StreamProgress> progress, long? contentLength, CancellationToken cancellationToken)
    {
        if (bufferSize < 0)
            throw new ArgumentOutOfRangeException(nameof(bufferSize));
        if (source is null)
            throw new ArgumentNullException(nameof(source));
        if (!source.CanRead)
            throw new InvalidOperationException($"'{nameof(source)}' is not readable.");
        if (destination == null)
            throw new ArgumentNullException(nameof(destination));
        if (!destination.CanWrite)
            throw new InvalidOperationException($"'{nameof(destination)}' is not writable.");

        byte[] buffer = new byte[bufferSize];
        long totalBytesRead = 0;
        int bytesRead;
        while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) != 0)
        {
            await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
            totalBytesRead += bytesRead;
            progress?.Report(new StreamProgress(totalBytesRead, contentLength ?? totalBytesRead));
        }
    }
}
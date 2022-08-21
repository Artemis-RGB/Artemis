// Based on: https://www.codeproject.com/Tips/5274597/An-Improved-Stream-CopyToAsync-that-Reports-Progre
// The MIT License
//
// Copyright (c) 2020 honey the codewitch
//
// Permission is hereby granted, free of charge,
// to any person obtaining a copy of this software and 
// associated documentation files (the "Software"), to
// deal in the Software without restriction, including 
// without limitation the rights to use, copy, modify,
// merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom 
// the Software is furnished to do so,
// subject to the following conditions:
//
// The above copyright notice and this permission notice 
// shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR 
// ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Artemis.Core;

internal static class StreamExtensions
{
    private const int DefaultBufferSize = 81920;

    /// <summary>
    ///     Copies a stream to another stream
    /// </summary>
    /// <param name="source">The source <see cref="Stream" /> to copy from</param>
    /// <param name="sourceLength">The length of the source stream, if known - used for progress reporting</param>
    /// <param name="destination">The destination <see cref="Stream" /> to copy to</param>
    /// <param name="bufferSize">The size of the copy block buffer</param>
    /// <param name="progress">An <see cref="IProgress{T}" /> implementation for reporting progress</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>A task representing the operation</returns>
    public static async Task CopyToAsync(
        this Stream source,
        long sourceLength,
        Stream destination,
        int bufferSize,
        IProgress<(long, long)> progress,
        CancellationToken cancellationToken)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        if (!source.CanRead)
            throw new ArgumentException("Has to be readable", nameof(source));
        if (destination == null)
            throw new ArgumentNullException(nameof(destination));
        if (!destination.CanWrite)
            throw new ArgumentException("Has to be writable", nameof(destination));
        if (bufferSize <= 0)
            bufferSize = DefaultBufferSize;

        byte[] buffer = new byte[bufferSize];
        long totalBytesRead = 0;
        int bytesRead;
        while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) != 0)
        {
            await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
            totalBytesRead += bytesRead;
            progress?.Report((totalBytesRead, sourceLength));
        }

        progress?.Report((totalBytesRead, sourceLength));
        cancellationToken.ThrowIfCancellationRequested();
    }

    /// <summary>
    ///     Copies a stream to another stream
    /// </summary>
    /// <param name="source">The source <see cref="Stream" /> to copy from</param>
    /// <param name="sourceLength">The length of the source stream, if known - used for progress reporting</param>
    /// <param name="destination">The destination <see cref="Stream" /> to copy to</param>
    /// <param name="progress">An <see cref="IProgress{T}" /> implementation for reporting progress</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>A task representing the operation</returns>
    public static Task CopyToAsync(this Stream source, long sourceLength, Stream destination, IProgress<(long, long)> progress, CancellationToken cancellationToken)
    {
        return CopyToAsync(source, sourceLength, destination, 0, progress, cancellationToken);
    }

    /// <summary>
    ///     Copies a stream to another stream
    /// </summary>
    /// <param name="source">The source <see cref="Stream" /> to copy from</param>
    /// <param name="destination">The destination <see cref="Stream" /> to copy to</param>
    /// <param name="progress">An <see cref="IProgress{T}" /> implementation for reporting progress</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>A task representing the operation</returns>
    public static Task CopyToAsync(this Stream source, Stream destination, IProgress<(long, long)> progress, CancellationToken cancellationToken)
    {
        return CopyToAsync(source, 0L, destination, 0, progress, cancellationToken);
    }

    /// <summary>
    ///     Copies a stream to another stream
    /// </summary>
    /// <param name="source">The source <see cref="Stream" /> to copy from</param>
    /// <param name="sourceLength">The length of the source stream, if known - used for progress reporting</param>
    /// <param name="destination">The destination <see cref="Stream" /> to copy to</param>
    /// <param name="progress">An <see cref="IProgress{T}" /> implementation for reporting progress</param>
    /// <returns>A task representing the operation</returns>
    public static Task CopyToAsync(this Stream source, long sourceLength, Stream destination, IProgress<(long, long)> progress)
    {
        return CopyToAsync(source, sourceLength, destination, 0, progress, default);
    }

    /// <summary>
    ///     Copies a stream to another stream
    /// </summary>
    /// <param name="source">The source <see cref="Stream" /> to copy from</param>
    /// <param name="destination">The destination <see cref="Stream" /> to copy to</param>
    /// <param name="progress">An <see cref="IProgress{T}" /> implementation for reporting progress</param>
    /// <returns>A task representing the operation</returns>
    public static Task CopyToAsync(this Stream source, Stream destination, IProgress<(long, long)> progress)
    {
        return CopyToAsync(source, 0L, destination, 0, progress, default);
    }
}
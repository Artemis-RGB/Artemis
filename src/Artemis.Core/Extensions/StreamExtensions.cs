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
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Artemis.Core
{
    internal static class StreamExtensions
    {
        private const int DefaultBufferSize = 81920;

        /// <summary>
        ///     Copys a stream to another stream
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
            if (0 == bufferSize)
                bufferSize = DefaultBufferSize;
            byte[]? buffer = new byte[bufferSize];
            if (0 > sourceLength && source.CanSeek)
                sourceLength = source.Length - source.Position;
            long totalBytesCopied = 0L;
            if (null != progress)
                progress.Report((totalBytesCopied, sourceLength));
            int bytesRead = -1;
            while (0 != bytesRead && !cancellationToken.IsCancellationRequested)
            {
                bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                if (0 == bytesRead || cancellationToken.IsCancellationRequested)
                    break;
                await destination.WriteAsync(buffer, 0, buffer.Length, cancellationToken);
                totalBytesCopied += bytesRead;
                progress?.Report((totalBytesCopied, sourceLength));
            }

            if (0 < totalBytesCopied)
                progress?.Report((totalBytesCopied, sourceLength));
            cancellationToken.ThrowIfCancellationRequested();
        }

        /// <summary>
        ///     Copys a stream to another stream
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
        ///     Copys a stream to another stream
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
        ///     Copys a stream to another stream
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
        ///     Copys a stream to another stream
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
}
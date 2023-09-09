// Heavily based on:
// SkyClip
// - ProgressableStreamContent.cs
// --------------------------------------------------------------------
// Author: Jeff Hansen <jeff@jeffijoe.com>
// Copyright (C) Jeff Hansen 2015. All rights reserved.

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Artemis.UI.Shared.Utilities;

/// <summary>
///     Provides HTTP content based on a stream with support for IProgress.
/// </summary>
public class ProgressableStreamContent : StreamContent
{
    private const int DEFAULT_BUFFER_SIZE = 4096;

    private readonly int _bufferSize;
    private readonly IProgress<StreamProgress> _progress;
    private readonly Stream _streamToWrite;
    private bool _contentConsumed;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ProgressableStreamContent" /> class.
    /// </summary>
    /// <param name="streamToWrite">The stream to write.</param>
    /// <param name="progress">The downloader.</param>
    public ProgressableStreamContent(Stream streamToWrite, IProgress<StreamProgress> progress) : this(streamToWrite, DEFAULT_BUFFER_SIZE, progress)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ProgressableStreamContent" /> class.
    /// </summary>
    /// <param name="streamToWrite">The stream to write.</param>
    /// <param name="bufferSize">The buffer size.</param>
    /// <param name="progress">The downloader.</param>
    public ProgressableStreamContent(Stream streamToWrite, int bufferSize, IProgress<StreamProgress> progress) : base(streamToWrite, bufferSize)
    {
        if (bufferSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(bufferSize));

        _streamToWrite = streamToWrite;
        _bufferSize = bufferSize;
        _progress = progress;
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _streamToWrite.Dispose();

        base.Dispose(disposing);
    }

    /// <inheritdoc />
    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
    {
        await SerializeToStreamAsync(stream, context, CancellationToken.None);
    }

    /// <inheritdoc />
    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context, CancellationToken cancellationToken)
    {
        PrepareContent();

        byte[] buffer = new byte[_bufferSize];
        long size = _streamToWrite.Length;
        int uploaded = 0;

        await using (_streamToWrite)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                int length = await _streamToWrite.ReadAsync(buffer, cancellationToken);
                if (length <= 0)
                    break;

                uploaded += length;
                _progress.Report(new StreamProgress(uploaded, size));
                await stream.WriteAsync(buffer, 0, length, cancellationToken);
            }
        }
    }

    /// <inheritdoc />
    protected override bool TryComputeLength(out long length)
    {
        length = _streamToWrite.Length;
        return true;
    }

    /// <summary>
    ///     Prepares the content.
    /// </summary>
    /// <exception cref="System.InvalidOperationException">The stream has already been read.</exception>
    private void PrepareContent()
    {
        if (_contentConsumed)
        {
            // If the content needs to be written to a target stream a 2nd time, then the stream must support
            // seeking (e.g. a FileStream), otherwise the stream can't be copied a second time to a target 
            // stream (e.g. a NetworkStream).
            if (_streamToWrite.CanSeek)
                _streamToWrite.Position = 0;
            else
                throw new InvalidOperationException("The stream has already been read.");
        }

        _contentConsumed = true;
    }
}
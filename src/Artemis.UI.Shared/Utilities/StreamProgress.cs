// Heavily based on:
// SkyClip
// - UploadProgress.cs
// --------------------------------------------------------------------
// Author: Jeff Hansen <jeff@jeffijoe.com>
// Copyright (C) Jeff Hansen 2015. All rights reserved.

namespace Artemis.UI.Shared.Utilities;

/// <summary>
///     The upload progress.
/// </summary>
public class StreamProgress
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="StreamProgress" /> class.
    /// </summary>
    /// <param name="bytesTransfered">
    ///     The bytes transfered.
    /// </param>
    /// <param name="totalBytes">
    ///     The total bytes.
    /// </param>
    public StreamProgress(long bytesTransfered, long? totalBytes)
    {
        BytesTransfered = bytesTransfered;
        TotalBytes = totalBytes;
        if (totalBytes.HasValue)
            ProgressPercentage = (int) ((float) bytesTransfered / totalBytes.Value * 100);
    }

    /// <summary>
    ///     Returns a <see cref="System.String" /> that represents this instance.
    /// </summary>
    /// <returns>
    ///     A <see cref="System.String" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        return string.Format("{0}% ({1} / {2})", ProgressPercentage, BytesTransfered, TotalBytes);
    }

    /// <summary>
    ///     Gets the bytes transfered.
    /// </summary>
    public long BytesTransfered { get; }

    /// <summary>
    ///     Gets the progress percentage.
    /// </summary>
    public int ProgressPercentage { get; }

    /// <summary>
    ///     Gets the total bytes.
    /// </summary>
    public long? TotalBytes { get; }
}
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a plugin prerequisite action that downloads a file
    /// </summary>
    public class DownloadFileAction : PluginPrerequisiteAction
    {
        /// <summary>
        ///     Creates a new instance of a copy folder action
        /// </summary>
        /// <param name="name">The name of the action</param>
        /// <param name="url">The source URL to download</param>
        /// <param name="fileName">The target file to save as (will be created if needed)</param>
        public DownloadFileAction(string name, string url, string fileName) : base(name)
        {
            Url = url ?? throw new ArgumentNullException(nameof(url));
            FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));

            ShowProgressBar = true;
        }

        /// <summary>
        ///     Creates a new instance of a copy folder action
        /// </summary>
        /// <param name="name">The name of the action</param>
        /// <param name="urlFunction">A function returning the URL to download</param>
        /// <param name="fileName">The target file to save as (will be created if needed)</param>
        public DownloadFileAction(string name, Func<Task<string>> urlFunction, string fileName) : base(name)
        {
            UrlFunction = urlFunction ?? throw new ArgumentNullException(nameof(urlFunction));
            FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));

            ShowProgressBar = true;
        }

        /// <summary>
        ///     Gets the source URL to download
        /// </summary>
        public string? Url { get; }

        /// <summary>
        ///     Gets the function returning the URL to download
        /// </summary>
        public Func<Task<string>>? UrlFunction { get; }

        /// <summary>
        ///     Gets the target file to save as (will be created if needed)
        /// </summary>
        public string FileName { get; }

        /// <inheritdoc />
        public override async Task Execute(CancellationToken cancellationToken)
        {
            using HttpClient client = new();
            await using FileStream destinationStream = new(FileName, FileMode.OpenOrCreate);
            string? url = Url;
            if (url is null)
            {
                Status = "Retrieving download URL";
                url = await UrlFunction!();
            }

            void ProgressOnProgressReported(object? sender, EventArgs e)
            {
                if (Progress.ProgressPerSecond != 0)
                    Status = $"Downloading {url} - {Progress.ProgressPerSecond.Bytes().Humanize("#.##")}/sec";
                else
                    Status = $"Downloading {url}";
            }

            Progress.ProgressReported += ProgressOnProgressReported;

            // Get the http headers first to examine the content length
            using HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            await using Stream download = await response.Content.ReadAsStreamAsync(cancellationToken);
            long? contentLength = response.Content.Headers.ContentLength;

            // Ignore progress reporting when no progress reporter was 
            // passed or when the content length is unknown
            if (!contentLength.HasValue)
            {
                ProgressIndeterminate = true;
                await download.CopyToAsync(destinationStream, Progress, cancellationToken);
                ProgressIndeterminate = false;
            }
            else
            {
                ProgressIndeterminate = false;
                await download.CopyToAsync(contentLength.Value, destinationStream, Progress, cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();
            
            Progress.ProgressReported -= ProgressOnProgressReported;
            Progress.Report((1, 1));
            Status = "Finished downloading";
        }
    }
}
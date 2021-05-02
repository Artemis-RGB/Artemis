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
        /// <param name="source">The source URL to download</param>
        /// <param name="target">The target file to save as (will be created if needed)</param>
        public DownloadFileAction(string name, string source, string target) : base(name)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Target = target ?? throw new ArgumentNullException(nameof(target));

            ShowProgressBar = true;
        }

        /// <summary>
        ///     Gets the source URL to download
        /// </summary>
        public string Source { get; }

        /// <summary>
        ///     Gets the target file to save as (will be created if needed)
        /// </summary>
        public string Target { get; }

        /// <inheritdoc />
        public override async Task Execute(CancellationToken cancellationToken)
        {
            using HttpClient client = new();
            await using FileStream destinationStream = File.Create(Target);

            void ProgressOnProgressReported(object? sender, EventArgs e)
            {
                if (Progress.ProgressPerSecond != 0)
                    Status = $"Downloading {Target} - {Progress.ProgressPerSecond.Bytes().Humanize("#.##")}/sec";
                else
                    Status = $"Downloading {Target}";
            }

            Progress.ProgressReported += ProgressOnProgressReported;

            // Get the http headers first to examine the content length
            using HttpResponseMessage response = await client.GetAsync(Target, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            await using Stream download = await response.Content.ReadAsStreamAsync(cancellationToken);
            long? contentLength = response.Content.Headers.ContentLength;

            // Ignore progress reporting when no progress reporter was 
            // passed or when the content length is unknown
            if (!contentLength.HasValue)
            {
                ProgressIndeterminate = true;
                await download.CopyToAsync(destinationStream, Progress, cancellationToken);
            }
            else
            {
                ProgressIndeterminate = false;
                await download.CopyToAsync(contentLength.Value, destinationStream, Progress, cancellationToken);
            }

            Progress.ProgressReported -= ProgressOnProgressReported;

            Progress.Report((1, 1));
            Status = "Finished downloading";
        }
    }
}
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.UI.Exceptions;
using Artemis.UI.Extensions;
using Artemis.UI.Shared.Extensions;
using Artemis.UI.Shared.Utilities;
using Artemis.WebClient.Updating;
using Octodiff.Core;
using Octodiff.Diagnostics;
using Serilog;
using StrawberryShake;

namespace Artemis.UI.Services.Updating;

/// <summary>
///     Represents the installation process of a release
/// </summary>
public class ReleaseInstaller : CorePropertyChanged
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private readonly Guid _releaseId;
    private readonly Platform _updatePlatform;
    private readonly IUpdatingClient _updatingClient;
    private readonly Progress<float> _progress = new();

    private IGetReleaseById_PublishedRelease _release = null!;
    private IGetReleaseById_PublishedRelease_Artifacts _artifact = null!;

    private Progress<StreamProgress> _stepProgress = new();
    private string _status = string.Empty;
    private float _floatProgress;

    public ReleaseInstaller(Guid releaseId, ILogger logger, IUpdatingClient updatingClient, HttpClient httpClient)
    {
        _releaseId = releaseId;
        _logger = logger;
        _updatingClient = updatingClient;
        _httpClient = httpClient;

        if (OperatingSystem.IsWindows())
            _updatePlatform = Platform.Windows;
        else if (OperatingSystem.IsLinux())
            _updatePlatform = Platform.Linux;
        else if (OperatingSystem.IsMacOS())
            _updatePlatform = Platform.Osx;
        else
            throw new PlatformNotSupportedException("Cannot auto update on the current platform");

        _progress.ProgressChanged += (_, f) => Progress = f;
    }

    public string Status
    {
        get => _status;
        private set => SetAndNotify(ref _status, value);
    }

    public float Progress
    {
        get => _floatProgress;
        set => SetAndNotify(ref _floatProgress, value);
    }

    public async Task InstallAsync(CancellationToken cancellationToken)
    {
        _stepProgress = new Progress<StreamProgress>();

        Status = "Retrieving details";
        _logger.Information("Retrieving details for release {ReleaseId}", _releaseId);
        IOperationResult<IGetReleaseByIdResult> result = await _updatingClient.GetReleaseById.ExecuteAsync(_releaseId, cancellationToken);
        result.EnsureNoErrors();

        _release = result.Data?.PublishedRelease!;
        if (_release == null)
            throw new Exception($"Could not find release with ID {_releaseId}");

        _artifact = _release.Artifacts.FirstOrDefault(a => a.Platform == _updatePlatform)!;
        if (_artifact == null)
            throw new Exception("Found the release but it has no artifact for the current platform");

        ((IProgress<float>) _progress).Report(10);

        // Determine whether the last update matches our local version, then we can download the delta
        if (_release.PreviousRelease != null && File.Exists(Path.Combine(Constants.UpdatingFolder, $"{_release.PreviousRelease.Version}.zip")) && _artifact.DeltaFileInfo.DownloadSize != 0)
            await DownloadDelta(Path.Combine(Constants.UpdatingFolder, $"{_release.PreviousRelease.Version}.zip"), cancellationToken);
        else
            await Download(cancellationToken);
    }

    private async Task DownloadDelta(string previousRelease, CancellationToken cancellationToken)
    {
        // 10 - 50%
        _stepProgress.ProgressChanged += StepProgressOnProgressChanged;
        void StepProgressOnProgressChanged(object? sender, StreamProgress e) => ((IProgress<float>) _progress).Report(10f + e.ProgressPercentage * 0.4f);

        Status = "Downloading...";
        await using MemoryStream stream = new();
        await _httpClient.DownloadDataAsync($"https://updating.artemis-rgb.com/api/artifacts/{_artifact.ArtifactId}/delta", stream, _stepProgress, cancellationToken);

        _stepProgress.ProgressChanged -= StepProgressOnProgressChanged;
        await PatchDelta(stream, previousRelease, cancellationToken);
    }

    private async Task PatchDelta(Stream deltaStream, string previousRelease, CancellationToken cancellationToken)
    {
        // 50 - 60%
        _stepProgress.ProgressChanged += StepProgressOnProgressChanged;
        void StepProgressOnProgressChanged(object? sender, StreamProgress e) => ((IProgress<float>) _progress).Report(50f + e.ProgressPercentage * 0.1f);

        Status = "Patching...";
        await using FileStream newFileStream = new(Path.Combine(Constants.UpdatingFolder, $"{_release.Version}.zip"), FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
        await using (FileStream baseStream = File.OpenRead(previousRelease))
        {
            deltaStream.Seek(0, SeekOrigin.Begin);
            DeltaApplier deltaApplier = new() {SkipHashCheck = true};
            // Patching is not async and so fast that it's not worth adding a progress reporter 
            deltaApplier.Apply(baseStream, new BinaryDeltaReader(deltaStream, new NullProgressReporter()), newFileStream);
            cancellationToken.ThrowIfCancellationRequested();
        }

        // The previous release is no longer required now that the latest has been downloaded
        File.Delete(previousRelease);

        _stepProgress.ProgressChanged -= StepProgressOnProgressChanged;

        await ValidateArchive(newFileStream, cancellationToken);
        await Extract(newFileStream, cancellationToken);
    }

    private async Task Download(CancellationToken cancellationToken)
    {
        // 10 - 60%
        _stepProgress.ProgressChanged += StepProgressOnProgressChanged;
        void StepProgressOnProgressChanged(object? sender, StreamProgress e) => ((IProgress<float>) _progress).Report(10f + e.ProgressPercentage * 0.5f);

        Status = "Downloading...";
        await using FileStream stream = new(Path.Combine(Constants.UpdatingFolder, $"{_release.Version}.zip"), FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
        await _httpClient.DownloadDataAsync($"https://updating.artemis-rgb.com/api/artifacts/{_artifact.ArtifactId}", stream, _stepProgress, cancellationToken);

        _stepProgress.ProgressChanged -= StepProgressOnProgressChanged;

        await ValidateArchive(stream, cancellationToken);
        await Extract(stream, cancellationToken);
    }

    private async Task Extract(Stream archiveStream, CancellationToken cancellationToken)
    {
        // 60 - 100%
        _stepProgress.ProgressChanged += StepProgressOnProgressChanged;
        void StepProgressOnProgressChanged(object? sender, StreamProgress e) => ((IProgress<float>) _progress).Report(60f + e.ProgressPercentage * 0.4f);

        Status = "Extracting...";
        // Ensure the directory is empty
        string extractDirectory = Path.Combine(Constants.UpdatingFolder, "pending");
        if (Directory.Exists(extractDirectory))
            Directory.Delete(extractDirectory, true);
        Directory.CreateDirectory(extractDirectory);

        await Task.Run(() =>
        {
            archiveStream.Seek(0, SeekOrigin.Begin);
            using ZipArchive archive = new(archiveStream);
            archive.ExtractToDirectory(extractDirectory, false, _stepProgress, cancellationToken);
        }, cancellationToken);

        ((IProgress<float>) _progress).Report(100);
        _stepProgress.ProgressChanged -= StepProgressOnProgressChanged;
    }

    private async Task ValidateArchive(Stream archiveStream, CancellationToken cancellationToken)
    {
        using MD5 md5 = MD5.Create();
        archiveStream.Seek(0, SeekOrigin.Begin);
        string hash = BitConverter.ToString(await md5.ComputeHashAsync(archiveStream, cancellationToken)).Replace("-", "");
        if (hash != _artifact.FileInfo.Md5Hash)
            throw new ArtemisUIException($"Update file hash mismatch, expected \"{_artifact.FileInfo.Md5Hash}\" but got \"{hash}\"");
    }
}
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.UI.Extensions;
using Artemis.WebClient.Updating;
using Octodiff.Core;
using Octodiff.Diagnostics;
using Serilog;
using StrawberryShake;

namespace Artemis.UI.Services.Updating;

/// <summary>
///     Represents the installation process of a release
/// </summary>
public class ReleaseInstaller
{
    private readonly string _dataFolder;
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private readonly string _releaseId;
    private readonly Platform _updatePlatform;
    private readonly IUpdatingClient _updatingClient;

    public ReleaseInstaller(string releaseId, ILogger logger, IUpdatingClient updatingClient, HttpClient httpClient)
    {
        _releaseId = releaseId;
        _logger = logger;
        _updatingClient = updatingClient;
        _httpClient = httpClient;
        _dataFolder = Path.Combine(Constants.DataFolder, "updating");

        if (OperatingSystem.IsWindows())
            _updatePlatform = Platform.Windows;
        else if (OperatingSystem.IsLinux())
            _updatePlatform = Platform.Linux;
        else if (OperatingSystem.IsMacOS())
            _updatePlatform = Platform.Osx;
        else
            throw new PlatformNotSupportedException("Cannot auto update on the current platform");

        if (!Directory.Exists(_dataFolder))
            Directory.CreateDirectory(_dataFolder);
    }


    public Progress<float> OverallProgress { get; } = new();
    public Progress<float> StepProgress { get; } = new();

    public async Task InstallAsync(CancellationToken cancellationToken)
    {
        ((IProgress<float>) OverallProgress).Report(0);

        _logger.Information("Retrieving details for release {ReleaseId}", _releaseId);
        IOperationResult<IGetReleaseByIdResult> result = await _updatingClient.GetReleaseById.ExecuteAsync(_releaseId, cancellationToken);
        result.EnsureNoErrors();

        IGetReleaseById_Release? release = result.Data?.Release;
        if (release == null)
            throw new Exception($"Could not find release with ID {_releaseId}");

        IGetReleaseById_Release_Artifacts? artifact = release.Artifacts.FirstOrDefault(a => a.Platform == _updatePlatform);
        if (artifact == null)
            throw new Exception("Found the release but it has no artifact for the current platform");

        ((IProgress<float>) OverallProgress).Report(10);

        // Determine whether the last update matches our local version, then we can download the delta
        if (release.PreviousRelease != null && File.Exists(Path.Combine(_dataFolder, $"{release.PreviousRelease}.zip")) && artifact.DeltaFileInfo.DownloadSize != 0)
            await DownloadDelta(artifact, Path.Combine(_dataFolder, $"{release.PreviousRelease}.zip"), cancellationToken);
        else
            await Download(artifact, cancellationToken);
    }

    private async Task DownloadDelta(IGetReleaseById_Release_Artifacts artifact, string previousRelease, CancellationToken cancellationToken)
    {
        await using MemoryStream stream = new();
        await _httpClient.DownloadDataAsync($"https://updating.artemis-rgb.com/api/artifacts/download/{artifact.ArtifactId}/delta", stream, StepProgress, cancellationToken);

        ((IProgress<float>) OverallProgress).Report(33);

        await PatchDelta(stream, previousRelease, cancellationToken);
    }

    private async Task PatchDelta(Stream deltaStream, string previousRelease, CancellationToken cancellationToken)
    {
        await using FileStream baseStream = File.OpenRead(previousRelease);
        await using FileStream newFileStream = new(Path.Combine(_dataFolder, $"{_releaseId}.zip"), FileMode.Create, FileAccess.ReadWrite, FileShare.Read);

        deltaStream.Seek(0, SeekOrigin.Begin);

        await Task.Run(() =>
        {
            DeltaApplier deltaApplier = new();
            deltaApplier.Apply(baseStream, new BinaryDeltaReader(deltaStream, new DeltaApplierProgressReporter(StepProgress)), newFileStream);
        });
        cancellationToken.ThrowIfCancellationRequested();

        ((IProgress<float>) OverallProgress).Report(66);
        await Extract(newFileStream, cancellationToken);
    }

    private async Task Download(IGetReleaseById_Release_Artifacts artifact, CancellationToken cancellationToken)
    {
        await using MemoryStream stream = new();
        await _httpClient.DownloadDataAsync($"https://updating.artemis-rgb.com/api/artifacts/download/{artifact.ArtifactId}", stream, StepProgress, cancellationToken);

        ((IProgress<float>) OverallProgress).Report(50);
        await Extract(stream, cancellationToken);
    }

    private async Task Extract(Stream archiveStream, CancellationToken cancellationToken)
    {
        // Ensure the directory is empty
        string extractDirectory = Path.Combine(_dataFolder, "pending");
        if (Directory.Exists(extractDirectory))
            Directory.Delete(extractDirectory, true);
        Directory.CreateDirectory(extractDirectory);

      
      
        await Task.Run(() =>
        {
            archiveStream.Seek(0, SeekOrigin.Begin);
            using ZipArchive archive = new(archiveStream);
            archive.ExtractToDirectory(extractDirectory, false, StepProgress, cancellationToken);
        });
        
        ((IProgress<float>) OverallProgress).Report(100);
    }
}

internal class DeltaApplierProgressReporter : IProgressReporter
{
    private readonly IProgress<float> _stepProgress;

    public DeltaApplierProgressReporter(IProgress<float> stepProgress)
    {
        _stepProgress = stepProgress;
    }

    public void ReportProgress(string operation, long currentPosition, long total)
    {
        _stepProgress.Report(currentPosition / total * 100);
    }
}
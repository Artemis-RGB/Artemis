using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.UI.Exceptions;
using Artemis.UI.Shared.Providers;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.MainWindow;
using Artemis.UI.Windows.Models;
using Artemis.UI.Windows.Screens.Update;
using Avalonia.Threading;
using Flurl;
using Flurl.Http;
using Microsoft.Toolkit.Uwp.Notifications;
using Serilog;
using File = System.IO.File;

namespace Artemis.UI.Windows.Providers;

public class UpdateProvider : IUpdateProvider, IDisposable
{
    private const string ApiUrl = "https://dev.azure.com/artemis-rgb/Artemis/_apis/";
    private const string InstallerUrl = "https://builds.artemis-rgb.com/binaries/Artemis.Installer.exe";

    private readonly ILogger _logger;
    private readonly IMainWindowService _mainWindowService;
    private readonly IWindowService _windowService;

    public UpdateProvider(ILogger logger, IWindowService windowService, IMainWindowService mainWindowService)
    {
        _logger = logger;
        _windowService = windowService;
        _mainWindowService = mainWindowService;

        ToastNotificationManagerCompat.OnActivated += ToastNotificationManagerCompatOnOnActivated;
    }

    public async Task<DevOpsBuild?> GetBuildInfo(int buildDefinition, string? buildNumber = null)
    {
        Url request = ApiUrl.AppendPathSegments("build", "builds")
            .SetQueryParam("definitions", buildDefinition)
            .SetQueryParam("resultFilter", "succeeded")
            .SetQueryParam("$top", 1)
            .SetQueryParam("api-version", "6.1-preview.6");

        if (buildNumber != null)
            request = request.SetQueryParam("buildNumber", buildNumber);

        try
        {
            DevOpsBuilds result = await request.GetJsonAsync<DevOpsBuilds>();
            try
            {
                return result.Builds.FirstOrDefault();
            }
            catch (Exception e)
            {
                _logger.Warning(e, "GetBuildInfo: Failed to retrieve build info JSON");
                throw;
            }
        }
        catch (FlurlHttpException e)
        {
            _logger.Warning("GetBuildInfo: Getting build info, request returned {StatusCode}", e.StatusCode);
            throw;
        }
    }

    public async Task<GitHubDifference> GetBuildDifferences(DevOpsBuild a, DevOpsBuild b)
    {
        return await "https://api.github.com"
            .AppendPathSegments("repos", "Artemis-RGB", "Artemis", "compare")
            .AppendPathSegment(a.SourceVersion + "..." + b.SourceVersion)
            .WithHeader("User-Agent", "Artemis 2")
            .WithHeader("Accept", "application/vnd.github.v3+json")
            .GetJsonAsync<GitHubDifference>();
    }

    private async void ToastNotificationManagerCompatOnOnActivated(ToastNotificationActivatedEventArgsCompat e)
    {
        ToastArguments args = ToastArguments.Parse(e.Argument);
        string channel = args.Get("channel");
        string action = "view-changes";
        if (args.Contains("action"))
            action = args.Get("action");

        if (action == "install")
            await RunInstaller(channel, true);
        else if (action == "view-changes")
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                _mainWindowService.OpenMainWindow();
                await OfferUpdate(channel, true);
            });
    }

    private async Task RunInstaller(string channel, bool silent)
    {
        _logger.Information("ApplyUpdate: Applying update");

        // Ensure the installer is up-to-date, get installer build info
        DevOpsBuild? buildInfo = await GetBuildInfo(6);
        string installerPath = Path.Combine(Constants.DataFolder, "installer", "Artemis.Installer.exe");

        // Always update installer if it is missing ^^
        if (!File.Exists(installerPath))
        {
            await UpdateInstaller();
        }
        // Compare the creation date of the installer with the build date and update if needed
        else
        {
            if (buildInfo != null && File.GetLastWriteTime(installerPath) < buildInfo.FinishTime)
                await UpdateInstaller();
        }

        _logger.Information("ApplyUpdate: Running installer at {InstallerPath}", installerPath);

        try
        {
            Process.Start(new ProcessStartInfo(installerPath, "-autoupdate")
            {
                UseShellExecute = true,
                Verb = "runas"
            });
        }
        catch (Win32Exception e)
        {
            if (e.NativeErrorCode == 0x4c7)
                _logger.Warning("ApplyUpdate: Operation was cancelled, user likely clicked No in UAC dialog");
            else
                throw;
        }
    }

    private async Task UpdateInstaller()
    {
        string installerDirectory = Path.Combine(Constants.DataFolder, "installer");
        string installerPath = Path.Combine(installerDirectory, "Artemis.Installer.exe");

        _logger.Information("UpdateInstaller: Downloading installer from {DownloadUrl}", InstallerUrl);
        using HttpClient client = new();
        HttpResponseMessage httpResponseMessage = await client.GetAsync(InstallerUrl);
        if (!httpResponseMessage.IsSuccessStatusCode)
            throw new ArtemisUIException($"Failed to download installer, status code {httpResponseMessage.StatusCode}");

        _logger.Information("UpdateInstaller: Writing installer file to {InstallerPath}", installerPath);
        if (File.Exists(installerPath))
            File.Delete(installerPath);

        Core.Utilities.CreateAccessibleDirectory(installerDirectory);
        await using FileStream fs = new(installerPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await httpResponseMessage.Content.CopyToAsync(fs);
    }

    private void ShowDesktopNotification(string channel)
    {
        new ToastContentBuilder()
            .AddArgument("channel", channel)
            .AddText("An update is available")
            .AddButton(new ToastButton().SetContent("Install").AddArgument("action", "install").SetBackgroundActivation())
            .AddButton(new ToastButton().SetContent("View changes").AddArgument("action", "view-changes"))
            .Show();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        ToastNotificationManagerCompat.OnActivated -= ToastNotificationManagerCompatOnOnActivated;
        ToastNotificationManagerCompat.Uninstall();
    }

    /// <inheritdoc />
    public async Task<bool> CheckForUpdate(string channel)
    {
        DevOpsBuild? buildInfo = await GetBuildInfo(1);
        if (buildInfo == null)
            return false;

        double buildNumber = double.Parse(buildInfo.BuildNumber, CultureInfo.InvariantCulture);
        string buildNumberDisplay = buildNumber.ToString(CultureInfo.InvariantCulture);
        _logger.Information("Latest build is {BuildNumber}, we're running {LocalBuildNumber}", buildNumberDisplay, Constants.BuildInfo.BuildNumberDisplay);

        return buildNumber > Constants.BuildInfo.BuildNumber;
    }

    /// <inheritdoc />
    public async Task ApplyUpdate(string channel, bool silent)
    {
        await RunInstaller(channel, silent);
    }

    /// <inheritdoc />
    public async Task OfferUpdate(string channel, bool windowOpen)
    {
        if (windowOpen)
        {
            bool update = await _windowService.ShowDialogAsync<UpdateDialogViewModel, bool>(("channel", channel));
            if (update)
                await RunInstaller(channel, false);
        }
        else
        {
            ShowDesktopNotification(channel);
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using Windows.UI.Notifications;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Exceptions;
using Artemis.UI.Screens.Settings.Dialogs;
using Artemis.UI.Services.Models.UpdateService;
using Artemis.UI.Shared.Services;
using Flurl;
using Flurl.Http;
using Microsoft.Toolkit.Uwp.Notifications;
using Serilog;
using File = System.IO.File;

namespace Artemis.UI.Services
{
    public class UpdateService : IUpdateService
    {
        private const double UpdateCheckInterval = 3600000; // once per hour
        private const string ApiUrl = "https://dev.azure.com/artemis-rgb/Artemis/_apis/";

        private readonly PluginSetting<bool> _checkForUpdates;
        private readonly IDialogService _dialogService;
        private readonly ILogger _logger;
        private readonly IWindowService _windowService;

        public UpdateService(ILogger logger, ISettingsService settingsService, IDialogService dialogService, IWindowService windowService)
        {
            _logger = logger;
            _dialogService = dialogService;
            _windowService = windowService;
            _windowService.MainWindowOpened += WindowServiceOnMainWindowOpened;

            _checkForUpdates = settingsService.GetSetting("UI.CheckForUpdates", true);
            _checkForUpdates.SettingChanged += CheckForUpdatesOnSettingChanged;

            Timer timer = new(UpdateCheckInterval);
            timer.Elapsed += async (_, _) => await AutoUpdate();
            timer.Start();
        }

        private async Task OfferUpdate(DevOpsBuild buildInfo)
        {
            object result = await _dialogService.ShowDialog<UpdateDialogViewModel>(new Dictionary<string, object> {{"buildInfo", buildInfo}});
            if (result is bool resultBool && resultBool == false)
                SuspendAutoUpdate = true;
        }

        private async Task UpdateInstaller()
        {
            string downloadUrl = "https://builds.artemis-rgb.com/binaries/Artemis.Installer.exe";
            string installerDirectory = Path.Combine(Constants.DataFolder, "installer");
            string installerPath = Path.Combine(installerDirectory, "Artemis.Installer.exe");

            _logger.Information("UpdateInstaller: Downloading installer from {downloadUrl}", downloadUrl);
            using HttpClient client = new();
            HttpResponseMessage httpResponseMessage = await client.GetAsync(downloadUrl);
            if (!httpResponseMessage.IsSuccessStatusCode)
                throw new ArtemisUIException($"Failed to download installer, status code {httpResponseMessage.StatusCode}");

            _logger.Information("UpdateInstaller: Writing installer file to {installerPath}", installerPath);
            if (File.Exists(installerPath))
                File.Delete(installerPath);

            Core.Utilities.CreateAccessibleDirectory(installerDirectory);
            await using FileStream fs = new(installerPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await httpResponseMessage.Content.CopyToAsync(fs);
        }

        private async void TOnActivated(ToastNotification sender, object args)
        {
            if (args is not ToastActivatedEventArgs toastEventArgs)
                return;

            if (toastEventArgs.Arguments == "update")
                await ApplyUpdate();
            else if (toastEventArgs.Arguments == "later")
                SuspendAutoUpdate = true;
        }

        private async void CheckForUpdatesOnSettingChanged(object sender, EventArgs e)
        {
            // Run an auto-update as soon as the setting gets changed to enabled
            if (_checkForUpdates.Value)
                await AutoUpdate();
        }

        private async void WindowServiceOnMainWindowOpened(object sender, EventArgs e)
        {
            await AutoUpdate();
        }

        public bool SuspendAutoUpdate { get; set; }

        public async Task<bool> AutoUpdate()
        {
            if (Constants.BuildInfo.IsLocalBuild)
                return false;
            if (!_checkForUpdates.Value || SuspendAutoUpdate)
                return false;

            try
            {
                return await OfferUpdateIfFound();
            }
            catch (Exception e)
            {
                _logger.Warning(e, "Auto update failed");
                return false;
            }
        }

        public async Task<bool> OfferUpdateIfFound()
        {
            _logger.Information("Checking for updates");

            DevOpsBuild buildInfo = await GetBuildInfo(1);
            double buildNumber = double.Parse(buildInfo.BuildNumber, CultureInfo.InvariantCulture);

            string buildNumberDisplay = buildNumber.ToString(CultureInfo.InvariantCulture);
            _logger.Information("Latest build is {buildNumber}, we're running {localBuildNumber}", buildNumberDisplay, Constants.BuildInfo.BuildNumberDisplay);

            if (buildNumber <= Constants.BuildInfo.BuildNumber)
                return false;

            if (_windowService.IsMainWindowOpen)
                await OfferUpdate(buildInfo);
            else
                // If auto-install is disabled and the window is closed, best we can do is notify the user and stop.
                new ToastContentBuilder()
                    .AddText("New version available", AdaptiveTextStyle.Header)
                    .AddText($"Build {buildNumberDisplay} is available, currently on {Constants.BuildInfo.BuildNumberDisplay}.")
                    .AddButton("Update", ToastActivationType.Background, "update")
                    .AddButton("Later", ToastActivationType.Background, "later")
                    .Show(t => t.Activated += TOnActivated);

            return true;
        }

        public async Task<bool> IsUpdateAvailable()
        {
            DevOpsBuild buildInfo = await GetBuildInfo(1);
            double buildNumber = double.Parse(buildInfo.BuildNumber, CultureInfo.InvariantCulture);
            return buildNumber > Constants.BuildInfo.BuildNumber;
        }

        public async Task ApplyUpdate()
        {
            _logger.Information("ApplyUpdate: Applying update");

            // Ensure the installer is up-to-date, get installer build info
            DevOpsBuild buildInfo = await GetBuildInfo(6);
            string installerPath = Path.Combine(Constants.DataFolder, "installer", "Artemis.Installer.exe");

            // Always update installer if it is missing ^^
            if (!File.Exists(installerPath))
                await UpdateInstaller();
            // Compare the creation date of the installer with the build date and update if needed
            else
            {
                if (File.GetLastWriteTime(installerPath) < buildInfo.FinishTime)
                    await UpdateInstaller();
            }

            _logger.Information("ApplyUpdate: Running installer at {installerPath}", installerPath);

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
                    _logger.Warning("ApplyUpdate: Operation was cancelled, user likely clicked No in UAC dialog.");
                else
                    throw;
            }
        }

        public async Task<DevOpsBuild> GetBuildInfo(int buildDefinition, string buildNumber = null)
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
                _logger.Warning("GetBuildInfo: Getting build info, request returned {statusCode}", e.StatusCode);
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
    }

    public interface IUpdateService : IArtemisUIService
    {
        bool SuspendAutoUpdate { get; set; }

        /// <summary>
        ///     If auto-update is enabled this will offer updates if found
        /// </summary>
        Task<bool> AutoUpdate();

        Task<bool> OfferUpdateIfFound();
        Task<bool> IsUpdateAvailable();
        Task<DevOpsBuild> GetBuildInfo(int buildDefinition, string buildNumber = null);
        Task<GitHubDifference> GetBuildDifferences(DevOpsBuild a, DevOpsBuild b);
        Task ApplyUpdate();
    }
}
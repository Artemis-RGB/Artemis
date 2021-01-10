using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Services.Interfaces;
using Artemis.UI.Shared.Services;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json.Linq;
using Serilog;

namespace Artemis.UI.Services
{
    public class UpdateService : IUpdateService
    {
        private const string ApiUrl = "https://dev.azure.com/artemis-rgb/Artemis/_apis/";
        private readonly PluginSetting<bool> _autoInstallUpdates;
        private readonly PluginSetting<bool> _checkForUpdates;
        private readonly ILogger _logger;
        private readonly IDialogService _dialogService;
        private readonly IMessageService _messageService;
        private readonly IWindowService _windowService;

        public UpdateService(ILogger logger, ISettingsService settingsService, IDialogService dialogService, IMessageService messageService, IWindowService windowService)
        {
            _logger = logger;
            _dialogService = dialogService;
            _messageService = messageService;
            _windowService = windowService;
            _windowService.MainWindowOpened += WindowServiceOnMainWindowOpened;

            _checkForUpdates = settingsService.GetSetting("UI.CheckForUpdates", true);
            _autoInstallUpdates = settingsService.GetSetting("UI.AutoInstallUpdates", false);

            _checkForUpdates.SettingChanged += CheckForUpdatesOnSettingChanged;
        }

        public async Task<double> GetLatestBuildNumber()
        {
            // TODO: The URL is hardcoded, that should change in the future
            string latestBuildUrl = ApiUrl + "build/builds?api-version=6.1-preview.6&branchName=refs/heads/master&resultFilter=succeeded&$top=1";
            _logger.Debug("Getting latest build number from {latestBuildUrl}", latestBuildUrl);

            // Make the request
            using HttpClient client = new();
            HttpResponseMessage httpResponseMessage = await client.GetAsync(latestBuildUrl);

            // Ensure it returned correctly
            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                _logger.Warning("Failed to check for updates, request returned {statusCode}", httpResponseMessage.StatusCode);
                return 0;
            }

            // Parse the response
            string response = await httpResponseMessage.Content.ReadAsStringAsync();
            try
            {
                JToken buildNumberToken = JObject.Parse(response).SelectToken("value[0].buildNumber");
                if (buildNumberToken != null)
                    return buildNumberToken.Value<double>();

                _logger.Warning("Failed to find build number at \"value[0].buildNumber\"");
                return 0;
            }
            catch (Exception e)
            {
                _logger.Warning(e, "Failed to retrieve build info JSON");
                return 0;
            }
        }

        public async Task<bool> OfferUpdatesIfFound()
        {
            _logger.Information("Checking for updates");

            double buildNumber = await GetLatestBuildNumber();
            string buildNumberDisplay = buildNumber.ToString(CultureInfo.InvariantCulture);
            _logger.Information("Latest build is {buildNumber}, we're running {localBuildNumber}", buildNumberDisplay, Constants.BuildInfo.BuildNumberDisplay);

            if (buildNumber < Constants.BuildInfo.BuildNumber)
                return false;

            if (_windowService.IsMainWindowOpen)
            {
            }
            else if (_autoInstallUpdates.Value)
            {
                // Lets go
                _messageService.ShowNotification(
                    "Installing new version",
                    $"Build {buildNumberDisplay} is available, currently on {Constants.BuildInfo.BuildNumberDisplay}.",
                    PackIconKind.Update
                );
            }
            else
            {
                // If auto-install is disabled and the window is closed, best we can do is notify the user and stop.
                _messageService.ShowNotification(
                    "New version available",
                    $"Build {buildNumberDisplay} is available, currently on {Constants.BuildInfo.BuildNumberDisplay}.",
                    PackIconKind.Update
                );
            }

            return true;
        }

        public async Task<bool> IsUpdateAvailable()
        {
            double buildNumber = await GetLatestBuildNumber();
            return buildNumber > Constants.BuildInfo.BuildNumber;
        }

        public void ApplyUpdate()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> AutoUpdate()
        {
            if (!_checkForUpdates.Value)
                return false;

            return await OfferUpdatesIfFound();
        }

        #region Event handlers

        private void CheckForUpdatesOnSettingChanged(object sender, EventArgs e)
        {
            // Run an auto-update as soon as the setting gets changed to enabled
            if (_checkForUpdates.Value)
                AutoUpdate();
        }

        private void WindowServiceOnMainWindowOpened(object? sender, EventArgs e)
        {
            _logger.Information("Main window opened!");
        }

        #endregion
    }

    public interface IUpdateService : IArtemisUIService
    {
        Task<bool> OfferUpdatesIfFound();
        Task<bool> IsUpdateAvailable();
        void ApplyUpdate();

        /// <summary>
        ///     If auto-update is enabled this will offer updates if found
        /// </summary>
        Task<bool> AutoUpdate();
    }
}
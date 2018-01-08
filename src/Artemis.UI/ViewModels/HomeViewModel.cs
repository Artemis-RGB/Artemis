using System;
using System.Diagnostics;
using Artemis.Core.Services.Interfaces;
using Artemis.UI.ViewModels.Interfaces;
using Stylet;

namespace Artemis.UI.ViewModels
{
    public class HomeViewModel : Screen, IHomeViewModel
    {
        private readonly IPluginService _pluginService;

        public HomeViewModel(IPluginService pluginService)
        {
            _pluginService = pluginService;

            _pluginService.FinishedLoadedPlugins += PluginServiceOnFinishedLoadedPlugins;
        }

        public string Title => "Home";

        public void OpenUrl(string url)
        {
            // Don't open anything but valid URIs
            if (Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
                Process.Start(url);
        }

        /// <summary>
        ///     Populates the sidebar with plugins when they are finished loading
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void PluginServiceOnFinishedLoadedPlugins(object sender, EventArgs eventArgs)
        {
            throw new NotImplementedException();
        }
    }
}
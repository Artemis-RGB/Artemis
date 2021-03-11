using System.Collections.Generic;
using System.Linq;
using Artemis.Core;
using Artemis.Core.DeviceProviders;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.Settings.Tabs.Plugins;
using Stylet;

namespace Artemis.UI.Screens.StartupWizard.Steps
{
    public class DevicesStepViewModel : Conductor<PluginFeatureViewModel>.Collection.AllActive
    {
        private readonly IPluginManagementService _pluginManagementService;
        private readonly ISettingsVmFactory _settingsVmFactory;

        public DevicesStepViewModel(IPluginManagementService pluginManagementService, ISettingsVmFactory settingsVmFactory)
        {
            _pluginManagementService = pluginManagementService;
            _settingsVmFactory = settingsVmFactory;
        }

        #region Overrides of Screen

        /// <inheritdoc />
        protected override void OnActivate()
        {
            Items.Clear();

            // _pluginManagementService.GetFeaturesOfType<>() will only give us enabled features so lets get all of them this way
            IEnumerable<PluginFeatureInfo> features = _pluginManagementService.GetAllPlugins()
                .SelectMany(p => p.Features.Where(f => typeof(DeviceProvider).IsAssignableFrom(f.FeatureType)))
                .OrderBy(d => d.GetType().Name);
            Items.AddRange(features.Select(d => _settingsVmFactory.CreatePluginFeatureViewModel(d, true)));

            base.OnActivate();
        }

        #endregion
    }
}
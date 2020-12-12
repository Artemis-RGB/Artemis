using System.Collections.Generic;
using System.Linq;
using Artemis.Core.DeviceProviders;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.Settings.Tabs.Plugins;
using Stylet;

namespace Artemis.UI.Screens.SetupWizard.Steps
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
            IEnumerable<DeviceProvider> features = _pluginManagementService.GetAllPlugins()
                .SelectMany(p => p.Features.Where(f => f is DeviceProvider))
                .Cast<DeviceProvider>()
                .OrderBy(d => d.GetType().Name);
            Items.AddRange(features.Select(d => _settingsVmFactory.CreatePluginFeatureViewModel(d)));

            base.OnActivate();
        }

        #endregion
    }
}
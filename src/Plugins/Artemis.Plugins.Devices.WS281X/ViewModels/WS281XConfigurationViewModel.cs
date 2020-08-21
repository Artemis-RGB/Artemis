using System.Collections.Generic;
using Artemis.Core.Plugins;
using Artemis.Core.Plugins.Settings;
using Artemis.Plugins.Devices.WS281X.Settings;

namespace Artemis.Plugins.Devices.WS281X.ViewModels
{
    public class WS281XConfigurationViewModel : PluginConfigurationViewModel
    {
        private PluginSetting<List<DeviceDefinition>> _definitions;

        public WS281XConfigurationViewModel(Plugin plugin, PluginSettings settings) : base(plugin)
        {
            _definitions = settings.GetSetting<List<DeviceDefinition>>("DeviceDefinitions");
        }
    }
}
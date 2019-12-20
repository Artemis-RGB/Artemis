using System;
using Artemis.Core.Plugins.Abstract;
using Stylet;

namespace Artemis.UI.Screens.Settings.Tabs.Plugins
{
    public class PluginSettingsViewModel : PropertyChangedBase
    {
        private readonly Plugin _plugin;

        public PluginSettingsViewModel(Plugin plugin)
        {
            _plugin = plugin;
            IsEnabled = true;
        }

        public string Type => _plugin.GetType().BaseType?.Name ?? _plugin.GetType().Name;
        public string Name => _plugin.PluginInfo.Name;
        public string Description => "N.Y.I.";
        public Version Version => _plugin.PluginInfo.Version;
        public bool IsEnabled { get; set; }
    }
}
using System;
using Artemis.Core;
using Artemis.UI.Avalonia.Shared;

namespace Artemis.UI.Avalonia.Screens.Plugins.ViewModels
{
    public class PluginSettingsWindowViewModel : ActivatableViewModelBase
    {
        public PluginSettingsWindowViewModel(PluginConfigurationViewModel configurationViewModel)
        {
            ConfigurationViewModel = configurationViewModel ?? throw new ArgumentNullException(nameof(configurationViewModel));
            Plugin = configurationViewModel.Plugin;

            DisplayName = $"{Plugin.Info.Name} | Settings";
        }

        public PluginConfigurationViewModel ConfigurationViewModel { get; }
        public Plugin Plugin { get; }
    }
}
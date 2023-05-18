using System;
using Artemis.Core;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.Plugins;

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
    public string LicenseButtonText => Plugin.Info.LicenseName ?? "View license";
}
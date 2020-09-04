using System;
using Artemis.Core;
using MaterialDesignThemes.Wpf;
using Stylet;

namespace Artemis.UI.Screens.Settings.Tabs.Plugins
{
    public class PluginSettingsWindowViewModel : Conductor<PluginConfigurationViewModel>
    {
        public PluginSettingsWindowViewModel(PluginConfigurationViewModel configurationViewModel, PackIconKind icon)
        {
            Icon = icon;
            ActiveItem = configurationViewModel ?? throw new ArgumentNullException(nameof(configurationViewModel));

            ActiveItem.Closed += ActiveItemOnClosed;
        }

        public PackIconKind Icon { get; }

        private void ActiveItemOnClosed(object sender, CloseEventArgs e)
        {
            ActiveItem.Closed -= ActiveItemOnClosed;
            RequestClose();
        }
    }
}
using Artemis.Core.Plugins.Abstract.ViewModels;
using MaterialDesignThemes.Wpf;
using Stylet;

namespace Artemis.UI.Screens.Settings.Tabs.Plugins
{
    public class PluginSettingsWindowViewModel : Conductor<PluginConfigurationViewModel>
    {
        public PackIconKind Icon { get; }

        public PluginSettingsWindowViewModel(PluginConfigurationViewModel configurationViewModel, PackIconKind icon)
        {
            Icon = icon;
            ActiveItem = configurationViewModel;

            ActiveItem.Closed += ActiveItemOnClosed;
        }

        private void ActiveItemOnClosed(object? sender, CloseEventArgs e)
        {
            ActiveItem.Closed -= ActiveItemOnClosed;
            RequestClose();
        }
    }
}
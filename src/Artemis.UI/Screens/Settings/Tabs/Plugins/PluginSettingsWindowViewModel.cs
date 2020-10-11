using System;
using Artemis.Core;
using MaterialDesignThemes.Wpf;
using Stylet;

namespace Artemis.UI.Screens.Settings.Tabs.Plugins
{
    public class PluginSettingsWindowViewModel : Conductor<PluginConfigurationViewModel>
    {
        private readonly PluginConfigurationViewModel _configurationViewModel;

        public PluginSettingsWindowViewModel(PluginConfigurationViewModel configurationViewModel, PackIconKind icon)
        {
            _configurationViewModel = configurationViewModel ?? throw new ArgumentNullException(nameof(configurationViewModel));
            Icon = icon;
        }

        protected override void OnInitialActivate()
        {
            ActiveItem = _configurationViewModel;
            ActiveItem.Closed += ActiveItemOnClosed;

            base.OnInitialActivate();
        }

        public PackIconKind Icon { get; }

        private void ActiveItemOnClosed(object sender, CloseEventArgs e)
        {
            ActiveItem.Closed -= ActiveItemOnClosed;
            RequestClose();
        }
    }
}
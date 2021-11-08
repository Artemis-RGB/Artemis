using System;
using Artemis.Core;

namespace Artemis.UI.Avalonia.Screens.Plugins.ViewModels
{
    public class PluginSettingsWindowViewModel : Conductor<PluginConfigurationViewModel>
    {
        private readonly PluginConfigurationViewModel _configurationViewModel;

        public PluginSettingsWindowViewModel(PluginConfigurationViewModel configurationViewModel)
        {
            _configurationViewModel = configurationViewModel ?? throw new ArgumentNullException(nameof(configurationViewModel));
            Plugin = configurationViewModel.Plugin;
        }

        public Plugin Plugin { get; }

        protected override void OnInitialActivate()
        {
            ActiveItem = _configurationViewModel;
            ActiveItem.Closed += ActiveItemOnClosed;

            base.OnInitialActivate();
        }

        private void ActiveItemOnClosed(object sender, CloseEventArgs e)
        {
            ActiveItem.Closed -= ActiveItemOnClosed;
            RequestClose();
        }
    }
}
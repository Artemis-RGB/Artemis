﻿using System;
using Artemis.UI.Shared;
using Stylet;

namespace Artemis.UI.Screens.Settings.Tabs.Plugins
{
    public class PluginSettingsWindowViewModel : Conductor<PluginConfigurationViewModel>
    {
        private readonly PluginConfigurationViewModel _configurationViewModel;

        public PluginSettingsWindowViewModel(PluginConfigurationViewModel configurationViewModel, object icon)
        {
            _configurationViewModel = configurationViewModel ?? throw new ArgumentNullException(nameof(configurationViewModel));
            Icon = icon;
        }

        public object Icon { get; }

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
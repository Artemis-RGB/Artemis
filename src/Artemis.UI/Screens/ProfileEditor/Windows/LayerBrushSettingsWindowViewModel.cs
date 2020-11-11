using System;
using Artemis.UI.Shared.LayerBrushes;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.Windows
{
    public class LayerBrushSettingsWindowViewModel : Conductor<BrushConfigurationViewModel>
    {
        public LayerBrushSettingsWindowViewModel(BrushConfigurationViewModel configurationViewModel)
        {
            ActiveItem = configurationViewModel ?? throw new ArgumentNullException(nameof(configurationViewModel));
            ActiveItem.Closed += ActiveItemOnClosed;
        }

        private void ActiveItemOnClosed(object sender, CloseEventArgs e)
        {
            ActiveItem.Closed -= ActiveItemOnClosed;
            RequestClose();
        }
    }
}
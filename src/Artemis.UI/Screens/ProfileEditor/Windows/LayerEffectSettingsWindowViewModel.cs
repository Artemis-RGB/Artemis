using System;
using Artemis.Core.LayerEffects;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.Windows
{
    public class LayerEffectSettingsWindowViewModel : Conductor<EffectConfigurationViewModel>
    {
        public LayerEffectSettingsWindowViewModel(EffectConfigurationViewModel configurationViewModel)
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
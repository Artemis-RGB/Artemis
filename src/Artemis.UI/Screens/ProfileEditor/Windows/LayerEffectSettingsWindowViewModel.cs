using System;
using Artemis.UI.Shared.LayerEffects;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.Windows
{
    public class LayerEffectSettingsWindowViewModel : Conductor<EffectConfigurationViewModel>
    {
        private int _height;
        private int _width;

        public LayerEffectSettingsWindowViewModel(EffectConfigurationViewModel configurationViewModel, LayerEffectConfigurationDialog configuration)
        {
            ActiveItem = configurationViewModel ?? throw new ArgumentNullException(nameof(configurationViewModel));
            ActiveItem.Closed += ActiveItemOnClosed;
            Width = configuration.DialogWidth;
            Height = configuration.DialogHeight;
        }

        public int Width
        {
            get => _width;
            set => SetAndNotify(ref _width, value);
        }

        public int Height
        {
            get => _height;
            set => SetAndNotify(ref _height, value);
        }

        private void ActiveItemOnClosed(object sender, CloseEventArgs e)
        {
            ActiveItem.Closed -= ActiveItemOnClosed;
            RequestClose();
        }
    }
}
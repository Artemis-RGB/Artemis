using System;
using Artemis.UI.Shared.LayerBrushes;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.Windows
{
    public class LayerBrushSettingsWindowViewModel : Conductor<BrushConfigurationViewModel>
    {
        private int _height;
        private int _width;

        public LayerBrushSettingsWindowViewModel(BrushConfigurationViewModel configurationViewModel, LayerBrushConfigurationDialog configuration)
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
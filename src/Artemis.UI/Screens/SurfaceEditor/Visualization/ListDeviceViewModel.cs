using Artemis.Core;
using SkiaSharp;
using Stylet;

namespace Artemis.UI.Screens.SurfaceEditor.Visualization
{
    public class ListDeviceViewModel : PropertyChangedBase
    {
        private bool _isSelected;
        private SKColor _color;
        public ArtemisDevice Device { get; }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetAndNotify(ref _isSelected, value);
        }

        public SKColor Color
        {
            get => _color;
            set => SetAndNotify(ref _color, value);
        }

        public ListDeviceViewModel(ArtemisDevice device)
        {
            Device = device;
        }
    }
}
using Artemis.Core;
using Stylet;

namespace Artemis.UI.Screens.SurfaceEditor.Visualization
{
    public class ListDeviceViewModel : PropertyChangedBase
    {
        private bool _isSelected;
        public ArtemisDevice Device { get; }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetAndNotify(ref _isSelected, value);
        }

        public ListDeviceViewModel(ArtemisDevice device)
        {
            Device = device;
        }
    }
}
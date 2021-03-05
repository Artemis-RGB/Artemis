using Artemis.Core;
using SkiaSharp;
using Stylet;

namespace Artemis.UI.Screens.SurfaceEditor.Visualization
{
    public class ListDeviceViewModel : PropertyChangedBase
    {
        private SKColor _color;
        private bool _isSelected;
        
        public ListDeviceViewModel(ArtemisDevice device, SurfaceEditorViewModel surfaceEditorViewModel)
        {
            Device = device;
            Parent = surfaceEditorViewModel;
        }

        public ArtemisDevice Device { get; }
        public SurfaceEditorViewModel Parent { get; }
        
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
    }
}
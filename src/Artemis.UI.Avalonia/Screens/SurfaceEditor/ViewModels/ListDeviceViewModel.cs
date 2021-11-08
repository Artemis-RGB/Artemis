using Artemis.Core;
using Artemis.UI.Avalonia.Shared;
using ReactiveUI;
using SkiaSharp;

namespace Artemis.UI.Avalonia.Screens.SurfaceEditor.ViewModels
{
    public class ListDeviceViewModel : ViewModelBase
    {
        private SKColor _color;
        private bool _isSelected;
        
        public ListDeviceViewModel(ArtemisDevice device)
        {
            Device = device;
        }

        public ArtemisDevice Device { get; }
        
        public bool IsSelected
        {
            get => _isSelected;
            set => this.RaiseAndSetIfChanged(ref _isSelected, value);    
        }

        public SKColor Color
        {
            get => _color;
            set => this.RaiseAndSetIfChanged(ref _color, value);
        }   
    }
}
using Artemis.Core;
using Artemis.UI.Shared;
using SkiaSharp;

namespace Artemis.UI.Screens.SurfaceEditor;

public class ListDeviceViewModel : ViewModelBase
{
    private SKColor _color;
    private bool _isSelected;

    public ListDeviceViewModel(ArtemisDevice device, SurfaceEditorViewModel surfaceEditorViewModel)
    {
        Device = device;
        SurfaceEditorViewModel = surfaceEditorViewModel;
    }

    public ArtemisDevice Device { get; }
    public SurfaceEditorViewModel SurfaceEditorViewModel { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set => RaiseAndSetIfChanged(ref _isSelected, value);
    }

    public SKColor Color
    {
        get => _color;
        set => RaiseAndSetIfChanged(ref _color, value);
    }
}
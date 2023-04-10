using System.Collections.Generic;
using System.Linq;
using Artemis.UI.Shared.Events;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.PanAndZoom;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.SurfaceEditor;

public partial class SurfaceEditorView : ReactiveUserControl<SurfaceEditorViewModel>
{
    public SurfaceEditorView()
    {
        InitializeComponent();
        ContainerZoomBorder.PropertyChanged += ZoomBorderOnPropertyChanged;
        UpdateZoomBorderBackground();
    }

    private void ZoomBorderOnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name == nameof(ZoomBorder.Background))
            UpdateZoomBorderBackground();
    }


    private void ZoomBorder_OnZoomChanged(object sender, ZoomChangedEventArgs e)
    {
        UpdateZoomBorderBackground();
        SurfaceBounds.BorderThickness = new Thickness(2 / ContainerZoomBorder.ZoomX);
    }

    private void SelectionRectangle_OnSelectionUpdated(object? sender, SelectionRectangleEventArgs e)
    {
        List<Control> containers = DeviceContainer.GetRealizedContainers().Where(c => c.Bounds.Intersects(e.Rectangle)).ToList();
        List<SurfaceDeviceViewModel> viewModels = containers.Where(c => c.DataContext is SurfaceDeviceViewModel).Select(c => (SurfaceDeviceViewModel) c.DataContext!).ToList();
        ViewModel?.UpdateSelection(viewModels, e.KeyModifiers.HasFlag(KeyModifiers.Shift), e.KeyModifiers.HasFlag(KeyModifiers.Control));
    }

    private void ZoomBorder_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!SelectionRectangle.IsSelecting && e.InitialPressMouseButton == MouseButton.Left)
            ViewModel?.ClearSelection();
    }

    private void UpdateZoomBorderBackground()
    {
        if (ContainerZoomBorder.Background is VisualBrush visualBrush)
            visualBrush.DestinationRect = new RelativeRect(ContainerZoomBorder.OffsetX * -1, ContainerZoomBorder.OffsetY * -1, 20, 20, RelativeUnit.Absolute);
    }
}
using System.Collections.Generic;
using System.Linq;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Events;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.PanAndZoom;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.SurfaceEditor;

public class SurfaceEditorView : ReactiveUserControl<SurfaceEditorViewModel>
{
    private readonly ItemsControl _deviceContainer;
    private readonly SelectionRectangle _selectionRectangle;
    private readonly Border _surfaceBounds;
    private readonly ZoomBorder _zoomBorder;

    public SurfaceEditorView()
    {
        InitializeComponent();

        _zoomBorder = this.Find<ZoomBorder>("ZoomBorder");
        _deviceContainer = this.Find<ItemsControl>("DeviceContainer");
        _selectionRectangle = this.Find<SelectionRectangle>("SelectionRectangle");
        _surfaceBounds = this.Find<Border>("SurfaceBounds");

        _zoomBorder.PropertyChanged += ZoomBorderOnPropertyChanged;
        UpdateZoomBorderBackground();
    }

    private void ZoomBorderOnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name == nameof(_zoomBorder.Background))
            UpdateZoomBorderBackground();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void ZoomBorder_OnZoomChanged(object sender, ZoomChangedEventArgs e)
    {
        UpdateZoomBorderBackground();
        _selectionRectangle.BorderThickness = 1 / _zoomBorder.ZoomX;
        _surfaceBounds.BorderThickness = new Thickness(2 / _zoomBorder.ZoomX);
    }

    private void SelectionRectangle_OnSelectionUpdated(object? sender, SelectionRectangleEventArgs e)
    {
        List<ItemContainerInfo> itemContainerInfos = _deviceContainer.ItemContainerGenerator.Containers.Where(c => c.ContainerControl.Bounds.Intersects(e.Rectangle)).ToList();
        List<SurfaceDeviceViewModel> viewModels = itemContainerInfos.Where(c => c.Item is SurfaceDeviceViewModel).Select(c => (SurfaceDeviceViewModel) c.Item).ToList();
        ViewModel?.UpdateSelection(viewModels, e.KeyModifiers.HasFlag(KeyModifiers.Shift), e.KeyModifiers.HasFlag(KeyModifiers.Control));
    }

    private void ZoomBorder_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_selectionRectangle.IsSelecting && e.InitialPressMouseButton == MouseButton.Left)
            ViewModel?.ClearSelection();
    }

    private void UpdateZoomBorderBackground()
    {
        if (_zoomBorder.Background is VisualBrush visualBrush)
            visualBrush.DestinationRect = new RelativeRect(_zoomBorder.OffsetX * -1, _zoomBorder.OffsetY * -1, 20, 20, RelativeUnit.Absolute);
    }
}
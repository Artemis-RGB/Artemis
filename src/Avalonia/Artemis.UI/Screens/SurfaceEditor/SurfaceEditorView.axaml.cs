using System.Reactive;
using Artemis.UI.Shared.Controls;
using Artemis.UI.Shared.Events;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.PanAndZoom;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.SurfaceEditor
{
    public class SurfaceEditorView : ReactiveUserControl<SurfaceEditorViewModel>
    {
        private readonly Grid _containerGrid;
        private readonly SelectionRectangle _selectionRectangle;
        private readonly Border _surfaceBounds;
        private readonly ZoomBorder _zoomBorder;
        private bool _dragging;

        public SurfaceEditorView()
        {
            InitializeComponent();

            _zoomBorder = this.Find<ZoomBorder>("ZoomBorder");
            _containerGrid = this.Find<Grid>("ContainerGrid");
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

        private void ZoomBorder_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                return;

            _dragging = false;

            if (e.Source is Border {Name: "SurfaceDeviceBorder"})
            {
                e.Pointer.Capture(_zoomBorder);
                e.Handled = true;
            }
        }

        private void ZoomBorder_OnPointerMoved(object? sender, PointerEventArgs e)
        {
            if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                return;

            if (ReferenceEquals(e.Pointer.Captured, sender))
            {
                if (!_dragging)
                    ViewModel?.StartMouseDrag(e.GetPosition(_containerGrid));
                ViewModel?.UpdateMouseDrag(e.GetPosition(_containerGrid));
            }

            _dragging = true;
        }

        private void ZoomBorder_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (e.InitialPressMouseButton != MouseButton.Left)
                return;

            // If the mouse didn't move, apply selection
            if (!_dragging)
            {
                ViewModel?.SelectFirstDeviceAtPoint(e.GetPosition(_containerGrid), e.KeyModifiers.HasFlag(KeyModifiers.Shift));
                return;
            }

            if (ReferenceEquals(e.Pointer.Captured, sender))
            {
                ViewModel?.StopMouseDrag(e.GetPosition(_containerGrid));
                e.Pointer.Capture(null);
            }
        }

        private void SelectionRectangle_OnSelectionUpdated(object? sender, SelectionRectangleEventArgs e)
        {
            ViewModel?.UpdateSelection(e.Rectangle, e.KeyModifiers.HasFlag(KeyModifiers.Shift));
        }

        private void UpdateZoomBorderBackground()
        {
            if (_zoomBorder.Background is VisualBrush visualBrush)
                visualBrush.DestinationRect = new RelativeRect(_zoomBorder.OffsetX * -1, _zoomBorder.OffsetY * -1, 20, 20, RelativeUnit.Absolute);
        }
    }
}
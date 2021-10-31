using Artemis.UI.Avalonia.Screens.SurfaceEditor.ViewModels;
using Artemis.UI.Avalonia.Shared.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.PanAndZoom;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using Avalonia.VisualTree;

namespace Artemis.UI.Avalonia.Screens.SurfaceEditor.Views
{
    public class SurfaceEditorView : ReactiveUserControl<SurfaceEditorViewModel>
    {
        private readonly SelectionRectangle _selectionRectangle;
        private readonly Grid _containerGrid;
        private readonly ZoomBorder _zoomBorder;
        private readonly Border _surfaceBounds;

        public SurfaceEditorView()
        {
            InitializeComponent();

            _zoomBorder = this.Find<ZoomBorder>("ZoomBorder");
            _containerGrid = this.Find<Grid>("ContainerGrid");
            _selectionRectangle = this.Find<SelectionRectangle>("SelectionRectangle");
            _surfaceBounds = this.Find<Border>("SurfaceBounds");


            ((VisualBrush) _zoomBorder.Background).DestinationRect = new RelativeRect(_zoomBorder.OffsetX * -1, _zoomBorder.OffsetY * -1, 20, 20, RelativeUnit.Absolute);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void ZoomBorder_OnZoomChanged(object sender, ZoomChangedEventArgs e)
        {
            ((VisualBrush) _zoomBorder.Background).DestinationRect = new RelativeRect(_zoomBorder.OffsetX * -1, _zoomBorder.OffsetY * -1, 20, 20, RelativeUnit.Absolute);
            _selectionRectangle.BorderThickness = 1 / _zoomBorder.ZoomX;
            _surfaceBounds.BorderThickness = new Thickness(2 / _zoomBorder.ZoomX);
        }

        private void ZoomBorder_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                return;

            if (e.Source is Border {Name: "SurfaceDeviceBorder"})
            {
                e.Pointer.Capture(_zoomBorder);
                e.Handled = true;
                ViewModel?.StartMouseDrag(e.GetPosition(_containerGrid));
            }
            else
                ViewModel?.ClearSelection();
        }

        private void ZoomBorder_OnPointerMoved(object? sender, PointerEventArgs e)
        {
            if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                return;

            if (ReferenceEquals(e.Pointer.Captured, sender)) 
                ViewModel?.UpdateMouseDrag(e.GetPosition(_containerGrid));
        }

        private void ZoomBorder_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (e.InitialPressMouseButton != MouseButton.Left)
                return;

            if (ReferenceEquals(e.Pointer.Captured, sender))
            {
                ViewModel?.StopMouseDrag(e.GetPosition(_containerGrid));
                e.Pointer.Capture(null);
            }
        }
    }
}
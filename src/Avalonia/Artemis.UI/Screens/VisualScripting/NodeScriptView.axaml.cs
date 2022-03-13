using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.PanAndZoom;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using Avalonia.VisualTree;

namespace Artemis.UI.Screens.VisualScripting
{
    public partial class NodeScriptView : ReactiveUserControl<NodeScriptViewModel>
    {
        private readonly ZoomBorder _zoomBorder;
        private readonly Grid _grid;

        public NodeScriptView()
        {
            InitializeComponent();

            _zoomBorder = this.Find<ZoomBorder>("ZoomBorder");
            _grid = this.Find<Grid>("ContainerGrid");
            _zoomBorder.PropertyChanged += ZoomBorderOnPropertyChanged;
            UpdateZoomBorderBackground();

            _grid?.AddHandler(PointerReleasedEvent, CanvasOnPointerReleased, RoutingStrategies.Direct | RoutingStrategies.Tunnel | RoutingStrategies.Bubble, true);
        }

        private void CanvasOnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            // If the flyout handled the click, update the position of the node picker
            if (e.Handled && ViewModel != null)
                ViewModel.NodePickerViewModel.Position = e.GetPosition(_grid);
        }

        private void ZoomBorderOnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property.Name == nameof(_zoomBorder.Background))
                UpdateZoomBorderBackground();
        }

        private void UpdateZoomBorderBackground()
        {
            if (_zoomBorder.Background is VisualBrush visualBrush)
                visualBrush.DestinationRect = new RelativeRect(_zoomBorder.OffsetX * -1, _zoomBorder.OffsetY * -1, 20, 20, RelativeUnit.Absolute);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void ZoomBorder_OnZoomChanged(object sender, ZoomChangedEventArgs e)
        {
            UpdateZoomBorderBackground();
        }
    }
}
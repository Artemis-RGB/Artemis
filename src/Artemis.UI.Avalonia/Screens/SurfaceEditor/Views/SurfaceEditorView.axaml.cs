using Artemis.UI.Avalonia.Screens.SurfaceEditor.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.PanAndZoom;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Avalonia.Screens.SurfaceEditor.Views
{
    public class SurfaceEditorView : ReactiveUserControl<SurfaceEditorViewModel>
    {
        private readonly ZoomBorder _zoomBorder;

        public SurfaceEditorView()
        {
            InitializeComponent();

            _zoomBorder = this.Find<ZoomBorder>("ZoomBorder");
            ((VisualBrush)_zoomBorder.Background).DestinationRect = new RelativeRect(_zoomBorder.OffsetX * -1, _zoomBorder.OffsetY * -1, 20, 20, RelativeUnit.Absolute);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void ZoomBorder_OnZoomChanged(object sender, ZoomChangedEventArgs e)
        {
            ((VisualBrush) _zoomBorder.Background).DestinationRect = new RelativeRect(_zoomBorder.OffsetX * -1, _zoomBorder.OffsetY * -1, 20, 20, RelativeUnit.Absolute);
        }
    }
}
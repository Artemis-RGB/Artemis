using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.PanAndZoom;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.VisualEditor
{
    public class VisualEditorView : ReactiveUserControl<VisualEditorViewModel>
    {
        private readonly ZoomBorder _zoomBorder;

        public VisualEditorView()
        {
            InitializeComponent();

            _zoomBorder = this.Find<ZoomBorder>("ZoomBorder");
            _zoomBorder.PropertyChanged += ZoomBorderOnPropertyChanged;
            UpdateZoomBorderBackground();
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
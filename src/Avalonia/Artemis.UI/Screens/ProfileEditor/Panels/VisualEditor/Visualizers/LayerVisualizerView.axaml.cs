using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.PanAndZoom;
using Avalonia.Controls.Shapes;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Avalonia.VisualTree;

namespace Artemis.UI.Screens.ProfileEditor.VisualEditor.Visualizers
{
    public partial class LayerVisualizerView : ReactiveUserControl<LayerVisualizerViewModel>
    {
        private ZoomBorder? _zoomBorder;
        private readonly Path _layerVisualizer;

        public LayerVisualizerView()
        {
            InitializeComponent();
            _layerVisualizer = this.Get<Path>("LayerVisualizer");
        }

        #region Overrides of TemplatedControl

        /// <inheritdoc />
        protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            _zoomBorder = (ZoomBorder?) this.GetLogicalAncestors().FirstOrDefault(l => l is ZoomBorder);
            if (_zoomBorder != null)
                _zoomBorder.PropertyChanged += ZoomBorderOnPropertyChanged;
            base.OnAttachedToLogicalTree(e);
        }

        /// <inheritdoc />
        protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            if (_zoomBorder != null)
                _zoomBorder.PropertyChanged -= ZoomBorderOnPropertyChanged;
            base.OnDetachedFromLogicalTree(e);
        }

        private void ZoomBorderOnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property != ZoomBorder.ZoomXProperty || _zoomBorder == null)
                return;
            _layerVisualizer.StrokeThickness = Math.Max(1, 4 / _zoomBorder.ZoomX);
        }

        #endregion

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
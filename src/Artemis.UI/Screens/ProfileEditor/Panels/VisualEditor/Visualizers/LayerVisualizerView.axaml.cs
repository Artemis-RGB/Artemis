using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.PanAndZoom;
using Avalonia.Controls.Shapes;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.VisualEditor.Visualizers;

public class LayerVisualizerView : ReactiveUserControl<LayerVisualizerViewModel>
{
    private readonly Path _layerVisualizer;
    private ZoomBorder? _zoomBorder;

    public LayerVisualizerView()
    {
        InitializeComponent();
        _layerVisualizer = this.Get<Path>("LayerVisualizer");
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
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
}
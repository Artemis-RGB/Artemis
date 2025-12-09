using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls.PanAndZoom;
using Avalonia.LogicalTree;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Profiles.ProfileEditor.VisualEditor.Visualizers;

public partial class LayerVisualizerView : ReactiveUserControl<LayerVisualizerViewModel>
{
    private ZoomBorder? _zoomBorder;

    public LayerVisualizerView()
    {
        InitializeComponent();
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
        LayerVisualizer.StrokeThickness = Math.Max(1, 4 / _zoomBorder.ZoomX);
    }

    #endregion
}
using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.PanAndZoom;
using Avalonia.Controls.Shapes;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.VisualEditor.Visualizers;

public class LayerShapeVisualizerView : ReactiveUserControl<LayerShapeVisualizerViewModel>
{
    private readonly Path _layerVisualizer;
    private readonly Path _layerVisualizerUnbound;
    private ZoomBorder? _zoomBorder;

    public LayerShapeVisualizerView()
    {
        InitializeComponent();
        _layerVisualizer = this.Get<Path>("LayerVisualizer");
        _layerVisualizerUnbound = this.Get<Path>("LayerVisualizerUnbound");

        this.WhenActivated(d => ViewModel.WhenAnyValue(vm => vm.Selected).Subscribe(_ => UpdateStrokeThickness()).DisposeWith(d));
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

        UpdateStrokeThickness();
    }

    private void UpdateStrokeThickness()
    {
        if (_zoomBorder == null)
            return;

        if (ViewModel != null && ViewModel.Selected)
        {
            _layerVisualizer.StrokeThickness = Math.Max(1, 4 / _zoomBorder.ZoomX);
            _layerVisualizerUnbound.StrokeThickness = Math.Max(1, 4 / _zoomBorder.ZoomX);
        }
        else
        {
            _layerVisualizer.StrokeThickness = Math.Max(1, 4 / _zoomBorder.ZoomX) / 2;
            _layerVisualizerUnbound.StrokeThickness = Math.Max(1, 4 / _zoomBorder.ZoomX) / 2;
        }
    }

    #endregion
}
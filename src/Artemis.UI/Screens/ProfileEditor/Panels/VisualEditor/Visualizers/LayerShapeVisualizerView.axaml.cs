using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls.PanAndZoom;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.VisualEditor.Visualizers;

public partial class LayerShapeVisualizerView : ReactiveUserControl<LayerShapeVisualizerViewModel>
{
    private ZoomBorder? _zoomBorder;

    public LayerShapeVisualizerView()
    {
        InitializeComponent();
        this.WhenActivated(d => ViewModel.WhenAnyValue(vm => vm.Selected).ObserveOn(AvaloniaScheduler.Instance).Subscribe(_ => UpdateStrokeThickness()).DisposeWith(d));
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
            LayerVisualizer.StrokeThickness = Math.Max(1, 4 / _zoomBorder.ZoomX);
            LayerVisualizerUnbound.StrokeThickness = Math.Max(1, 4 / _zoomBorder.ZoomX);
        }
        else
        {
            LayerVisualizer.StrokeThickness = Math.Max(1, 4 / _zoomBorder.ZoomX) / 2;
            LayerVisualizerUnbound.StrokeThickness = Math.Max(1, 4 / _zoomBorder.ZoomX) / 2;
        }
    }

    #endregion
}
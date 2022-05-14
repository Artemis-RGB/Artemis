using System;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Extensions;
using Artemis.UI.Shared.Services.ProfileEditor;
using Avalonia;
using Avalonia.Controls.Mixins;
using Avalonia.Media;
using Avalonia.Skia;
using ReactiveUI;
using SkiaSharp;

namespace Artemis.UI.Screens.ProfileEditor.VisualEditor.Visualizers;

public class LayerShapeVisualizerViewModel : ActivatableViewModelBase, IVisualizerViewModel
{
    private Rect _layerBounds;
    private ObservableAsPropertyHelper<bool>? _selected;
    private Geometry? _shapeGeometry;
    private double _x;
    private double _y;

    public LayerShapeVisualizerViewModel(Layer layer, IProfileEditorService profileEditorService)
    {
        Layer = layer;

        this.WhenActivated(d =>
        {
            Observable.FromEventPattern(x => Layer.RenderPropertiesUpdated += x, x => Layer.RenderPropertiesUpdated -= x).Subscribe(_ => Update()).DisposeWith(d);
            Observable.FromEventPattern<LayerPropertyEventArgs>(x => Layer.Transform.Position.CurrentValueSet += x, x => Layer.Transform.Position.CurrentValueSet -= x)
                .Subscribe(_ => UpdateTransform())
                .DisposeWith(d);
            Observable.FromEventPattern<LayerPropertyEventArgs>(x => Layer.Transform.Rotation.CurrentValueSet += x, x => Layer.Transform.Rotation.CurrentValueSet -= x)
                .Subscribe(_ => UpdateTransform())
                .DisposeWith(d);
            Observable.FromEventPattern<LayerPropertyEventArgs>(x => Layer.Transform.Scale.CurrentValueSet += x, x => Layer.Transform.Scale.CurrentValueSet -= x)
                .Subscribe(_ => UpdateTransform())
                .DisposeWith(d);
            Observable.FromEventPattern<LayerPropertyEventArgs>(x => Layer.Transform.AnchorPoint.CurrentValueSet += x, x => Layer.Transform.AnchorPoint.CurrentValueSet -= x)
                .Subscribe(_ => UpdateTransform())
                .DisposeWith(d);

            _selected = profileEditorService.ProfileElement.Select(p => p == Layer).ToProperty(this, vm => vm.Selected).DisposeWith(d);

            profileEditorService.Time.Subscribe(_ => UpdateTransform()).DisposeWith(d);
            Update();
        });
    }

    public Layer Layer { get; }
    public bool Selected => _selected?.Value ?? false;

    public Rect LayerBounds
    {
        get => _layerBounds;
        private set => RaiseAndSetIfChanged(ref _layerBounds, value);
    }

    public Geometry? ShapeGeometry
    {
        get => _shapeGeometry;
        set => RaiseAndSetIfChanged(ref _shapeGeometry, value);
    }

    private void Update()
    {
        UpdateLayerBounds();

        if (Layer.General.ShapeType.CurrentValue == LayerShapeType.Rectangle)
            ShapeGeometry = new RectangleGeometry(LayerBounds);
        else
            ShapeGeometry = new EllipseGeometry(LayerBounds);

        UpdateTransform();
    }

    private void UpdateLayerBounds()
    {
        SKRect bounds = Layer.GetLayerBounds();
        LayerBounds = new Rect(0, 0, bounds.Width, bounds.Height);
        X = bounds.Left;
        Y = bounds.Top;
    }

    private void UpdateTransform()
    {
        if (ShapeGeometry != null)
            ShapeGeometry.Transform = new MatrixTransform(Layer.GetTransformMatrix(false, true, true, true, LayerBounds.ToSKRect()).ToMatrix());
    }

    public ProfileElement ProfileElement => Layer;

    public double X
    {
        get => _x;
        set => RaiseAndSetIfChanged(ref _x, value);
    }

    public double Y
    {
        get => _y;
        set => RaiseAndSetIfChanged(ref _y, value);
    }

    public int Order => 2;
}
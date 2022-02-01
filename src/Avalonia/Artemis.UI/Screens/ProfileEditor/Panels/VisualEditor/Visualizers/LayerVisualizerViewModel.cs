using System;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Extensions;
using Artemis.UI.Shared.Services.ProfileEditor;
using Avalonia.Controls.Mixins;
using Avalonia.Media;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.VisualEditor.Visualizers;

public class LayerVisualizerViewModel : ActivatableViewModelBase, IVisualizerViewModel
{
    private Geometry? _shapeGeometry;
    private ObservableAsPropertyHelper<bool>? _selected;

    public LayerVisualizerViewModel(Layer layer, IProfileEditorService profileEditorService)
    {
        Layer = layer;

        this.WhenActivated(d =>
        {
            Observable.FromEventPattern(x => Layer.RenderPropertiesUpdated += x, x => Layer.RenderPropertiesUpdated -= x).Subscribe(_ => UpdateShape()).DisposeWith(d);
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
            UpdateShape();
        });
    }

    public Layer Layer { get; }
    public bool Selected => _selected?.Value ?? false;

    public Geometry? ShapeGeometry
    {
        get => _shapeGeometry;
        set => this.RaiseAndSetIfChanged(ref _shapeGeometry, value);
    }

    private void UpdateShape()
    {
        if (Layer.General.ShapeType.CurrentValue == LayerShapeType.Rectangle)
            ShapeGeometry = new RectangleGeometry(Layer.Bounds.ToRect());
        else
            ShapeGeometry = new EllipseGeometry(Layer.Bounds.ToRect());
    }

    private void UpdateTransform()
    {
        if (ShapeGeometry != null)
            ShapeGeometry.Transform = new MatrixTransform(Layer.GetTransformMatrix(false, true, true, true).ToMatrix());
    }
}
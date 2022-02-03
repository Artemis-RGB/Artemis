using System;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Extensions;
using Artemis.UI.Shared.Services.ProfileEditor;
using Avalonia;
using Avalonia.Controls.Mixins;
using Avalonia.Media;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.VisualEditor.Visualizers;

public class LayerShapeVisualizerViewModel : ActivatableViewModelBase, IVisualizerViewModel
{
    private ObservableAsPropertyHelper<bool>? _selected;
    private Geometry? _shapeGeometry;

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
            UpdateTransform();
        });
    }

    public Layer Layer { get; }
    public bool Selected => _selected?.Value ?? false;
    public Rect LayerBounds => Layer.Bounds.ToRect();

    public Geometry? ShapeGeometry
    {
        get => _shapeGeometry;
        set => this.RaiseAndSetIfChanged(ref _shapeGeometry, value);
    }

    private void Update()
    {
        if (Layer.General.ShapeType.CurrentValue == LayerShapeType.Rectangle)
            ShapeGeometry = new RectangleGeometry(new Rect(0, 0, Layer.Bounds.Width, Layer.Bounds.Height));
        else
            ShapeGeometry = new EllipseGeometry(new Rect(0, 0, Layer.Bounds.Width, Layer.Bounds.Height));

        this.RaisePropertyChanged(nameof(X));
        this.RaisePropertyChanged(nameof(Y));
        this.RaisePropertyChanged(nameof(LayerBounds));
    }

    private void UpdateTransform()
    {
        if (ShapeGeometry != null)
            ShapeGeometry.Transform = new MatrixTransform(Layer.GetTransformMatrix(true, true, true, true).ToMatrix());
    }

    public int X => Layer.Bounds.Left;
    public int Y => Layer.Bounds.Top;
    public int Order => 2;
}
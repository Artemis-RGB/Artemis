using System;
using System.Reactive;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.UI.Shared.Extensions;
using Artemis.UI.Shared.Services.ProfileEditor;
using Avalonia;
using Avalonia.Controls.Mixins;
using Material.Icons;
using ReactiveUI;
using SkiaSharp;

namespace Artemis.UI.Screens.ProfileEditor.VisualEditor.Tools;

public class TransformToolViewModel : ToolViewModel
{
    private readonly ObservableAsPropertyHelper<bool> _isEnabled;
    private RelativePoint _relativeAnchor;
    private double _inverseRotation;
    private ObservableAsPropertyHelper<Layer?>? _layer;
    private double _rotation;
    private Rect _shapeBounds;
    private Point _anchor;

    /// <inheritdoc />
    public TransformToolViewModel(IProfileEditorService profileEditorService)
    {
        // Not disposed when deactivated but when really disposed
        _isEnabled = profileEditorService.ProfileElement.Select(p => p is Layer).ToProperty(this, vm => vm.IsEnabled);

        this.WhenActivated(d =>
        {
            _layer = profileEditorService.ProfileElement.Select(p => p as Layer).ToProperty(this, vm => vm.Layer).DisposeWith(d);

            this.WhenAnyValue(vm => vm.Layer)
                .Select(l => l != null
                    ? Observable.FromEventPattern(x => l.RenderPropertiesUpdated += x, x => l.RenderPropertiesUpdated -= x)
                    : Observable.Never<EventPattern<object>>())
                .Switch()
                .Subscribe(_ => Update())
                .DisposeWith(d);
            this.WhenAnyValue(vm => vm.Layer)
                .Select(l => l != null
                    ? Observable.FromEventPattern<LayerPropertyEventArgs>(x => l.Transform.Position.CurrentValueSet += x, x => l.Transform.Position.CurrentValueSet -= x)
                    : Observable.Never<EventPattern<LayerPropertyEventArgs>>())
                .Switch()
                .Subscribe(_ => Update())
                .DisposeWith(d);
            this.WhenAnyValue(vm => vm.Layer)
                .Select(l => l != null
                    ? Observable.FromEventPattern<LayerPropertyEventArgs>(x => l.Transform.Rotation.CurrentValueSet += x, x => l.Transform.Rotation.CurrentValueSet -= x)
                    : Observable.Never<EventPattern<LayerPropertyEventArgs>>())
                .Switch()
                .Subscribe(_ => Update())
                .DisposeWith(d);
            this.WhenAnyValue(vm => vm.Layer)
                .Select(l => l != null
                    ? Observable.FromEventPattern<LayerPropertyEventArgs>(x => l.Transform.Scale.CurrentValueSet += x, x => l.Transform.Scale.CurrentValueSet -= x)
                    : Observable.Never<EventPattern<LayerPropertyEventArgs>>())
                .Switch()
                .Subscribe(_ => Update())
                .DisposeWith(d);
            this.WhenAnyValue(vm => vm.Layer)
                .Select(l => l != null
                    ? Observable.FromEventPattern<LayerPropertyEventArgs>(x => l.Transform.AnchorPoint.CurrentValueSet += x, x => l.Transform.AnchorPoint.CurrentValueSet -= x)
                    : Observable.Never<EventPattern<LayerPropertyEventArgs>>())
                .Switch()
                .Subscribe(_ => Update())
                .DisposeWith(d);

            this.WhenAnyValue(vm => vm.Layer).Subscribe(_ => Update()).DisposeWith(d);
            profileEditorService.Time.Subscribe(_ => Update()).DisposeWith(d);
        });
    }

    public Layer? Layer => _layer?.Value;

    /// <inheritdoc />
    public override bool IsEnabled => _isEnabled?.Value ?? false;

    /// <inheritdoc />
    public override bool IsExclusive => true;

    /// <inheritdoc />
    public override bool ShowInToolbar => true;

    /// <inheritdoc />
    public override int Order => 3;

    /// <inheritdoc />
    public override MaterialIconKind Icon => MaterialIconKind.TransitConnectionVariant;

    /// <inheritdoc />
    public override string ToolTip => "Transform the shape of the current layer";

    public Rect ShapeBounds
    {
        get => _shapeBounds;
        set => RaiseAndSetIfChanged(ref _shapeBounds, value);
    }

    public double Rotation
    {
        get => _rotation;
        set => RaiseAndSetIfChanged(ref _rotation, value);
    }

    public double InverseRotation
    {
        get => _inverseRotation;
        set => RaiseAndSetIfChanged(ref _inverseRotation, value);
    }

    public RelativePoint RelativeAnchor
    {
        get => _relativeAnchor;
        set => RaiseAndSetIfChanged(ref _relativeAnchor, value);
    }

    public Point Anchor
    {
        get => _anchor;
        set => RaiseAndSetIfChanged(ref _anchor, value);
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _isEnabled?.Dispose();

        base.Dispose(disposing);
    }

    private void Update()
    {
        if (Layer == null)
            return;

        SKPath path = new();
        path.AddRect(Layer.GetLayerBounds());
        path.Transform(Layer.GetTransformMatrix(false, true, true, false));

        ShapeBounds = path.Bounds.ToRect();
        Rotation = Layer.Transform.Rotation.CurrentValue;
        InverseRotation = Rotation * -1;

        // The middle of the element is 0.5/0.5 in Avalonia, in Artemis it's 0.0/0.0 so compensate for that below
        SKPoint layerAnchor = Layer.Transform.AnchorPoint.CurrentValue;
        RelativeAnchor = new RelativePoint(layerAnchor.X + 0.5, layerAnchor.Y + 0.5, RelativeUnit.Relative);
        Anchor = new Point(ShapeBounds.Width * RelativeAnchor.Point.X, ShapeBounds.Height * RelativeAnchor.Point.Y);
    }

    public enum ShapeControlPoint
    {
        TopLeft,
        TopRight,
        BottomRight,
        BottomLeft,
        TopCenter,
        RightCenter,
        BottomCenter,
        LeftCenter,
        LayerShape,
        Anchor
    }
}
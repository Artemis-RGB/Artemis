using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.UI.Shared.Extensions;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.ProfileEditor.Commands;
using Avalonia;
using Avalonia.Controls.Mixins;
using Material.Icons;
using ReactiveUI;
using SkiaSharp;

namespace Artemis.UI.Screens.ProfileEditor.VisualEditor.Tools;

public class TransformToolViewModel : ToolViewModel
{
    private readonly IProfileEditorService _profileEditorService;
    private readonly ObservableAsPropertyHelper<bool> _isEnabled;
    private RelativePoint _relativeAnchor;
    private double _inverseRotation;
    private ObservableAsPropertyHelper<Layer?>? _layer;
    private double _rotation;
    private Rect _shapeBounds;
    private Point _anchor;
    private TimeSpan _time;

    /// <inheritdoc />
    public TransformToolViewModel(IProfileEditorService profileEditorService)
    {
        _profileEditorService = profileEditorService;

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
            profileEditorService.Time.Subscribe(t =>
            {
                _time = t;
                Update();
            }).DisposeWith(d);
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

    #region Resizing

    private SKSize _resizeStartScale;
    private bool _hadKeyframe;

    public void StartResize(SKPoint position)
    {
        if (Layer == null)
            return;

        _resizeStartScale = Layer.Transform.Scale.CurrentValue;
        _hadKeyframe = Layer.Transform.Scale.Keyframes.Any(k => k.Position == _time);
    }

    public void FinishResize(SKPoint position, ResizeSide side, bool evenSides)
    {
        if (Layer == null)
            return;

        // Grab the size one last time
        SKSize size = UpdateResize(position, side, evenSides);

        // If the layer has keyframes, new keyframes may have been added while the user was dragging
        if (Layer.Transform.Scale.KeyframesEnabled)
        {
            // If there was already a keyframe at the old spot, edit that keyframe
            if (_hadKeyframe)
                _profileEditorService.ExecuteCommand(new UpdateLayerProperty<SKSize>(Layer.Transform.Scale, size, _resizeStartScale, _time));
            // If there was no keyframe yet, remove the keyframe that was created while dragging and create a permanent one
            else
            {
                Layer.Transform.Scale.RemoveKeyframe(Layer.Transform.Scale.Keyframes.First(k => k.Position == _time));
                _profileEditorService.ExecuteCommand(new UpdateLayerProperty<SKSize>(Layer.Transform.Scale, size, _time));
            }
        }
        else
        {
            _profileEditorService.ExecuteCommand(new UpdateLayerProperty<SKSize>(Layer.Transform.Scale, size, _resizeStartScale, _time));
        }
    }

    public SKSize UpdateResize(SKPoint position, ResizeSide side, bool evenSides)
    {
        if (Layer == null)
            return SKSize.Empty;

        SKPoint normalizedAnchor = Layer.Transform.AnchorPoint;
        // TODO Remove when anchor is centralized at 0.5,0.5
        normalizedAnchor = new SKPoint(normalizedAnchor.X + 0.5f, normalizedAnchor.Y + 0.5f);

        // The anchor is used to ensure a side can't shrink past the anchor
        SKPoint anchor = Layer.GetLayerAnchorPosition();
        // The bounds are used to determine whether to shrink or grow
        SKRect shapeBounds = Layer.GetLayerPath(true, true, false).Bounds;

        float width = shapeBounds.Width;
        float height = shapeBounds.Height;

        // Resize each side as requested, the sides of each axis are mutually exclusive
        if (side.HasFlag(ResizeSide.Left))
        {
            if (position.X > anchor.X)
                position.X = anchor.X;

            float anchorOffset = 1f - normalizedAnchor.X;
            float difference = MathF.Abs(shapeBounds.Left - position.X);
            if (position.X < shapeBounds.Left)
                width += difference / anchorOffset;
            else
                width -= difference / anchorOffset;
        }
        else if (side.HasFlag(ResizeSide.Right))
        {
            if (position.X < anchor.X)
                position.X = anchor.X;

            float anchorOffset = normalizedAnchor.X;
            float difference = MathF.Abs(shapeBounds.Right - position.X);
            if (position.X > shapeBounds.Right)
                width += difference / anchorOffset;
            else
                width -= difference / anchorOffset;
        }

        if (side.HasFlag(ResizeSide.Top))
        {
            if (position.Y > anchor.Y)
                position.Y = anchor.Y;

            float anchorOffset = 1f - normalizedAnchor.Y;
            float difference = MathF.Abs(shapeBounds.Top - position.Y);
            if (position.Y < shapeBounds.Top)
                height += difference / anchorOffset;
            else
                height -= difference / anchorOffset;
        }
        else if (side.HasFlag(ResizeSide.Bottom))
        {
            if (position.Y < anchor.Y)
                position.Y = anchor.Y;

            float anchorOffset = normalizedAnchor.Y;
            float difference = MathF.Abs(shapeBounds.Bottom - position.Y);
            if (position.Y > shapeBounds.Bottom)
                height += difference / anchorOffset;
            else
                height -= difference / anchorOffset;
        }

        // Even out the sides to the size of the longest side
        if (evenSides)
        {
            if (width > height)
                width = height;
            else
                height = width;
        }

        // Normalize the scale to a percentage
        SKRect bounds = Layer.GetLayerBounds();
        width = width / bounds.Width * 100f;
        height = height / bounds.Height * 100f;

        Layer.Transform.Scale.SetCurrentValue(new SKSize(width, height), _time);
        return new SKSize(width, height);
    }

    #endregion

    #region Rotating



    #endregion

    #region Movement

    

    #endregion

    #region Anchor movement



    #endregion

    [Flags]
    public enum ResizeSide
    {
        Top = 1,
        Right = 2,
        Bottom = 4,
        Left = 8,
    }
}
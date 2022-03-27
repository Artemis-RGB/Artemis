using System;
using System.Reactive;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.UI.Exceptions;
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
    private readonly ObservableAsPropertyHelper<bool> _isEnabled;
    private readonly IProfileEditorService _profileEditorService;
    private Point _anchor;
    private ObservableAsPropertyHelper<Layer?>? _layer;
    private RelativePoint _relativeAnchor;
    private double _rotation;
    private Rect _shapeBounds;
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
        SKRect layerBounds = Layer.GetLayerBounds();
        path.AddRect(layerBounds);
        path.Transform(Layer.GetTransformMatrix(false, true, true, false, layerBounds));

        ShapeBounds = path.Bounds.ToRect();
        Rotation = Layer.Transform.Rotation.CurrentValue;

        SKPoint layerAnchor = Layer.Transform.AnchorPoint.CurrentValue;
        RelativeAnchor = new RelativePoint(layerAnchor.X, layerAnchor.Y, RelativeUnit.Relative);
        Anchor = new Point(ShapeBounds.Width * RelativeAnchor.Point.X, ShapeBounds.Height * RelativeAnchor.Point.Y);
    }

    [Flags]
    public enum ResizeSide
    {
        Top = 1,
        Right = 2,
        Bottom = 4,
        Left = 8
    }

    #region Movement

    private LayerPropertyPreview<SKPoint>? _movementPreview;

    public void StartMovement()
    {
        if (Layer == null)
            return;

        _movementPreview?.DiscardPreview();
        _movementPreview = new LayerPropertyPreview<SKPoint>(Layer.Transform.Position, _time);
    }

    public void UpdateMovement(SKPoint position, bool stickToAxis, bool round)
    {
        if (Layer == null)
            return;
        if (_movementPreview == null)
            throw new ArtemisUIException("Can't update movement without a preview having been started");

        // Get a normalized point
        SKPoint scaled = Layer.GetNormalizedPoint(position, true);
        // Compensate for the anchor
        scaled.X += ((Layer.Transform.AnchorPoint.CurrentValue.X) * (Layer.Transform.Scale.CurrentValue.Width/100f));
        scaled.Y += ((Layer.Transform.AnchorPoint.CurrentValue.Y) * (Layer.Transform.Scale.CurrentValue.Height/100f));
        _movementPreview.Preview(scaled);
    }

    public void FinishMovement()
    {
        if (Layer == null)
            return;
        if (_movementPreview == null)
            throw new ArtemisUIException("Can't update movement without a preview having been started");

        if (_movementPreview.DiscardPreview())
            _profileEditorService.ExecuteCommand(new UpdateLayerProperty<SKPoint>(Layer.Transform.Position, _movementPreview.PreviewValue, _time));
        _movementPreview = null;
    }

    #endregion

    #region Anchor movement

    private SKPoint _dragOffset;
    private SKPoint _dragStartAnchor;

    private LayerPropertyPreview<SKPoint>? _anchorMovementPreview;

    public void StartAnchorMovement(SKPoint position)
    {
        if (Layer == null)
            return;

        // Mouse doesn't care about rotation so get the layer path without rotation
        SKPath path = Layer.GetLayerPath(true, true, false);
        SKPoint topLeft = path.Points[0];
        // Measure from the top-left of the shape (without rotation)
        _dragOffset = topLeft + (position - topLeft);
        // Get the absolute layer anchor and make it relative to the unrotated shape
        _dragStartAnchor = Layer.GetLayerAnchorPosition() - topLeft;

        _movementPreview?.DiscardPreview();
        _anchorMovementPreview?.DiscardPreview();
        _movementPreview = new LayerPropertyPreview<SKPoint>(Layer.Transform.Position, _time);
        _anchorMovementPreview = new LayerPropertyPreview<SKPoint>(Layer.Transform.AnchorPoint, _time);
    }

    public void UpdateAnchorMovement(SKPoint position, bool stickToAxis, bool round)
    {
        if (Layer == null)
            return;
        if (_movementPreview == null || _anchorMovementPreview == null)
            throw new ArtemisUIException("Can't update movement without a preview having been started");

        SKPoint topLeft = Layer.GetLayerPath(true, true, true)[0];

        // The start anchor is relative to an unrotated version of the shape
        SKPoint start = _dragStartAnchor;
        // Add the current position to the start anchor to determine the new position
        SKPoint current = start + (position - _dragOffset);
        // In order to keep the mouse movement unrotated, counter-act the active rotation
        SKPoint[] countered = UnTransformPoints(new[] {start, current}, Layer, start, true);

        // If shift is held down, round down to 1 decimal to allow moving the anchor in big increments
        int decimals = round ? 1 : 3;
        SKPoint scaled = RoundPoint(Layer.GetNormalizedPoint(countered[1], false), decimals);

        // Update the anchor point, this causes the shape to move
        _anchorMovementPreview.Preview(scaled);
        // TopLeft is not updated yet and acts as a snapshot of the top-left before changing the anchor
        SKPath path = Layer.GetLayerPath(true, true, true);
        // Calculate the (scaled) difference between the old and now position
        SKPoint difference = Layer.GetNormalizedPoint(topLeft - path.Points[0], false);
        // Apply the difference so that the shape effectively stays in place
        _movementPreview.Preview(Layer.Transform.Position.CurrentValue + difference);
    }

    public void FinishAnchorMovement()
    {
        if (Layer == null)
            return;
        if (_movementPreview == null || _anchorMovementPreview == null)
            throw new ArtemisUIException("Can't update movement without a preview having been started");

        // Not interested in this one's return value but we do need to discard it
        _movementPreview.DiscardPreview();
        if (_anchorMovementPreview.DiscardPreview())
        {
            using ProfileEditorCommandScope commandScope = _profileEditorService.CreateCommandScope("Update anchor point");
            _profileEditorService.ExecuteCommand(new UpdateLayerProperty<SKPoint>(Layer.Transform.Position, _movementPreview.PreviewValue, _time));
            _profileEditorService.ExecuteCommand(new UpdateLayerProperty<SKPoint>(Layer.Transform.AnchorPoint, _anchorMovementPreview.PreviewValue, _time));
        }

        _movementPreview = null;
        _anchorMovementPreview = null;
    }

    #endregion

    #region Resizing

    private LayerPropertyPreview<SKSize>? _resizePreview;

    public void StartResize()
    {
        if (Layer == null)
            return;

        _resizePreview?.DiscardPreview();
        _resizePreview = new LayerPropertyPreview<SKSize>(Layer.Transform.Scale, _time);
    }

    public void UpdateResize(SKPoint position, ResizeSide side, bool evenPercentages, bool evenPixels, bool round)
    {
        if (Layer == null)
            return;
        if (_resizePreview == null)
            throw new ArtemisUIException("Can't update size without a preview having been started");

        SKPoint normalizedAnchor = Layer.Transform.AnchorPoint;
        normalizedAnchor = new SKPoint(normalizedAnchor.X, normalizedAnchor.Y);

        // The anchor is used to ensure a side can't shrink past the anchor
        SKPoint anchor = Layer.GetLayerAnchorPosition();
        SKRect bounds = Layer.GetLayerBounds();

        float width = Layer.Transform.Scale.CurrentValue.Width;
        float height = Layer.Transform.Scale.CurrentValue.Height;

        // Resize each side as requested, the sides of each axis are mutually exclusive
        if (side.HasFlag(ResizeSide.Left) && normalizedAnchor.X != 0)
        {
            if (position.X > anchor.X)
                position.X = anchor.X;

            float anchorWeight = normalizedAnchor.X;

            float requiredDistance = anchor.X - position.X;
            float requiredSize = requiredDistance / anchorWeight;
            width = requiredSize / bounds.Width * 100f;
            if (round)
                width = MathF.Round(width / 5f) * 5f;
        }
        else if (side.HasFlag(ResizeSide.Right) && Math.Abs(normalizedAnchor.X - 1) > 0.001)
        {
            if (position.X < anchor.X)
                position.X = anchor.X;

            float anchorWeight = 1f - normalizedAnchor.X;

            float requiredDistance = position.X - anchor.X;
            float requiredSize = requiredDistance / anchorWeight;
            width = requiredSize / bounds.Width * 100f;
            if (round)
                width = MathF.Round(width / 5f) * 5f;
        }

        if (side.HasFlag(ResizeSide.Top) && normalizedAnchor.Y != 0)
        {
            if (position.Y > anchor.Y)
                position.Y = anchor.Y;

            float anchorWeight = normalizedAnchor.Y;

            float requiredDistance = anchor.Y - position.Y;
            float requiredSize = requiredDistance / anchorWeight;
            height = requiredSize / bounds.Height * 100f;
            if (round)
                height = MathF.Round(height / 5f) * 5f;
        }
        else if (side.HasFlag(ResizeSide.Bottom) && Math.Abs(normalizedAnchor.Y - 1) > 0.001)
        {
            if (position.Y < anchor.Y)
                position.Y = anchor.Y;

            float anchorWeight = 1f - normalizedAnchor.Y;

            float requiredDistance = position.Y - anchor.Y;
            float requiredSize = requiredDistance / anchorWeight;
            height = requiredSize / bounds.Height * 100f;
            if (round)
                height = MathF.Round(height / 5f) * 5f;
        }

        // Apply side evening but only when resizing on a corner
        bool resizingCorner = (side.HasFlag(ResizeSide.Left) || side.HasFlag(ResizeSide.Right)) && (side.HasFlag(ResizeSide.Top) || side.HasFlag(ResizeSide.Bottom));
        if (evenPercentages && resizingCorner)
        {
            if (width > height)
                width = height;
            else
                height = width;
        }
        else if (evenPixels && resizingCorner)
        {
            if (width * bounds.Width > height * bounds.Height)
                height = width * bounds.Width / bounds.Height;
            else
                width = height * bounds.Height / bounds.Width;
        }

        _resizePreview.Preview(new SKSize(width, height));
    }

    public void FinishResize()
    {
        if (Layer == null)
            return;
        if (_resizePreview == null)
            throw new ArtemisUIException("Can't update size without a preview having been started");

        if (_resizePreview.DiscardPreview())
            _profileEditorService.ExecuteCommand(new UpdateLayerProperty<SKSize>(Layer.Transform.Scale, _resizePreview.PreviewValue, _time));
        _resizePreview = null;
    }

    #endregion

    #region Rotating

    private LayerPropertyPreview<float>? _rotatePreview;

    public void StartRotation()
    {
        if (Layer == null)
            return;

        _rotatePreview?.DiscardPreview();
        _rotatePreview = new LayerPropertyPreview<float>(Layer.Transform.Rotation, _time);
    }

    public void UpdateRotation(float rotation, bool round)
    {
        if (_rotatePreview == null)
            throw new ArtemisUIException("Can't update rotation without a preview having been started");

        if (round)
            rotation = MathF.Round(rotation / 5f) * 5f;

        _rotatePreview.Preview(rotation);
    }

    public void FinishRotation()
    {
        if (Layer == null)
            return;
        if (_rotatePreview == null)
            throw new ArtemisUIException("Can't update rotation without a preview having been started");

        if (_rotatePreview.DiscardPreview())
            _profileEditorService.ExecuteCommand(new UpdateLayerProperty<float>(Layer.Transform.Rotation, _rotatePreview.PreviewValue, _time));
        _rotatePreview = null;
    }

    #endregion

    #region Utilities

    private static SKPoint RoundPoint(SKPoint point, int decimals)
    {
        return new SKPoint(
            (float) Math.Round(point.X, decimals, MidpointRounding.AwayFromZero),
            (float) Math.Round(point.Y, decimals, MidpointRounding.AwayFromZero)
        );
    }

    private static SKPoint[] UnTransformPoints(SKPoint[] skPoints, Layer layer, SKPoint pivot, bool includeScale)
    {
        using SKPath counterRotatePath = new();
        counterRotatePath.AddPoly(skPoints, false);
        counterRotatePath.Transform(SKMatrix.CreateRotationDegrees(layer.Transform.Rotation.CurrentValue * -1, pivot.X, pivot.Y));
        if (includeScale)
            counterRotatePath.Transform(SKMatrix.CreateScale(1f / (layer.Transform.Scale.CurrentValue.Width / 100f), 1f / (layer.Transform.Scale.CurrentValue.Height / 100f)));

        return counterRotatePath.Points;
    }

    #endregion
}
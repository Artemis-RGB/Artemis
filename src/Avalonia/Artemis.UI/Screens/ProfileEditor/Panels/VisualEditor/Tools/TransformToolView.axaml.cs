using System;
using System.Linq;
using Artemis.Core;
using Artemis.UI.Shared.Extensions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.PanAndZoom;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Avalonia.Skia;
using Avalonia.Styling;
using ReactiveUI;
using SkiaSharp;

namespace Artemis.UI.Screens.ProfileEditor.VisualEditor.Tools;

public class TransformToolView : ReactiveUserControl<TransformToolViewModel>
{
    private ZoomBorder? _zoomBorder;
    private SKPoint _dragStart;
    private SKPoint _dragOffset;

    private readonly Ellipse _rotateTopLeft;
    private readonly Ellipse _rotateTopRight;
    private readonly Ellipse _rotateBottomRight;
    private readonly Ellipse _rotateBottomLeft;

    private readonly Rectangle _resizeTopCenter;
    private readonly Rectangle _resizeRightCenter;
    private readonly Rectangle _resizeBottomCenter;
    private readonly Rectangle _resizeLeftCenter;
    private readonly Rectangle _resizeTopLeft;
    private readonly Rectangle _resizeTopRight;
    private readonly Rectangle _resizeBottomRight;
    private readonly Rectangle _resizeBottomLeft;

    private readonly Ellipse _anchorPoint;

    public TransformToolView()
    {
        InitializeComponent();

        _rotateTopLeft = this.Get<Ellipse>("RotateTopLeft");
        _rotateTopRight = this.Get<Ellipse>("RotateTopRight");
        _rotateBottomRight = this.Get<Ellipse>("RotateBottomRight");
        _rotateBottomLeft = this.Get<Ellipse>("RotateBottomLeft");

        _resizeTopCenter = this.Get<Rectangle>("ResizeTopCenter");
        _resizeRightCenter = this.Get<Rectangle>("ResizeRightCenter");
        _resizeBottomCenter = this.Get<Rectangle>("ResizeBottomCenter");
        _resizeLeftCenter = this.Get<Rectangle>("ResizeLeftCenter");
        _resizeTopLeft = this.Get<Rectangle>("ResizeTopLeft");
        _resizeTopRight = this.Get<Rectangle>("ResizeTopRight");
        _resizeBottomRight = this.Get<Rectangle>("ResizeBottomRight");
        _resizeBottomLeft = this.Get<Rectangle>("ResizeBottomLeft");

        _anchorPoint = this.Get<Ellipse>("AnchorPoint");
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    #region Zoom

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

        // TODO
    }

    #endregion

    #region Rotation

    private void RotationOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        Shape? element = (Shape?) sender;
        if (element == null || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed || ViewModel?.Layer == null)
            return;

        _dragStart = e.GetCurrentPoint(_zoomBorder).Position.ToSKPoint();
        _dragOffset = ViewModel.Layer.GetDragOffset(_dragStart);

        e.Pointer.Capture(element);
        e.Handled = true;
    }

    private void RotationOnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        Shape? element = (Shape?) sender;
        if (element == null || !ReferenceEquals(e.Pointer.Captured, element) || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        e.Pointer.Capture(null);
        e.Handled = true;
    }

    private void RotationOnPointerMoved(object? sender, PointerEventArgs e)
    {
        Shape? element = (Shape?) sender;
        if (element == null || !ReferenceEquals(e.Pointer.Captured, element) || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        e.Handled = true;
    }

    #endregion

    #region Movement

    private void MoveOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        Shape? element = (Shape?) sender;
        if (element == null || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed || ViewModel?.Layer == null)
            return;

        _dragStart = e.GetCurrentPoint(_zoomBorder).Position.ToSKPoint();
        _dragOffset = ViewModel.Layer.GetDragOffset(_dragStart);

        e.Pointer.Capture(element);
        e.Handled = true;
    }

    private void MoveOnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        Shape? element = (Shape?) sender;
        if (element == null || !ReferenceEquals(e.Pointer.Captured, element) || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        e.Pointer.Capture(null);
        e.Handled = true;
    }

    private void MoveOnPointerMoved(object? sender, PointerEventArgs e)
    {
        Shape? element = (Shape?) sender;
        if (element == null || !ReferenceEquals(e.Pointer.Captured, element) || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        e.Handled = true;
    }

    #endregion

    #region Resizing

    private void ResizeOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        Shape? element = (Shape?) sender;
        if (element == null || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed || ViewModel?.Layer == null)
            return;

        _dragStart = CounteractLayerRotation(e.GetCurrentPoint(this).Position.ToSKPoint(), ViewModel.Layer);
        _dragOffset = ViewModel.Layer.GetDragOffset(_dragStart);

        SKPoint position = GetPositionForViewModel(e);
        ViewModel.StartResize(position);

        e.Pointer.Capture(element);
        e.Handled = true;
    }

    private void ResizeOnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        Shape? element = (Shape?) sender;
        if (element == null || !ReferenceEquals(e.Pointer.Captured, element) || e.InitialPressMouseButton != MouseButton.Left || ViewModel?.Layer == null)
            return;

        SKPoint position = GetPositionForViewModel(e);
        ViewModel.FinishResize(position, GetResizeDirection(element), e.KeyModifiers.HasFlag(KeyModifiers.Shift));

        e.Pointer.Capture(null);
        e.Handled = true;
    }

    private void ResizeOnPointerMoved(object? sender, PointerEventArgs e)
    {
        Shape? element = (Shape?) sender;
        if (element == null || !ReferenceEquals(e.Pointer.Captured, element) || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed || ViewModel?.Layer == null)
            return;

        SKPoint position = GetPositionForViewModel(e);
        ViewModel.UpdateResize(position, GetResizeDirection(element), e.KeyModifiers.HasFlag(KeyModifiers.Shift));

        e.Handled = true;
    }

    #endregion

    private SKPoint GetPositionForViewModel(PointerEventArgs e)
    {
        if (ViewModel?.Layer == null)
            return SKPoint.Empty;

        SKPoint point = CounteractLayerRotation(e.GetCurrentPoint(this).Position.ToSKPoint(), ViewModel.Layer);
        return point + _dragOffset;
    }

    private static SKPoint CounteractLayerRotation(SKPoint point, Layer layer)
    {
        SKPoint pivot = layer.GetLayerAnchorPosition();

        using SKPath counterRotatePath = new();
        counterRotatePath.AddPoly(new[] {SKPoint.Empty, point}, false);
        counterRotatePath.Transform(SKMatrix.CreateRotationDegrees(layer.Transform.Rotation.CurrentValue * -1, pivot.X, pivot.Y));

        return counterRotatePath.Points[1];
    }

    private TransformToolViewModel.ResizeSide GetResizeDirection(Shape shape)
    {
        if (ReferenceEquals(shape, _resizeTopLeft))
            return TransformToolViewModel.ResizeSide.Top | TransformToolViewModel.ResizeSide.Left;
        if (ReferenceEquals(shape, _resizeTopRight))
            return TransformToolViewModel.ResizeSide.Top | TransformToolViewModel.ResizeSide.Right;
        if (ReferenceEquals(shape, _resizeBottomRight))
            return TransformToolViewModel.ResizeSide.Bottom | TransformToolViewModel.ResizeSide.Right;
        if (ReferenceEquals(shape, _resizeBottomLeft))
            return TransformToolViewModel.ResizeSide.Bottom | TransformToolViewModel.ResizeSide.Left;
        if (ReferenceEquals(shape, _resizeTopCenter))
            return TransformToolViewModel.ResizeSide.Top;
        if (ReferenceEquals(shape, _resizeRightCenter))
            return TransformToolViewModel.ResizeSide.Right;
        if (ReferenceEquals(shape, _resizeBottomCenter))
            return TransformToolViewModel.ResizeSide.Bottom;
        if (ReferenceEquals(shape, _resizeLeftCenter))
            return TransformToolViewModel.ResizeSide.Left;

        throw new ArgumentException("Given shape isn't a resize shape");
    }
}
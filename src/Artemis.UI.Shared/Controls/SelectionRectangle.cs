using System;
using Artemis.Core;
using Artemis.UI.Shared.Events;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace Artemis.UI.Shared;

/// <summary>
///     Visualizes an <see cref="ArtemisDevice" /> with optional per-LED colors
/// </summary>
public class SelectionRectangle : Control
{
    /// <summary>
    ///     Defines the <see cref="Background" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush> BackgroundProperty =
        AvaloniaProperty.Register<SelectionRectangle, IBrush>(nameof(Background), new SolidColorBrush(Colors.CadetBlue, 0.25));

    /// <summary>
    ///     Defines the <see cref="BorderBrush" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush> BorderBrushProperty =
        AvaloniaProperty.Register<SelectionRectangle, IBrush>(nameof(BorderBrush), new SolidColorBrush(Colors.CadetBlue));

    /// <summary>
    ///     Defines the <see cref="BorderThickness" /> property.
    /// </summary>
    public static readonly StyledProperty<double> BorderThicknessProperty =
        AvaloniaProperty.Register<SelectionRectangle, double>(nameof(BorderThickness), 1);

    /// <summary>
    ///     Defines the <see cref="BorderRadius" /> property.
    /// </summary>
    public static readonly StyledProperty<double> BorderRadiusProperty =
        AvaloniaProperty.Register<SelectionRectangle, double>(nameof(BorderRadius));

    /// <summary>
    ///     Defines the <see cref="InputElement" /> property.
    /// </summary>
    public static readonly StyledProperty<IControl?> InputElementProperty =
        AvaloniaProperty.Register<SelectionRectangle, IControl?>(nameof(InputElement), notifying: OnInputElementChanged);

    /// <summary>
    ///     Defines the <see cref="ZoomRatio" /> property.
    /// </summary>
    public static readonly StyledProperty<double> ZoomRatioProperty =
        AvaloniaProperty.Register<SelectionRectangle, double>(nameof(ZoomRatio), 1);


    /// <summary>
    ///     Defines the read-only <see cref="IsSelecting" /> property.
    /// </summary>
    public static readonly DirectProperty<SelectionRectangle, bool> IsSelectingProperty = AvaloniaProperty.RegisterDirect<SelectionRectangle, bool>(nameof(IsSelecting), o => o.IsSelecting);

    private Rect? _absoluteRect;
    private Point _absoluteStartPosition;

    private Rect? _displayRect;
    private bool _isSelecting;
    private IControl? _oldInputElement;
    private Point _startPosition;
    private Point _lastPosition;

    /// <inheritdoc />
    public SelectionRectangle()
    {
        AffectsRender<TextBlock>(BackgroundProperty, BorderBrushProperty, BorderThicknessProperty);
        IsHitTestVisible = false;
    }

    /// <summary>
    ///     Gets or sets a brush used to paint the control's background.
    /// </summary>
    public IBrush Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    /// <summary>
    ///     Gets or sets a brush used to paint the control's border
    /// </summary>
    public IBrush BorderBrush
    {
        get => GetValue(BorderBrushProperty);
        set => SetValue(BorderBrushProperty, value);
    }

    /// <summary>
    ///     Gets or sets the width of the control's border
    /// </summary>
    public double BorderThickness
    {
        get => GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }

    /// <summary>
    ///     Gets or sets the radius of the control's border
    /// </summary>
    public double BorderRadius
    {
        get => GetValue(BorderRadiusProperty);
        set => SetValue(BorderRadiusProperty, value);
    }

    /// <summary>
    ///     Gets or sets the element that captures input for the selection rectangle.
    /// </summary>
    public IControl? InputElement
    {
        get => GetValue(InputElementProperty);
        set => SetValue(InputElementProperty, value);
    }

    /// <summary>
    ///     Gets or sets the zoom ratio to counteract when drawing
    /// </summary>
    public double ZoomRatio
    {
        get => GetValue(ZoomRatioProperty);
        set => SetValue(ZoomRatioProperty, value);
    }

    /// <summary>
    ///     Gets a boolean indicating whether the selection rectangle is currently performing a selection.
    /// </summary>
    public bool IsSelecting
    {
        get => _isSelecting;
        private set => SetAndRaise(IsSelectingProperty, ref _isSelecting, value);
    }

    /// <summary>
    ///     Occurs when the selection rect is being updated, indicating the user is dragging.
    /// </summary>
    public event EventHandler<SelectionRectangleEventArgs>? SelectionUpdated;

    /// <summary>
    ///     Occurs when the selection has finished, indicating the user stopped dragging.
    /// </summary>
    public event EventHandler<SelectionRectangleEventArgs>? SelectionFinished;

    /// <summary>
    ///     Invokes the <see cref="SelectionUpdated" /> event
    /// </summary>
    /// <param name="e"></param>
    protected virtual void OnSelectionUpdated(SelectionRectangleEventArgs e)
    {
        SelectionUpdated?.Invoke(this, e);
    }

    /// <summary>
    ///     Invokes the <see cref="SelectionFinished" /> event
    /// </summary>
    /// <param name="e"></param>
    protected virtual void OnSelectionFinished(SelectionRectangleEventArgs e)
    {
        SelectionFinished?.Invoke(this, e);
    }

    private static void OnInputElementChanged(IAvaloniaObject sender, bool before)
    {
        ((SelectionRectangle) sender).SubscribeToInputElement();
    }
    
    private void ParentOnPointerMoved(object? sender, PointerEventArgs e)
    {
        // Point moved seems to trigger when the element under the mouse changes?
        // I'm not sure why this is needed but this check makes sure the position really hasn't changed.
        Point position = e.GetCurrentPoint(null).Position;
        if (position == _lastPosition)
            return;

        _lastPosition = position;

        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;
        
        // Capture the pointer and initialize dragging the first time it moves 
        if (!ReferenceEquals(e.Pointer.Captured, this))
        {
            e.Pointer.Capture(this);

            _startPosition = e.GetPosition(Parent);
            _absoluteStartPosition = e.GetPosition(VisualRoot);
            _displayRect = null;
        }

        Point currentPosition = e.GetPosition(Parent);
        Point absoluteCurrentPosition = e.GetPosition(VisualRoot);

        _displayRect = new Rect(
            new Point(Math.Min(_startPosition.X, currentPosition.X), Math.Min(_startPosition.Y, currentPosition.Y)),
            new Point(Math.Max(_startPosition.X, currentPosition.X), Math.Max(_startPosition.Y, currentPosition.Y))
        );
        _absoluteRect = new Rect(
            new Point(Math.Min(_absoluteStartPosition.X, absoluteCurrentPosition.X), Math.Min(_absoluteStartPosition.Y, absoluteCurrentPosition.Y)),
            new Point(Math.Max(_absoluteStartPosition.X, absoluteCurrentPosition.X), Math.Max(_absoluteStartPosition.Y, absoluteCurrentPosition.Y))
        );

        OnSelectionUpdated(new SelectionRectangleEventArgs(_displayRect.Value, _absoluteRect.Value, e.KeyModifiers));
        InvalidateVisual();
        IsSelecting = true;
    }

    private void ParentOnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!ReferenceEquals(e.Pointer.Captured, this))
            return;

        e.Pointer.Capture(null);

        if (_displayRect != null && _absoluteRect != null)
        {
            OnSelectionFinished(new SelectionRectangleEventArgs(_displayRect.Value, _absoluteRect.Value, e.KeyModifiers));
            e.Handled = true;
        }

        _displayRect = null;
        InvalidateVisual();
        IsSelecting = false;
    }

    private void SubscribeToInputElement()
    {
        if (_oldInputElement != null)
        {
            _oldInputElement.PointerMoved -= ParentOnPointerMoved;
            _oldInputElement.PointerReleased -= ParentOnPointerReleased;
        }

        _oldInputElement = InputElement;

        if (InputElement != null)
        {
            InputElement.PointerMoved += ParentOnPointerMoved;
            InputElement.PointerReleased += ParentOnPointerReleased;
        }
    }

    #region Overrides of Visual

    /// <inheritdoc />
    public override void Render(DrawingContext drawingContext)
    {
        if (_displayRect != null)
            drawingContext.DrawRectangle(Background, new Pen(BorderBrush, BorderThickness / ZoomRatio), _displayRect.Value, BorderRadius / ZoomRatio, BorderRadius / ZoomRatio);
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        SubscribeToInputElement();
        base.OnAttachedToVisualTree(e);
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_oldInputElement != null)
        {
            _oldInputElement.PointerMoved -= ParentOnPointerMoved;
            _oldInputElement.PointerReleased -= ParentOnPointerReleased;
            _oldInputElement = null;
        }

        base.OnDetachedFromVisualTree(e);
    }

    #endregion
}
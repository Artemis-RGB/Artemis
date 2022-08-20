using System;
using Artemis.Core;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace Artemis.UI.Shared.Controls.GradientPicker;

internal class GradientPickerColorStop : TemplatedControl
{
    private static ColorGradientStop? _draggingStop;
    private static IPointer? _dragPointer;

    /// <summary>
    ///     Gets or sets the gradient picker.
    /// </summary>
    public static readonly StyledProperty<GradientPicker?> GradientPickerProperty =
        AvaloniaProperty.Register<GradientPickerColorStop, GradientPicker?>(nameof(GradientPicker), notifying: Notifying);

    private static void Notifying(IAvaloniaObject sender, bool before)
    {
        if (sender is not GradientPickerColorStop self)
            return;

        if (before && self.GradientPicker != null)
            self.GradientPicker.PropertyChanged -= self.GradientPickerOnPropertyChanged;
        else if (self.GradientPicker != null)
            self.GradientPicker.PropertyChanged += self.GradientPickerOnPropertyChanged;

        self.IsSelected = ReferenceEquals(self.GradientPicker?.SelectedColorStop, self.ColorStop);
    }

    /// <summary>
    ///     Gets or sets the color stop.
    /// </summary>
    public static readonly StyledProperty<ColorGradientStop> ColorStopProperty =
        AvaloniaProperty.Register<GradientPickerColorStop, ColorGradientStop>(nameof(ColorStop));

    /// <summary>
    ///     Gets or sets the position reference to use when positioning and dragging this color stop.
    ///     <para>If <see langword="null" /> then dragging is not enabled.</para>
    /// </summary>
    public static readonly StyledProperty<IControl?> PositionReferenceProperty =
        AvaloniaProperty.Register<GradientPickerColorStop, IControl?>(nameof(PositionReference));

    /// <summary>
    ///     Gets the linear gradient brush representing the color gradient.
    /// </summary>
    public static readonly DirectProperty<GradientPickerColorStop, bool> IsSelectedProperty =
        AvaloniaProperty.RegisterDirect<GradientPickerColorStop, bool>(nameof(IsSelected), g => g.IsSelected);

    private bool _isSelected;
    private double _dragOffset;

    /// <summary>
    ///     Gets or sets the gradient picker.
    /// </summary>
    public GradientPicker? GradientPicker
    {
        get => GetValue(GradientPickerProperty);
        set => SetValue(GradientPickerProperty, value);
    }

    /// <summary>
    ///     Gets or sets the color stop.
    /// </summary>
    public ColorGradientStop ColorStop
    {
        get => GetValue(ColorStopProperty);
        set => SetValue(ColorStopProperty, value);
    }

    /// <summary>
    ///     Gets or sets the position reference to use when positioning and dragging this color stop.
    ///     <para>If <see langword="null" /> then dragging is not enabled.</para>
    /// </summary>
    public IControl? PositionReference
    {
        get => GetValue(PositionReferenceProperty);
        set => SetValue(PositionReferenceProperty, value);
    }

    /// <summary>
    ///     Gets the linear gradient brush representing the color gradient.
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        private set
        {
            SetAndRaise(IsSelectedProperty, ref _isSelected, value);
            if (IsSelected)
                PseudoClasses.Add(":selected");
            else
                PseudoClasses.Remove(":selected");
        }
    }

    private void GradientPickerOnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (GradientPicker != null && e.Property == GradientPicker.SelectedColorStopProperty)
        {
            IsSelected = GradientPicker.SelectedColorStop == ColorStop;
        }
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed || PositionReference == null)
            return;

        _dragOffset = e.GetCurrentPoint(PositionReference).Position.X - GetPixelPosition();
        e.Pointer.Capture(this);

        // Store these in case the control is being recreated due to an array resort
        // it's a bit ugly but it gives us a way to pick up dragging again with the new control
        _dragPointer = e.Pointer;
        _draggingStop = ColorStop;
        e.Handled = true;

        if (GradientPicker != null)
            GradientPicker.SelectedColorStop = ColorStop;
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed || !ReferenceEquals(e.Pointer.Captured, this) || PositionReference == null)
        {
            if (!Equals(_draggingStop, ColorStop))
                return;

            _dragOffset = e.GetCurrentPoint(PositionReference).Position.X - GetPixelPosition();
        }

        double position = e.GetCurrentPoint(PositionReference).Position.X - _dragOffset;
        ColorStop.Position = MathF.Round((float) Math.Clamp(position / PositionReference?.Bounds.Width ?? 0, 0, 1), 3, MidpointRounding.AwayFromZero);
        e.Handled = true;
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton != MouseButton.Left)
            return;

        e.Pointer.Capture(null);
        e.Handled = true;
        _draggingStop = null;
    }

    private double GetPixelPosition()
    {
        if (PositionReference == null)
            return 0;

        return PositionReference.Bounds.Width * ColorStop.Position;
    }

    #region Overrides of Visual

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (GradientPicker != null)
            GradientPicker.PropertyChanged += GradientPickerOnPropertyChanged;

        if (PositionReference != null)
        {
            PointerPressed += OnPointerPressed;
            PointerMoved += OnPointerMoved;
            PointerReleased += OnPointerReleased;

            // If this stop was previously being dragged, carry on dragging again
            // This can happen if the control was recreated due to an array sort
            if (_draggingStop == ColorStop && _dragPointer != null)
            {
                _dragPointer.Capture(this);
                IsSelected = true;
            }
        }

        base.OnAttachedToVisualTree(e);
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (GradientPicker != null)
            GradientPicker.PropertyChanged -= GradientPickerOnPropertyChanged;
        PointerPressed -= OnPointerPressed;
        PointerMoved -= OnPointerMoved;
        PointerReleased -= OnPointerReleased;

        base.OnDetachedFromVisualTree(e);
    }

    #endregion
}
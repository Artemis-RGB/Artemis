using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;

namespace Artemis.UI.Shared.Controls;

/// <summary>
///     Represents a number box that can be mutated by dragging over it horizontally
/// </summary>
public class DraggableNumberBox : UserControl
{
    /// <summary>
    ///     Defines the <see cref="Value" /> property.
    /// </summary>
    public static readonly StyledProperty<double> ValueProperty = AvaloniaProperty.Register<DraggableNumberBox, double>(nameof(Value), defaultBindingMode: BindingMode.TwoWay, notifying: ValueChanged);

    private static void ValueChanged(IAvaloniaObject sender, bool before)
    {
        if (before)
            return;

        DraggableNumberBox draggable = (DraggableNumberBox) sender;
        if (!(Math.Abs(draggable._numberBox.Value - draggable.Value) > 0.00001))
            return;

        draggable._updating = true;
        draggable._numberBox.Value = draggable.Value;
        draggable._updating = false;
    }

    /// <summary>
    ///     Defines the <see cref="Minimum" /> property.
    /// </summary>
    public static readonly StyledProperty<double> MinimumProperty = AvaloniaProperty.Register<DraggableNumberBox, double>(nameof(Minimum), double.MinValue);

    /// <summary>
    ///     Defines the <see cref="Maximum" /> property.
    /// </summary>
    public static readonly StyledProperty<double> MaximumProperty = AvaloniaProperty.Register<DraggableNumberBox, double>(nameof(Maximum), double.MaxValue);

    /// <summary>
    ///     Defines the <see cref="LargeChange" /> property.
    /// </summary>
    public static readonly StyledProperty<double> LargeChangeProperty = AvaloniaProperty.Register<DraggableNumberBox, double>(nameof(LargeChange));

    /// <summary>
    ///     Defines the <see cref="SmallChange" /> property.
    /// </summary>
    public static readonly StyledProperty<double> SmallChangeProperty = AvaloniaProperty.Register<DraggableNumberBox, double>(nameof(SmallChange));

    /// <summary>
    ///     Defines the <see cref="SimpleNumberFormat" /> property.
    /// </summary>
    public static readonly StyledProperty<string> SimpleNumberFormatProperty = AvaloniaProperty.Register<DraggableNumberBox, string>(nameof(SimpleNumberFormat));

    /// <summary>
    ///     Defines the <see cref="Prefix" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> PrefixProperty = AvaloniaProperty.Register<DraggableNumberBox, string?>(nameof(Prefix));

    /// <summary>
    ///     Defines the <see cref="Suffix" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> SuffixProperty = AvaloniaProperty.Register<DraggableNumberBox, string?>(nameof(Suffix));

    private readonly NumberBox _numberBox;
    private TextBox? _inputTextBox;
    private double _lastX;
    private bool _moved;
    private double _startX;
    private bool _updating;

    /// <summary>
    ///     Creates a new instance of the <see cref="DraggableNumberBox" /> class.
    /// </summary>
    public DraggableNumberBox()
    {
        InitializeComponent();
        _numberBox = this.Get<NumberBox>("NumberBox");
        _numberBox.Value = Value;

        PointerPressed += OnPointerPressed;
        PointerMoved += OnPointerMoved;
        PointerReleased += OnPointerReleased;

        AddHandler(KeyUpEvent, HandleKeyUp, RoutingStrategies.Direct | RoutingStrategies.Tunnel | RoutingStrategies.Bubble, true);
    }

    /// <summary>
    ///     Gets or sets the value of the number box.
    /// </summary>
    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>
    ///     Gets or sets the minimum of the number box.
    /// </summary>
    public double Minimum
    {
        get => GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    /// <summary>
    ///     Gets or sets the maximum of the number box.
    /// </summary>
    public double Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    /// <summary>
    ///     Gets or sets the amount with which to increase/decrease the value when dragging.
    /// </summary>
    public double LargeChange
    {
        get => GetValue(LargeChangeProperty);
        set => SetValue(LargeChangeProperty, value);
    }

    /// <summary>
    ///     Gets or sets the amount with which to increase/decrease the value when dragging and holding down shift.
    /// </summary>
    public double SmallChange
    {
        get => GetValue(SmallChangeProperty);
        set => SetValue(SmallChangeProperty, value);
    }

    /// <summary>
    ///     Gets or sets the number format string used to format the value into a display value.
    /// </summary>
    public string SimpleNumberFormat
    {
        get => GetValue(SimpleNumberFormatProperty);
        set => SetValue(SimpleNumberFormatProperty, value);
    }

    /// <summary>
    ///     Gets or sets the prefix to show before the value.
    /// </summary>
    public string? Prefix
    {
        get => GetValue(PrefixProperty);
        set => SetValue(PrefixProperty, value);
    }

    /// <summary>
    ///     Gets or sets the affix to show behind the value.
    /// </summary>
    public string? Suffix
    {
        get => GetValue(SuffixProperty);
        set => SetValue(SuffixProperty, value);
    }

    /// <summary>
    ///     Occurs when the user starts dragging over the control.
    /// </summary>
    public event TypedEventHandler<DraggableNumberBox, EventArgs>? DragStarted;

    /// <summary>
    ///     Occurs when the user finishes dragging over the control.
    /// </summary>
    public event TypedEventHandler<DraggableNumberBox, EventArgs>? DragFinished;

    private void HandleKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter || e.Key == Key.Escape)
            Parent?.Focus();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        PointerPoint point = e.GetCurrentPoint(this);
        _inputTextBox = _numberBox.FindDescendantOfType<TextBox>();
        _moved = false;
        _startX = point.Position.X;
        _lastX = point.Position.X;
        e.Handled = true;
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        PointerPoint point = e.GetCurrentPoint(this);
        if (!point.Properties.IsLeftButtonPressed || _inputTextBox == null || _inputTextBox.IsFocused)
            return;

        if (!_moved && Math.Abs(point.Position.X - _startX) < 2)
        {
            _lastX = point.Position.X;
            return;
        }

        if (!_moved)
        {
            // Let our parent take focus, it would make more sense to take focus ourselves but that hides the collider
            Parent?.Focus();
            _moved = true;
            e.Pointer.Capture(this);
            DragStarted?.Invoke(this, EventArgs.Empty);
        }

        double smallChange;
        if (SmallChange != 0)
            smallChange = SmallChange;
        else if (LargeChange != 0)
            smallChange = LargeChange / 10;
        else
            smallChange = 0.1;

        double largeChange;
        if (LargeChange != 0)
            largeChange = LargeChange;
        else if (LargeChange != 0)
            largeChange = LargeChange * 10;
        else
            largeChange = 1;

        double changeMultiplier = e.KeyModifiers.HasFlag(KeyModifiers.Shift) ? smallChange : largeChange;
        Value = Math.Clamp(Value + (point.Position.X - _lastX) * changeMultiplier, Minimum, Maximum);
        _lastX = point.Position.X;
        e.Handled = true;
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_moved)
        {
            _inputTextBox?.Focus();
        }
        else
        {
            _moved = false;
            DragFinished?.Invoke(this, EventArgs.Empty);
        }

        e.Handled = true;
    }

    private void NumberBox_OnValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        if (_updating)
            return;

        if (args.NewValue < Minimum)
        {
            _numberBox.Value = Minimum;
            return;
        }

        if (args.NewValue > Maximum)
        {
            _numberBox.Value = Maximum;
            return;
        }

        if (Math.Abs(Value - _numberBox.Value) > 0.00001)
            Value = _numberBox.Value;
    }
}
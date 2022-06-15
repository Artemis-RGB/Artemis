using System;
using System.Diagnostics;
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

public partial class DraggableNumberBox : UserControl
{
    /// <summary>
    ///    Gets or sets the value of the number box.
    /// </summary>
    public static readonly StyledProperty<double> ValueProperty = AvaloniaProperty.Register<DraggableNumberBox, double>(nameof(Value), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    ///     Gets or sets the value of the number box.
    /// </summary>
    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>
    ///    Gets or sets the minimum of the number box.
    /// </summary>
    public static readonly StyledProperty<double> MinimumProperty = AvaloniaProperty.Register<DraggableNumberBox, double>(nameof(Minimum), double.MinValue);

    /// <summary>
    ///     Gets or sets the minimum of the number box.
    /// </summary>
    public double Minimum
    {
        get => GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    /// <summary>
    ///    Gets or sets the maximum of the number box.
    /// </summary>
    public static readonly StyledProperty<double> MaximumProperty = AvaloniaProperty.Register<DraggableNumberBox, double>(nameof(Maximum), double.MaxValue);

    /// <summary>
    ///     Gets or sets the maximum of the number box.
    /// </summary>
    public double Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public static readonly StyledProperty<double> LargeChangeProperty = AvaloniaProperty.Register<DraggableNumberBox, double>(nameof(LargeChange));

    public double LargeChange
    {
        get => GetValue(LargeChangeProperty);
        set => SetValue(LargeChangeProperty, value);
    }

    public static readonly StyledProperty<double> SmallChangeProperty = AvaloniaProperty.Register<DraggableNumberBox, double>(nameof(SmallChange));

    public double SmallChange
    {
        get => GetValue(SmallChangeProperty);
        set => SetValue(SmallChangeProperty, value);
    }

    public static readonly StyledProperty<string> SimpleNumberFormatProperty = AvaloniaProperty.Register<DraggableNumberBox, string>(nameof(SimpleNumberFormat));

    public string SimpleNumberFormat
    {
        get => GetValue(SimpleNumberFormatProperty);
        set => SetValue(SimpleNumberFormatProperty, value);
    }

    public static readonly StyledProperty<string> PrefixProperty = AvaloniaProperty.Register<DraggableNumberBox, string>(nameof(Prefix));

    public string Prefix
    {
        get => GetValue(PrefixProperty);
        set => SetValue(PrefixProperty, value);
    }

    public static readonly StyledProperty<string> SuffixProperty = AvaloniaProperty.Register<DraggableNumberBox, string>(nameof(Suffix));

    public string Suffix
    {
        get => GetValue(SuffixProperty);
        set => SetValue(SuffixProperty, value);
    }

    public event TypedEventHandler<DraggableNumberBox, EventArgs>? DragStarted;
    public event TypedEventHandler<DraggableNumberBox, EventArgs>? DragFinished;

    private readonly NumberBox _numberBox;
    private TextBox? _inputTextBox;
    private bool _moved;
    private double _lastX;
    private double _startX;

    public DraggableNumberBox()
    {
        InitializeComponent();
        _numberBox = this.Get<NumberBox>("NumberBox");

        PointerPressed += OnPointerPressed;
        PointerMoved += OnPointerMoved;
        PointerReleased += OnPointerReleased;

        AddHandler(KeyUpEvent, HandleKeyUp, RoutingStrategies.Direct | RoutingStrategies.Tunnel | RoutingStrategies.Bubble, true);
    }

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
            _inputTextBox?.Focus();
        else
        {
            _moved = false;
            DragFinished?.Invoke(this, EventArgs.Empty);
        }

        e.Handled = true;
    }
}
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;

namespace Artemis.UI.Shared.Behaviors;

/// <summary>
///     Represents a behavior that can be used to make a slider only update it's binding on pointer release.
/// </summary>
public class SliderPointerReleasedBindingBehavior : Behavior<Slider>
{
    /// <summary>
    ///     Gets or sets the value of the binding.
    /// </summary>
    public static readonly StyledProperty<double> ValueProperty = AvaloniaProperty.Register<LostFocusTextBoxBindingBehavior, double>(
        nameof(Value), defaultBindingMode: BindingMode.TwoWay);

    static SliderPointerReleasedBindingBehavior()
    {
        ValueProperty.Changed.Subscribe(e => ((SliderPointerReleasedBindingBehavior) e.Sender).OnBindingValueChanged());
    }

    /// <summary>
    ///     Gets or sets the value of the binding.
    /// </summary>
    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <inheritdoc />
    protected override void OnAttached()
    {
        if (AssociatedObject != null)
            AssociatedObject.PointerCaptureLost += OnPointerCaptureLost;
        base.OnAttached();
    }

    /// <inheritdoc />
    protected override void OnDetaching()
    {
        if (AssociatedObject != null)
            AssociatedObject.PointerCaptureLost -= OnPointerCaptureLost;
        base.OnDetaching();
    }

    private void OnPointerCaptureLost(object? sender, RoutedEventArgs e)
    {
        if (AssociatedObject != null)
            Value = AssociatedObject.Value;
    }

    private void OnBindingValueChanged()
    {
        if (AssociatedObject != null)
            AssociatedObject.Value = Value;
    }
}
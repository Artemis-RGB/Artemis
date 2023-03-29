using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;

namespace Artemis.UI.Shared.Behaviors;

/// <summary>
///     Represents a behavior that can be used to make a numeric up down only update it's binding on focus loss.
/// </summary>
public class LostFocusNumericUpDownBindingBehavior : Behavior<NumericUpDown>
{
    /// <summary>
    ///     Gets or sets the value of the binding.
    /// </summary>
    public static readonly StyledProperty<decimal?> ValueProperty = AvaloniaProperty.Register<LostFocusNumericUpDownBindingBehavior, decimal?>(
        nameof(Value), defaultBindingMode: BindingMode.TwoWay);

    static LostFocusNumericUpDownBindingBehavior()
    {
        ValueProperty.Changed.Subscribe(e => ((LostFocusNumericUpDownBindingBehavior) e.Sender).OnBindingValueChanged());
    }

    /// <summary>
    ///     Gets or sets the value of the binding.
    /// </summary>
    public decimal? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <inheritdoc />
    protected override void OnAttached()
    {
        if (AssociatedObject != null)
            AssociatedObject.LostFocus += OnLostFocus;
        base.OnAttached();
    }

    /// <inheritdoc />
    protected override void OnDetaching()
    {
        if (AssociatedObject != null)
            AssociatedObject.LostFocus -= OnLostFocus;
        base.OnDetaching();
    }

    private void OnLostFocus(object? sender, RoutedEventArgs e)
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
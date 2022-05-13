using System;
using Avalonia;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using FluentAvalonia.UI.Controls;

namespace Artemis.UI.Shared.Behaviors;

/// <summary>
///     Represents a behavior that can be used to make a text box only update it's binding on focus loss.
/// </summary>
public class LostFocusNumberBoxBindingBehavior : Behavior<NumberBox>
{
    /// <summary>
    /// Gets or sets the value of the binding.
    /// </summary>
    public static readonly StyledProperty<double> ValueProperty = AvaloniaProperty.Register<LostFocusTextBoxBindingBehavior, double>(
        nameof(Value), defaultBindingMode: BindingMode.TwoWay);

    static LostFocusNumberBoxBindingBehavior()
    {
        ValueProperty.Changed.Subscribe(e => ((LostFocusNumberBoxBindingBehavior) e.Sender).OnBindingValueChanged());
    }

    /// <summary>
    /// Gets or sets the value of the binding.
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
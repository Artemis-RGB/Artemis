using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;

namespace Artemis.UI.Shared.Behaviors;

/// <summary>
///     Represents a behavior that can be used to make a text box only update it's binding on focus loss.
/// </summary>
public class LostFocusTextBoxBindingBehavior : Behavior<TextBox>
{
    /// <summary>
    ///     Gets or sets the value of the binding.
    /// </summary>
    public static readonly StyledProperty<string?> TextProperty = AvaloniaProperty.Register<LostFocusTextBoxBindingBehavior, string?>(
        "Text", defaultBindingMode: BindingMode.TwoWay);

    static LostFocusTextBoxBindingBehavior()
    {
        TextProperty.Changed.Subscribe(e => ((LostFocusTextBoxBindingBehavior) e.Sender).OnBindingValueChanged());
    }

    /// <summary>
    ///     Gets or sets the value of the binding.
    /// </summary>
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
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
            Text = AssociatedObject.Text;
    }

    private void OnBindingValueChanged()
    {
        if (AssociatedObject != null)
            AssociatedObject.Text = Text;
    }
}
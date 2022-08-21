using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;

namespace Artemis.UI.Shared.AttachedProperties;

/// <summary>
///     Helper properties for working with TextBoxes.
/// </summary>
public class TextBoxAssist : AvaloniaObject
{
    /// <summary>
    ///     Identifies the <seealso cref="SuffixTextProperty" /> Avalonia attached property.
    /// </summary>
    /// <value>Provide an <see cref="ICommand" /> derived object or binding.</value>
    public static readonly AttachedProperty<string> SuffixTextProperty = AvaloniaProperty.RegisterAttached<TextBoxAssist, string>("SuffixText", typeof(TextBox));

    /// <summary>
    ///     Identifies the <seealso cref="PrefixTextProperty" /> Avalonia attached property.
    /// </summary>
    /// <value>Provide an <see cref="ICommand" /> derived object or binding.</value>
    public static readonly AttachedProperty<string> PrefixTextProperty = AvaloniaProperty.RegisterAttached<TextBoxAssist, string>("PrefixText", typeof(TextBox));

    /// <summary>
    ///     Accessor for Attached property <see cref="SuffixTextProperty" />.
    /// </summary>
    public static void SetSuffixText(AvaloniaObject element, string value)
    {
        element.SetValue(SuffixTextProperty, value);

        if (!string.IsNullOrWhiteSpace(value) && !((TextBox) element).Classes.Contains("suffixed"))
            ((TextBox) element).Classes.Add("suffixed");
        else
            ((TextBox) element).Classes.Remove("suffixed");
    }

    /// <summary>
    ///     Accessor for Attached property <see cref="SuffixTextProperty" />.
    /// </summary>
    public static string GetSuffixText(AvaloniaObject element)
    {
        return element.GetValue(SuffixTextProperty);
    }

    /// <summary>
    ///     Accessor for Attached property <see cref="PrefixTextProperty" />.
    /// </summary>
    public static void SetPrefixText(AvaloniaObject element, string value)
    {
        element.SetValue(PrefixTextProperty, value);

        if (!string.IsNullOrWhiteSpace(value) && !((TextBox) element).Classes.Contains("prefixed"))
            ((TextBox) element).Classes.Add("prefixed");
        else
            ((TextBox) element).Classes.Remove("prefixed");
    }

    /// <summary>
    ///     Accessor for Attached property <see cref="PrefixTextProperty" />.
    /// </summary>
    public static string GetPrefixText(AvaloniaObject element)
    {
        return element.GetValue(PrefixTextProperty);
    }
}
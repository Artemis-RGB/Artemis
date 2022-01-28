using System.Windows.Input;
using Avalonia;
using FluentAvalonia.UI.Controls;

namespace Artemis.UI.Shared.AttachedProperties;

/// <summary>
///     Helper properties for working with NumberBoxes.
/// </summary>
public class NumberBoxAssist : AvaloniaObject
{
    /// <summary>
    ///     Identifies the <seealso cref="SuffixTextProperty" /> Avalonia attached property.
    /// </summary>
    /// <value>Provide an <see cref="ICommand" /> derived object or binding.</value>
    public static readonly AttachedProperty<string> SuffixTextProperty = AvaloniaProperty.RegisterAttached<NumberBoxAssist, string>("SuffixText", typeof(NumberBox));

    /// <summary>
    ///     Identifies the <seealso cref="PrefixTextProperty" /> Avalonia attached property.
    /// </summary>
    /// <value>Provide an <see cref="ICommand" /> derived object or binding.</value>
    public static readonly AttachedProperty<string> PrefixTextProperty = AvaloniaProperty.RegisterAttached<NumberBoxAssist, string>("PrefixText", typeof(NumberBox));

    /// <summary>
    ///     Accessor for Attached property <see cref="SuffixTextProperty" />.
    /// </summary>
    public static void SetSuffixText(AvaloniaObject element, string value)
    {
        element.SetValue(SuffixTextProperty, value);
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
    }

    /// <summary>
    ///     Accessor for Attached property <see cref="PrefixTextProperty" />.
    /// </summary>
    public static string GetPrefixText(AvaloniaObject element)
    {
        return element.GetValue(PrefixTextProperty);
    }
}
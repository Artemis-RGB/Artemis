using System;
using System.Collections.Specialized;

namespace Artemis.Core;

/// <summary>
/// Represents a configuration item that accepts ColorGradient input from the user.
/// </summary>
public class ConfigurationColorGradientItem : ConfigurationInputItem<ColorGradient>
{
    /// <summary>
    ///     Creates a new instance of the <see cref="ConfigurationColorGradientItem" /> class.
    /// </summary>
    public ConfigurationColorGradientItem()
    {
        // The gradient picker doesn't handle null values well
        DefaultValue = ColorGradient.GetUnicornBarf();
        Value = ColorGradient.GetUnicornBarf();
    }

    /// <inheritdoc />
    protected override ColorGradient DataModelFallbackValue => new();

    /// <inheritdoc />
    protected override bool IsSameValue(ColorGradient? a, ColorGradient? b)
    {
        // Gradients are mutable and compared by value, a different instance should always be treated as a change
        return ReferenceEquals(a, b);
    }

    /// <inheritdoc />
    protected override ColorGradient? CopyValue(ColorGradient? value)
    {
        return value != null ? new ColorGradient(value) : null;
    }

    /// <inheritdoc />
    protected override void OnValueReplaced(ColorGradient? previous, ColorGradient? current)
    {
        // Gradients are edited in-place, forward those edits as a change of the property
        if (previous != null)
        {
            previous.CollectionChanged -= GradientOnCollectionChanged;
            previous.StopChanged -= GradientOnStopChanged;
        }

        if (current != null)
        {
            current.CollectionChanged += GradientOnCollectionChanged;
            current.StopChanged += GradientOnStopChanged;
        }
    }

    private void GradientOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnGradientEdited(sender);
    }

    private void GradientOnStopChanged(object? sender, EventArgs e)
    {
        OnGradientEdited(sender);
    }

    private void OnGradientEdited(object? sender)
    {
        if (ReferenceEquals(sender, Value))
            OnPropertyChanged(nameof(Value));
        if (ReferenceEquals(sender, DefaultValue))
            OnPropertyChanged(nameof(DefaultValue));
    }
}
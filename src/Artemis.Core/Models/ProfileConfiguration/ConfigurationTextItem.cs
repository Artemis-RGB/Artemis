using System;

namespace Artemis.Core;

/// <summary>
/// Represents a configuration item that displays static text.
/// </summary>
public class ConfigurationTextItem : CorePropertyChanged, IConfigurationItem
{
    /// <inheritdoc />
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the text content to display.
    /// </summary>
    public required string Text
    {
        get;
        set => SetAndNotify(ref field, value);
    }
}

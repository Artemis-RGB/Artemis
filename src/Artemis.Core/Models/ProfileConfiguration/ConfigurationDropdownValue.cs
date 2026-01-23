namespace Artemis.Core;

/// <summary>
/// Represents a single option in a configuration dropdown list.
/// </summary>
/// <typeparam name="T">The type of the value that this dropdown option represents.</typeparam>
public class ConfigurationDropdownValue<T> : CorePropertyChanged
{
    /// <summary>
    /// Gets or sets the display name shown to the user for this dropdown option.
    /// </summary>
    public required string DisplayName
    {
        get;
        set => SetAndNotify(ref field, value);
    }

    /// <summary>
    /// Gets or sets the actual value associated with this dropdown option.
    /// </summary>
    public required T Value
    {
        get;
        set => SetAndNotify(ref field, value);
    }
}
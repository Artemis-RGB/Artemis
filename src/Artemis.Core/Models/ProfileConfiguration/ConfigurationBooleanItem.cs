namespace Artemis.Core;

/// <summary>
/// Represents a configuration item that accepts boolean input from the user.
/// </summary>
public class ConfigurationBooleanItem : ConfigurationInputItem<bool>
{
    /// <summary>
    /// Gets or sets the display text shown when the boolean value is true.
    /// </summary>
    public required string TrueText
    {
        get;
        set => SetAndNotify(ref field, value);
    }

    /// <summary>
    /// Gets or sets the display text shown when the boolean value is false.
    /// </summary>
    public required string FalseText
    {
        get;
        set => SetAndNotify(ref field, value);
    }
}
namespace Artemis.Core;

/// <summary>
///     Represents a configuration item that accepts boolean input from the user.
/// </summary>
public class ConfigurationBooleanItem : ConfigurationInputItem<bool>
{
    /// <summary>
    ///     Gets or sets the display text shown when the boolean value is true.
    /// </summary>
    public string TrueText
    {
        get;
        set => SetAndNotify(ref field, value);
    } = "Yes";

    /// <summary>
    ///     Gets or sets the display text shown when the boolean value is false.
    /// </summary>
    public string FalseText
    {
        get;
        set => SetAndNotify(ref field, value);
    } = "No";
}
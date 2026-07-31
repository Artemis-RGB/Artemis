namespace Artemis.Core;

/// <summary>
///     Represents a configuration item that accepts numeric input from the user.
/// </summary>
public class ConfigurationNumericItem : ConfigurationInputItem<Numeric>
{
    public Numeric? Minimum
    {
        get;
        set => SetAndNotify(ref field, value);
    }

    public Numeric? Maximum
    {
        get;
        set => SetAndNotify(ref field, value);
    }

    public bool Slider
    {
        get;
        set => SetAndNotify(ref field, value);
    }
}
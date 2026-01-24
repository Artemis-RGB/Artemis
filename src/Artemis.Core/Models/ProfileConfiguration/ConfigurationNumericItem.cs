namespace Artemis.Core;

/// <summary>
/// Represents a configuration item that accepts numeric input from the user.
/// </summary>
public class ConfigurationNumericItem : ConfigurationInputItem<Numeric>
{
    public Numeric? Minimum { get; set; }
    public Numeric? Maximum { get; set; }
    public bool Slider { get; set; }
}
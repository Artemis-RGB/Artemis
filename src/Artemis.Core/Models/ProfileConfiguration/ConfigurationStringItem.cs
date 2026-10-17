namespace Artemis.Core;

/// <summary>
/// Represents a configuration item that accepts string input from the user.
/// </summary>
public class ConfigurationStringItem : ConfigurationInputItem<string>
{
    /// <inheritdoc />
    protected override string DataModelFallbackValue => string.Empty;
}
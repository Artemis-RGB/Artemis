using Artemis.Core;

namespace Artemis.UI.Screens.ProfileEditor.DesignPanels.Items;

public class ConfigurationStringItemDesignViewModel(ConfigurationStringItem item, ConfigurationSection configurationSection)
    : ConfigurationInputItemDesignViewModel<ConfigurationStringItem, string>(item, configurationSection)
{
    public ConfigurationStringItem StringItem { get; } = item;

    /// <inheritdoc />
    protected override string CreateDropdownValue(int number)
    {
        return $"Option {number}";
    }
}

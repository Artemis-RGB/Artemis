using Artemis.Core;

namespace Artemis.UI.Screens.ProfileEditor.DesignPanels.Items;

public class ConfigurationSKColorItemDesignViewModel(ConfigurationSKColorItem item, ConfigurationSection configurationSection) : ConfigurationItemDesignViewModel(item, configurationSection)
{
    public ConfigurationSKColorItem ColorItem { get; } = item;
}
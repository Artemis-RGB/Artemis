using Artemis.Core;

namespace Artemis.UI.Screens.ProfileEditor.DesignPanels.Items;

public class ConfigurationNumericItemDesignViewModel(ConfigurationNumericItem item, ConfigurationSection configurationSection) : ConfigurationItemDesignViewModel(item, configurationSection)
{
    public ConfigurationNumericItem NumericItem { get; } = item;
}
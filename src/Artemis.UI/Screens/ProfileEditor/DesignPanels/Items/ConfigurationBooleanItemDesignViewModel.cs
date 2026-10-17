using Artemis.Core;

namespace Artemis.UI.Screens.ProfileEditor.DesignPanels.Items;

public class ConfigurationBooleanItemDesignViewModel(ConfigurationBooleanItem item, ConfigurationSection configurationSection)
    : ConfigurationInputItemDesignViewModel<ConfigurationBooleanItem, bool>(item, configurationSection)
{
    public ConfigurationBooleanItem BooleanItem { get; } = item;
}

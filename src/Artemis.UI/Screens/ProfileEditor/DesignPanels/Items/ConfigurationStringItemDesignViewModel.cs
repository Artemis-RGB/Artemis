using Artemis.Core;

namespace Artemis.UI.Screens.ProfileEditor.DesignPanels.Items;

public class ConfigurationStringItemDesignViewModel(ConfigurationStringItem item, ConfigurationSection configurationSection) : ConfigurationItemDesignViewModel(item, configurationSection)
{
    public ConfigurationStringItem StringItem { get; } = item;
}
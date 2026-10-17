using Artemis.Core;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.ProfileEditor.DesignPanels.Items;

public class ConfigurationTextItemDesignViewModel(ConfigurationTextItem item, ConfigurationSection configurationSection) : ConfigurationItemDesignViewModel(item, configurationSection)
{
    public ConfigurationTextItem TextItem { get; } = item;
}

public class ConfigurationItemDesignViewModel : ActivatableViewModelBase
{
    protected ConfigurationItemDesignViewModel(IConfigurationItem item, ConfigurationSection configurationSection)
    {
        Item = item;
        ConfigurationSection = configurationSection;
    }

    public IConfigurationItem Item { get; }
    public ConfigurationSection ConfigurationSection { get; }

    public void RemoveItem()
    {
        ConfigurationSection.Items.Remove(Item);
    }
}
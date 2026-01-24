using System.Collections.ObjectModel;
using Artemis.Core;
using Artemis.UI.Screens.ProfileEditor.DesignPanels.Items;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.ProfileEditor.DesignPanels.Section;

public class ConfigurationSectionDesignViewModel : ActivatableViewModelBase
{
    public ConfigurationSection ConfigurationSection { get; }
    public ObservableCollection<ActivatableViewModelBase> Items { get; }

    public ConfigurationSectionDesignViewModel(ConfigurationSection configurationSection)
    {
        ConfigurationSection = configurationSection;
        Items = [];
        foreach (IConfigurationItem item in ConfigurationSection.Items)
        {
            if (item is ConfigurationTextItem textItem)
                Items.Add(new ConfigurationTextItemDesignViewModel(textItem));
            else if (item is ConfigurationStringItem stringItem)
                Items.Add(new ConfigurationStringItemDesignViewModel(stringItem));
            else if (item is ConfigurationBooleanItem booleanItem)
                Items.Add(new ConfigurationBooleanItemDesignViewModel(booleanItem));
            else if (item is ConfigurationNumericItem numberItem)
                Items.Add(new ConfigurationNumericItemDesignViewModel(numberItem));
            else if (item is ConfigurationSKColorItem colorItem)
                Items.Add(new ConfigurationSKColorItemDesignViewModel(colorItem));
            else if (item is ConfigurationColorGradientItem colorGradientItem)
                Items.Add(new ConfigurationColorGradientItemDesignViewModel(colorGradientItem));
        }
    }
}
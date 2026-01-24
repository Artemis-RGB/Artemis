using System.Collections.ObjectModel;
using Artemis.Core;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.ProfileEditor.ConfigurationPanels.Section;

public class ConfigurationSectionViewModel : ActivatableViewModelBase
{
    public ConfigurationSection ConfigurationSection { get; }
    public ObservableCollection<ActivatableViewModelBase> Items { get; }

    public ConfigurationSectionViewModel(ConfigurationSection configurationSection)
    {
        ConfigurationSection = configurationSection;
        Items = [];
        foreach (IConfigurationItem item in ConfigurationSection.Items)
        {
            if (item is ConfigurationTextItem textItem)
                Items.Add(new ConfigurationTextItemViewModel(textItem));
            else if (item is ConfigurationStringItem stringItem)
                Items.Add(new ConfigurationStringItemViewModel(stringItem));
            else if (item is ConfigurationBooleanItem booleanItem)
                Items.Add(new ConfigurationBooleanItemViewModel(booleanItem));
            else if (item is ConfigurationNumericItem numberItem)
                Items.Add(new ConfigurationNumericItemViewModel(numberItem));
            else if (item is ConfigurationSKColorItem colorItem)
                Items.Add(new ConfigurationSKColorItemViewModel(colorItem));
            else if (item is ConfigurationColorGradientItem colorGradientItem)
                Items.Add(new ConfigurationColorGradientItemViewModel(colorGradientItem));
        }
    }
}
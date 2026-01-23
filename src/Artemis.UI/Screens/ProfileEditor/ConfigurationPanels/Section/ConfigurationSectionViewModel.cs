using Artemis.Core;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.ProfileEditor.ConfigurationPanels.Section;

public class ConfigurationSectionViewModel : ActivatableViewModelBase
{
    public ConfigurationSection ConfigurationSection { get; }

    public ConfigurationSectionViewModel(ConfigurationSection configurationSection)
    {
        ConfigurationSection = configurationSection;
    }
}
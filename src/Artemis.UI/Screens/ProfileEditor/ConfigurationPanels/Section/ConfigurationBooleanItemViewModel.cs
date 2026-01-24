using Artemis.Core;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.ProfileEditor.ConfigurationPanels.Section;

public class ConfigurationBooleanItemViewModel : ActivatableViewModelBase
{
    public ConfigurationBooleanItem Item { get; }

    public ConfigurationBooleanItemViewModel(ConfigurationBooleanItem item)
    {
        Item = item;
    }
}
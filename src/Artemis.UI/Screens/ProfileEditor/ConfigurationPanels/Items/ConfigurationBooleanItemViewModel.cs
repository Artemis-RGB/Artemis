using Artemis.Core;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.ProfileEditor.ConfigurationPanels.Items;

public class ConfigurationBooleanItemViewModel : ActivatableViewModelBase
{
    public ConfigurationBooleanItemViewModel(ConfigurationBooleanItem item)
    {
        Item = item;
    }

    public ConfigurationBooleanItem Item { get; }
}
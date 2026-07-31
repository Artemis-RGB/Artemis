using Artemis.Core;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.ProfileEditor.ConfigurationPanels.Items;

public class ConfigurationSKColorItemViewModel : ActivatableViewModelBase
{
    public ConfigurationSKColorItemViewModel(ConfigurationSKColorItem item)
    {
        Item = item;
    }

    public ConfigurationSKColorItem Item { get; }
}
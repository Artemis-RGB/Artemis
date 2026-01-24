using Artemis.Core;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.ProfileEditor.DesignPanels.Items;

public class ConfigurationSKColorItemDesignViewModel : ActivatableViewModelBase
{
    public ConfigurationSKColorItemDesignViewModel(ConfigurationSKColorItem item)
    {
        Item = item;
    }

    public ConfigurationSKColorItem Item { get; }
}
using Artemis.Core;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.ProfileEditor.DesignPanels.Items;

public class ConfigurationBooleanItemDesignViewModel : ActivatableViewModelBase
{
    public ConfigurationBooleanItem Item { get; }

    public ConfigurationBooleanItemDesignViewModel(ConfigurationBooleanItem item)
    {
        Item = item;
    }
}
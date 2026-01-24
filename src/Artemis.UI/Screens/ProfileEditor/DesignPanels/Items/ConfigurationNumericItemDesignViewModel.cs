using Artemis.Core;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.ProfileEditor.DesignPanels.Items;

public class ConfigurationNumericItemDesignViewModel : ActivatableViewModelBase
{
    public ConfigurationNumericItem Item { get; }

    public ConfigurationNumericItemDesignViewModel(ConfigurationNumericItem item)
    {
        Item = item;
    }
}
using Artemis.Core;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.ProfileEditor.DesignPanels.Items;

public class ConfigurationTextItemDesignViewModel : ActivatableViewModelBase
{
    public ConfigurationTextItem Item { get; }

    public ConfigurationTextItemDesignViewModel(ConfigurationTextItem item)
    {
        Item = item;
    }
}
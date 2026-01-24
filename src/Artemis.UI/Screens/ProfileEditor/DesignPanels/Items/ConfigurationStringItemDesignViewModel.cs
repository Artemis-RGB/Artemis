using Artemis.Core;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.ProfileEditor.DesignPanels.Items;

public class ConfigurationStringItemDesignViewModel : ActivatableViewModelBase
{
    public ConfigurationStringItem Item { get; }

    public ConfigurationStringItemDesignViewModel(ConfigurationStringItem item)
    {
        Item = item;
    }
}
using Artemis.Core;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.ProfileEditor.DesignPanels.Items;

public class ConfigurationColorGradientItemDesignViewModel : ActivatableViewModelBase
{
    public ConfigurationColorGradientItemDesignViewModel(ConfigurationColorGradientItem item)
    {
        Item = item;
    }

    public ConfigurationColorGradientItem Item { get; }
}
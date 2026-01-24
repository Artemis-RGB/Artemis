using Artemis.Core;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.ProfileEditor.ConfigurationPanels.Section;

public class ConfigurationColorGradientItemViewModel : ActivatableViewModelBase
{
    public ConfigurationColorGradientItemViewModel(ConfigurationColorGradientItem item)
    {
        Item = item;
    }

    public ConfigurationColorGradientItem Item { get; }
}
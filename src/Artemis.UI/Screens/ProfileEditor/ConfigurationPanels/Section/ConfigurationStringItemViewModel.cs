using Artemis.Core;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.ProfileEditor.ConfigurationPanels.Section;

public class ConfigurationStringItemViewModel : ActivatableViewModelBase
{
    public ConfigurationStringItem Item { get; }

    public ConfigurationStringItemViewModel(ConfigurationStringItem item)
    {
        Item = item;
    }
}
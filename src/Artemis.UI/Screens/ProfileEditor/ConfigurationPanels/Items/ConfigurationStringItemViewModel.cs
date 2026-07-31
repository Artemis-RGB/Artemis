using Artemis.Core;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.ProfileEditor.ConfigurationPanels.Items;

public class ConfigurationStringItemViewModel : ActivatableViewModelBase
{
    public ConfigurationStringItemViewModel(ConfigurationStringItem item)
    {
        Item = item;
    }

    public ConfigurationStringItem Item { get; }
}
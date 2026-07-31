using Artemis.Core;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.ProfileEditor.ConfigurationPanels.Items;

public class ConfigurationNumericItemViewModel : ActivatableViewModelBase
{
    public ConfigurationNumericItemViewModel(ConfigurationNumericItem item)
    {
        Item = item;
    }

    public ConfigurationNumericItem Item { get; }
}
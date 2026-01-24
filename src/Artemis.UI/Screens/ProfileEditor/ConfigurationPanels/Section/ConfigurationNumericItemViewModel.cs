using Artemis.Core;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.ProfileEditor.ConfigurationPanels.Section;

public class ConfigurationNumericItemViewModel : ActivatableViewModelBase
{
    public ConfigurationNumericItem Item { get; }

    public ConfigurationNumericItemViewModel(ConfigurationNumericItem item)
    {
        Item = item;
    }
}
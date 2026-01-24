using Artemis.Core;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.ProfileEditor.ConfigurationPanels.Section;

public class ConfigurationTextItemViewModel : ActivatableViewModelBase
{
    public ConfigurationTextItem Item { get; }

    public ConfigurationTextItemViewModel(ConfigurationTextItem item)
    {
        Item = item;
    }
}
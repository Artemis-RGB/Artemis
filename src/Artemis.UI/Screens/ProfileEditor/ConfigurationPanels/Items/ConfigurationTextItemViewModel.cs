using Artemis.Core;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.ProfileEditor.ConfigurationPanels.Items;

public class ConfigurationTextItemViewModel : ActivatableViewModelBase
{
    public ConfigurationTextItemViewModel(ConfigurationTextItem item)
    {
        Item = item;
    }

    public ConfigurationTextItem Item { get; }
}
using Artemis.Core;
using SkiaSharp;

namespace Artemis.UI.Screens.ProfileEditor.DesignPanels.Items;

public class ConfigurationSKColorItemDesignViewModel(ConfigurationSKColorItem item, ConfigurationSection configurationSection)
    : ConfigurationInputItemDesignViewModel<ConfigurationSKColorItem, SKColor>(item, configurationSection)
{
    public ConfigurationSKColorItem ColorItem { get; } = item;
}

using Artemis.Core;

namespace Artemis.UI.Screens.ProfileEditor.DesignPanels.Items;

public class ConfigurationColorGradientItemDesignViewModel(ConfigurationColorGradientItem item, ConfigurationSection configurationSection)
    : ConfigurationInputItemDesignViewModel<ConfigurationColorGradientItem, ColorGradient>(item, configurationSection)
{
    public ConfigurationColorGradientItem GradientItem { get; } = item;
}

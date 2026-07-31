using Artemis.Core;

namespace Artemis.UI.Screens.ProfileEditor.DesignPanels.Items;

public class ConfigurationColorGradientItemDesignViewModel : ConfigurationItemDesignViewModel
{
    public ConfigurationColorGradientItemDesignViewModel(ConfigurationColorGradientItem item, ConfigurationSection configurationSection) : base(item, configurationSection)
    {
        // The gradient picker doesn't handle null values well
        item.DefaultValue ??= ColorGradient.GetUnicornBarf();
        item.Value ??= ColorGradient.GetUnicornBarf();

        GradientItem = item;
    }

    public ConfigurationColorGradientItem GradientItem { get; }
}
using Material.Icons;
using ReactiveUI;

namespace Artemis.UI.Screens.Plugins;

public class PluginPlatformViewModel : ReactiveObject
{
    public PluginPlatformViewModel(string displayName, MaterialIconKind icon)
    {
        DisplayName = displayName;
        Icon = icon;
    }

    public string DisplayName { get; set; }
    public MaterialIconKind Icon { get; set; }
}
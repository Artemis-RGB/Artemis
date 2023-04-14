using Artemis.Core;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.Plugins.Prerequisites;

public class PluginPrerequisiteActionViewModel : ViewModelBase
{
    public PluginPrerequisiteActionViewModel(PluginPrerequisiteAction action)
    {
        Action = action;
    }

    public PluginPrerequisiteAction Action { get; }
}
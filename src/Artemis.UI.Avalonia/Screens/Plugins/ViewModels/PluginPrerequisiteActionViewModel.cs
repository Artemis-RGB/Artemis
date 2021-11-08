using Artemis.Core;
using Artemis.UI.Avalonia.Shared;

namespace Artemis.UI.Avalonia.Screens.Plugins.ViewModels
{
    public class PluginPrerequisiteActionViewModel : ViewModelBase
    {


        public PluginPrerequisiteActionViewModel(PluginPrerequisiteAction action)
        {
            Action = action;
        }

        public PluginPrerequisiteAction Action { get; }
    }
}
using System;
using System.ComponentModel;
using Artemis.Core;
using Artemis.UI.Avalonia;
using Artemis.UI.Avalonia.Shared;

namespace Artemis.UI.Screens.Plugins
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
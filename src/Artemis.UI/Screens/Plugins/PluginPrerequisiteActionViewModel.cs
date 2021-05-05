using System;
using System.ComponentModel;
using Artemis.Core;
using Stylet;

namespace Artemis.UI.Screens.Plugins
{
    public class PluginPrerequisiteActionViewModel : Screen
    {


        public PluginPrerequisiteActionViewModel(PluginPrerequisiteAction action)
        {
            Action = action;
        }

        public PluginPrerequisiteAction Action { get; }
    }
}
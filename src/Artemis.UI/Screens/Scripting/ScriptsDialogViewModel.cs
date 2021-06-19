using Artemis.Core;
using Stylet;

namespace Artemis.UI.Screens.Scripting
{
    public class ScriptsDialogViewModel : Conductor<ScriptConfigurationViewModel>.Collection.OneActive
    {
        public ScriptsDialogViewModel(Profile profile)
        {
        }

        public ScriptsDialogViewModel(Layer layer)
        {
        }

        public ScriptsDialogViewModel(ILayerProperty layerProperty)
        {
        }
    }

    public class ScriptConfigurationViewModel : Screen
    {
    }
}
using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.VisualScripting
{
    public class NodeScriptWindowViewModel : DialogViewModelBase<bool>
    {
        public NodeScript NodeScript { get; }
        public NodeScriptViewModel NodeScriptViewModel { get; set; }

        public NodeScriptWindowViewModel(NodeScript nodeScript, INodeVmFactory vmFactory)
        {
            NodeScript = nodeScript;
            NodeScriptViewModel = vmFactory.NodeScriptViewModel(NodeScript, false);
        }
    }
}
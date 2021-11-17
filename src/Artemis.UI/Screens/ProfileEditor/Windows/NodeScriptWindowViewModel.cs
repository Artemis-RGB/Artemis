using Artemis.Core;
using Artemis.Core.Services;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.Windows
{
    public class NodeScriptWindowViewModel : Screen
    {
        public NodeScriptWindowViewModel(NodeScript nodeScript, INodeService nodeService, ISettingsService settingsService)
        {
            NodeScript = nodeScript;
            AvailableNodes = new BindableCollection<NodeData>(nodeService.AvailableNodes);
            AlwaysShowValues = settingsService.GetSetting("ProfileEditor.AlwaysShowValues", true);
        }

        public NodeScript NodeScript { get; }
        public BindableCollection<NodeData> AvailableNodes { get; }
        public PluginSetting<bool> AlwaysShowValues { get; }
    }
}

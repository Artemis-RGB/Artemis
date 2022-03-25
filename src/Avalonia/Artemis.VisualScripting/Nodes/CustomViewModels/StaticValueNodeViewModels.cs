using Artemis.Core;
using Artemis.UI.Shared.VisualScripting;

namespace Artemis.VisualScripting.Nodes.CustomViewModels;

public class StaticNumericValueNodeCustomViewModel : CustomNodeViewModel
{
    public StaticNumericValueNodeCustomViewModel(INode node, INodeScript script) : base(node, script)
    {
    }
}

public class StaticStringValueNodeCustomViewModel : CustomNodeViewModel
{
    public StaticStringValueNodeCustomViewModel(INode node, INodeScript script) : base(node, script)
    {
    }
}
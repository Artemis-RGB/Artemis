using Artemis.Core.Nodes;
using Artemis.Core.Tests.Infrastructure.VisualScripting;

namespace Artemis.Core.Tests.Infrastructure.Plugins.Nodes;

/// <summary>
///     Registers the nodes used by data binding and event condition tests in the node type store, so scripts using them can
///     be saved and loaded again.
/// </summary>
public class DataModelTestNodeProvider : NodeProvider
{
    public override void Enable()
    {
        RegisterNodeType<TestDataModelNumberNode>();
        RegisterNodeType<TestColorNode>();
        RegisterNodeType<TestGreaterThanNode>();
    }

    public override void Disable()
    {
    }

    /// <summary>
    ///     Creates a registered node, the way the editor does, and adds it to the script.
    /// </summary>
    public T AddNode<T>(INodeScript script) where T : INode
    {
        NodeData nodeData = NodeDescriptors.Single(d => d.Type == typeof(T));
        T node = (T) nodeData.CreateNode(script, null);
        script.AddNode(node);
        return node;
    }
}

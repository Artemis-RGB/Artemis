using Artemis.Core.Nodes;
using Artemis.Core.Tests.Infrastructure.VisualScripting;

namespace Artemis.Core.Tests.Infrastructure.Plugins.Nodes;

/// <summary>
///     Registers the test node types in the node type store, so scripts using them can be saved and loaded again.
///     The real nodes live in the Artemis.Plugins repository.
/// </summary>
public class TestNodeProvider : NodeProvider
{
    public override void Enable()
    {
        RegisterNodeType<TestBooleanNode>();
        RegisterNodeType<TestNumericNode>();
    }

    public override void Disable()
    {
    }

    /// <summary>
    ///     Creates a registered test node, the way the editor does, and adds it to the script.
    /// </summary>
    public T AddNode<T>(INodeScript script) where T : INode
    {
        NodeData nodeData = NodeDescriptors.Single(d => d.Type == typeof(T));
        T node = (T) nodeData.CreateNode(script, null);
        script.AddNode(node);
        return node;
    }
}

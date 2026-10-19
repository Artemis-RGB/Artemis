using Artemis.Core.Nodes;
using Artemis.Core.Tests.Infrastructure.Plugins;
using SkiaSharp;

namespace Artemis.Core.Tests.Infrastructure.VisualScripting;

/// <summary>
///     Registers the nodes used by the visual scripting tests, so scripts using them can be saved and loaded again.
///     Also gives text a type color, which <see cref="ObjectOutputPins" /> needs before it creates pins for text
///     properties.
/// </summary>
public class ScriptTestNodeProvider : NodeProvider
{
    public override void Enable()
    {
        RegisterNodeType<TestSumNode>();
        RegisterNodeType<TestThrowingNode>();
        RegisterNodeType<TestTextNode>();
        RegisterNodeType<TestSettingsNode>();
        RegisterNodeType<TestViewModelNode>();
        RegisterTypeColor<string>(SKColors.Orange);
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

    /// <summary>
    ///     Gets the ID a <see cref="ScriptTestNodeProvider" /> gets once it is added to the given plugin, saved nodes refer
    ///     to their provider by this ID.
    /// </summary>
    public static string IdFor(TestPlugin plugin)
    {
        return $"{typeof(ScriptTestNodeProvider).FullName}-{plugin.Plugin.Guid.ToString()[..8]}";
    }
}

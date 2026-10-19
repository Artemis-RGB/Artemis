using Artemis.Core.Internal;
using Artemis.Core.Tests.Infrastructure.Plugins;
using Artemis.Core.Tests.Infrastructure.Plugins.Nodes;
using Artemis.Core.Tests.Infrastructure.VisualScripting;

namespace Artemis.Core.Tests.VisualScripting;

public class NodeScriptTests : IDisposable
{
    private readonly TestPlugin _plugin = new();
    private readonly TestNodeProvider _nodes;

    public NodeScriptTests()
    {
        _nodes = _plugin.AddFeature(new TestNodeProvider());
    }

    [Fact]
    public void Script_ProducesTheValueConnectedToTheExitNode()
    {
        NodeScript<bool> script = new("Test", "A test script");
        TestBooleanNode node = _nodes.AddNode<TestBooleanNode>(script);
        ExitNode(script).Input.ConnectTo(node.Output);

        node.Value = true;
        script.Run();
        script.Result.ShouldBeTrue();

        node.Value = false;
        script.Run();
        script.Result.ShouldBeFalse();
    }

    [Fact]
    public void Script_WithRegisteredNodes_SurvivesSavingAndLoading()
    {
        NodeScript<bool> script = new("Test", "A test script");
        TestBooleanNode node = _nodes.AddNode<TestBooleanNode>(script);
        ExitNode(script).Input.ConnectTo(node.Output);
        node.Value = true;
        script.Save();

        NodeScript<bool> loaded = new("Test", "A test script", script.Entity);
        loaded.Run();

        loaded.Nodes.OfType<TestBooleanNode>().Single().Value.ShouldBeTrue();
        loaded.ExitNodeConnected.ShouldBeTrue();
        loaded.Result.ShouldBeTrue();
    }

    public void Dispose()
    {
        _plugin.Dispose();
    }

    private static ExitNode<bool> ExitNode(NodeScript<bool> script)
    {
        return script.Nodes.OfType<ExitNode<bool>>().Single();
    }
}

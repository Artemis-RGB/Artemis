using Artemis.Core.Tests.Infrastructure.Plugins;
using Artemis.Core.Tests.Infrastructure.Plugins.Nodes;
using Artemis.Core.Tests.Infrastructure.VisualScripting;

namespace Artemis.Core.Tests.VisualScripting;

public class NodeScriptTests : ScriptTestBase
{
    [Fact]
    public void Script_ProducesTheValueConnectedToTheExitNode()
    {
        NodeScript<bool> script = CreateScript<bool>();
        TestBooleanNode node = Nodes.AddNode<TestBooleanNode>(script);
        script.GetExitNode().Input.ConnectTo(node.Output);

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
        NodeScript<bool> script = CreateScript<bool>();
        TestBooleanNode node = Nodes.AddNode<TestBooleanNode>(script);
        script.GetExitNode().Input.ConnectTo(node.Output);
        node.Value = true;
        script.Save();

        NodeScript<bool> loaded = Track(new NodeScript<bool>("Test", "A test script", script.Entity));
        loaded.Run();

        loaded.Nodes.OfType<TestBooleanNode>().Single().Value.ShouldBeTrue();
        loaded.ExitNodeConnected.ShouldBeTrue();
        loaded.Result.ShouldBeTrue();
    }

    [Fact]
    public void NewScript_ContainsOnlyItsExitNode()
    {
        NodeScript<bool> script = CreateScript<bool>("My script");

        INode exitNode = script.Nodes.ShouldHaveSingleItem();
        exitNode.IsExitNode.ShouldBeTrue();
        exitNode.IsDefaultNode.ShouldBeFalse();
        exitNode.Name.ShouldBe("My script");
        exitNode.Description.ShouldBe("A test script");
        script.Name.ShouldBe("My script");
        script.Description.ShouldBe("A test script");
        script.ResultType.ShouldBe(typeof(bool));
        script.ExitNodeConnected.ShouldBeFalse();
    }

    [Fact]
    public void Script_KeepsItsContext()
    {
        object context = new();
        using NodeScript<bool> script = new("Test", "A test script", context);

        script.Context.ShouldBeSameAs(context);
    }

    [Fact]
    public void Script_WithoutAConnectedExitNode_ProducesTheDefaultValue()
    {
        NodeScript<Numeric> numericScript = CreateScript<Numeric>();
        NodeScript<string> textScript = CreateScript<string>();
        NodeScript<bool> booleanScript = CreateScript<bool>();

        numericScript.Run();
        textScript.Run();
        booleanScript.Run();

        numericScript.Result.ShouldBe(new Numeric(0));
        textScript.Result.ShouldBeNull();
        booleanScript.Result.ShouldBeFalse();
    }

    [Fact]
    public void Script_ResultOnlyChangesWhenTheScriptRuns()
    {
        NodeScript<Numeric> script = CreateScript<Numeric>();
        TestNumericNode node = Nodes.AddNode<TestNumericNode>(script);
        script.GetExitNode().Input.ConnectTo(node.Output);
        node.Value = 1;
        script.Run();

        node.Value = 2;

        script.Result.ShouldBe(new Numeric(1));
        script.Run();
        script.Result.ShouldBe(new Numeric(2));
    }

    [Fact]
    public void NumericScript_ProducesTheNumber()
    {
        NodeScript<Numeric> script = CreateScript<Numeric>();
        TestNumericNode node = Nodes.AddNode<TestNumericNode>(script);
        script.GetExitNode().Input.ConnectTo(node.Output);
        node.Value = 2.5f;

        script.Run();

        script.Result.ShouldBe(new Numeric(2.5f));
    }

    [Fact]
    public void TextScript_ProducesTheText()
    {
        NodeScript<string> script = CreateScript<string>();
        TestTextNode node = ScriptNodes.AddNode<TestTextNode>(script);
        script.GetExitNode().Input.ConnectTo(node.Output);
        node.Value = "Hello";

        script.Run();

        script.Result.ShouldBe("Hello");
    }

    [Fact]
    public void ObjectScript_ProducesWhateverIsConnected()
    {
        NodeScript<object> script = CreateScript<object>();
        TestNumericNode node = Nodes.AddNode<TestNumericNode>(script);
        script.GetExitNode().Input.ConnectTo(node.Output);
        node.Value = 3;

        script.Run();

        script.Result.ShouldBe(new Numeric(3));
    }

    [Fact]
    public void FloatScript_DoesNotTurnANumericIntoAFloat()
    {
        // Connecting pins in code isn't type checked, the editor only offers compatible pins. A float input doesn't accept
        // a Numeric, so the result is the default value.
        NodeScript<float> script = CreateScript<float>();
        TestNumericNode node = Nodes.AddNode<TestNumericNode>(script);
        script.GetExitNode().Input.ConnectTo(node.Output);
        node.Value = 3;

        script.Run();

        script.ExitNodeConnected.ShouldBeTrue();
        script.Result.ShouldBe(0f);
    }

    [Fact]
    public void Script_CombinesTheValuesOfSeveralNodes()
    {
        NodeScript<Numeric> script = CreateScript<Numeric>();
        TestNumericNode first = Nodes.AddNode<TestNumericNode>(script);
        TestNumericNode second = Nodes.AddNode<TestNumericNode>(script);
        TestSumNode sum = ScriptNodes.AddNode<TestSumNode>(script);
        sum.Values.Pins.ElementAt(0).ConnectTo(first.Output);
        sum.Values.Pins.ElementAt(1).ConnectTo(second.Output);
        script.GetExitNode().Input.ConnectTo(sum.Result);
        first.Value = 2;
        second.Value = 5;

        script.Run();

        script.Result.ShouldBe(new Numeric(7));
    }

    [Fact]
    public void Run_EvaluatesANodeOnce_EvenWhenItFeedsSeveralInputs()
    {
        NodeScript<Numeric> script = CreateScript<Numeric>();
        TestNumericNode number = Nodes.AddNode<TestNumericNode>(script);
        TestSumNode sum = ScriptNodes.AddNode<TestSumNode>(script);
        sum.Values.Pins.ElementAt(0).ConnectTo(number.Output);
        sum.Values.Pins.ElementAt(1).ConnectTo(number.Output);
        script.GetExitNode().Input.ConnectTo(sum.Result);
        number.Value = 4;

        script.Run();
        script.Result.ShouldBe(new Numeric(8));
        number.Evaluations.ShouldBe(1);
        sum.Evaluations.ShouldBe(1);

        script.Run();
        number.Evaluations.ShouldBe(2);
        sum.Evaluations.ShouldBe(2);
    }

    [Fact]
    public void Run_SkipsNodesThatDoNotLeadToTheExitNode()
    {
        NodeScript<Numeric> script = CreateScript<Numeric>();
        TestNumericNode connected = Nodes.AddNode<TestNumericNode>(script);
        TestNumericNode loose = Nodes.AddNode<TestNumericNode>(script);
        script.GetExitNode().Input.ConnectTo(connected.Output);

        script.Run();

        connected.Evaluations.ShouldBe(1);
        loose.Evaluations.ShouldBe(0);
    }

    [Fact]
    public void Run_ResetsEveryNodeFirst()
    {
        NodeScript<bool> script = CreateScript<bool>();
        TestNode loose = new();
        script.AddNode(loose);
        int resets = 0;
        loose.Resetting += (_, _) => resets++;

        script.Run();
        script.Run();

        resets.ShouldBe(2);
        loose.Evaluations.ShouldBe(0);
    }

    [Fact]
    public void AddNode_AddsTheNodeAndRaisesNodeAdded()
    {
        NodeScript<bool> script = CreateScript<bool>();
        TestNode node = new();
        List<INode> added = [];
        script.NodeAdded += (_, e) => added.Add(e.Value);

        script.AddNode(node);

        script.Nodes.ShouldContain(node);
        added.ShouldBe([node]);
    }

    [Fact]
    public void RemoveNode_RemovesTheNodeAndRaisesNodeRemoved()
    {
        NodeScript<bool> script = CreateScript<bool>();
        TestNode node = new();
        script.AddNode(node);
        List<INode> removed = [];
        script.NodeRemoved += (_, e) => removed.Add(e.Value);

        script.RemoveNode(node);

        script.Nodes.ShouldNotContain(node);
        removed.ShouldBe([node]);
    }

    [Fact]
    public void RemoveNode_LeavesTheConnectionsOfTheNodeAlone()
    {
        // The editor disconnects the pins itself before it removes a node (see DeleteNode in Artemis.UI.Shared), so the
        // script still reads from the removed node
        NodeScript<bool> script = CreateScript<bool>();
        TestBooleanNode node = Nodes.AddNode<TestBooleanNode>(script);
        script.GetExitNode().Input.ConnectTo(node.Output);
        node.Value = true;

        script.RemoveNode(node);
        script.Run();

        script.ExitNodeConnected.ShouldBeTrue();
        script.Result.ShouldBeTrue();
    }

    [Fact]
    public void RemoveNode_OfANodeThatIsNotInTheScript_DoesNotRaiseNodeRemoved()
    {
        NodeScript<bool> script = CreateScript<bool>();
        int raised = 0;
        script.NodeRemoved += (_, _) => raised++;

        script.RemoveNode(new TestNode());

        raised.ShouldBe(0);
    }

    [Fact]
    public void Dispose_DisposesNodesThatCanBeDisposed()
    {
        NodeScript<bool> script = new("Test", "A test script");
        TestDisposableNode node = new();
        script.AddNode(node);

        script.Dispose();

        node.IsDisposed.ShouldBeTrue();
    }

    [Fact]
    public void DisablingTheNodeProvider_RemovesItsNodesFromTheScript()
    {
        TestPlugin otherPlugin = new();
        TestNodeProvider otherNodes = otherPlugin.AddFeature(new TestNodeProvider());
        NodeScript<bool> script = CreateScript<bool>();
        TestBooleanNode node = otherNodes.AddNode<TestBooleanNode>(script);
        TestSumNode keptNode = ScriptNodes.AddNode<TestSumNode>(script);
        List<INode> removed = [];
        script.NodeRemoved += (_, e) => removed.Add(e.Value);

        otherPlugin.Dispose();

        removed.ShouldBe([node]);
        script.Nodes.ShouldNotContain(node);
        script.Nodes.ShouldContain(keptNode);
    }

    [Fact]
    public void DisablingTheNodeProvider_LeavesNodesOfTheSameTypeFromAnotherProvider()
    {
        TestPlugin otherPlugin = new();
        TestNodeProvider otherNodes = otherPlugin.AddFeature(new TestNodeProvider());
        NodeScript<bool> script = CreateScript<bool>();
        TestNumericNode fromOtherProvider = otherNodes.AddNode<TestNumericNode>(script);
        TestNumericNode fromStillEnabledProvider = Nodes.AddNode<TestNumericNode>(script);

        otherPlugin.Dispose();

        script.Nodes.ShouldNotContain(fromOtherProvider);
        script.Nodes.ShouldContain(fromStillEnabledProvider);
    }

    [Fact]
    public void DisposedScript_NoLongerReactsToNodeProviders()
    {
        TestPlugin otherPlugin = new();
        TestNodeProvider otherNodes = otherPlugin.AddFeature(new TestNodeProvider());
        NodeScript<bool> script = new("Test", "A test script");
        TestBooleanNode node = otherNodes.AddNode<TestBooleanNode>(script);

        script.Dispose();
        otherPlugin.Dispose();

        script.Nodes.ShouldContain(node);
    }

    [Fact]
    public void FeatureDependencies_AreTheProvidersOfEveryNode()
    {
        NodeScript<bool> script = CreateScript<bool>();
        Nodes.AddNode<TestBooleanNode>(script);
        ScriptNodes.AddNode<TestSumNode>(script);
        ScriptNodes.AddNode<TestTextNode>(script);
        script.AddNode(new TestNode());

        // One entry per node rather than per provider, nodes without node data (the exit node and the unregistered
        // node) add nothing
        script.GetFeatureDependencies().ShouldBe([Nodes, ScriptNodes, ScriptNodes]);
    }
}

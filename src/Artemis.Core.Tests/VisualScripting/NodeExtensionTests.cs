using Artemis.Core.Tests.Infrastructure.VisualScripting;

namespace Artemis.Core.Tests.VisualScripting;

public class NodeExtensionTests : ScriptTestBase
{
    #region Size estimates

    [Fact]
    public void EstimateHeight_OfANodeWithoutPins_IsTheTitle()
    {
        new TestNode().EstimateHeight().ShouldBe(46);
    }

    [Fact]
    public void EstimateHeight_CountsTheSideWithTheMostPins()
    {
        TestNode node = new();
        node.CreateInputPin<Numeric>();
        node.CreateOutputPin<Numeric>();
        node.CreateOutputPin<Numeric>();

        node.EstimateHeight().ShouldBe(46 + 2 * 26);
    }

    [Fact]
    public void EstimateHeight_CountsAnExtraRowPerPinCollection()
    {
        // Two pins in the collection plus a row for the collection itself
        new TestSumNode().EstimateHeight().ShouldBe(46 + 3 * 26);
    }

    [Fact]
    public void EstimateWidth_IsTheSameForEveryNode()
    {
        new TestNode().EstimateWidth().ShouldBe(120);
        new TestSumNode().EstimateWidth().ShouldBe(120);
    }

    #endregion

    #region Loops

    [Fact]
    public void IsInLoop_ConnectingANodeToItself_IsTrue()
    {
        TestSumNode node = new();

        node.IsInLoop(node).ShouldBeTrue();
    }

    [Fact]
    public void IsInLoop_ForUnrelatedNodes_IsFalse()
    {
        TestSumNode first = new();
        TestSumNode second = new();

        first.IsInLoop(second).ShouldBeFalse();
    }

    [Fact]
    public void IsInLoop_LooksThroughSeveralNodes()
    {
        // a -> b -> c, connecting c back into a would make a loop
        TestSumNode a = new();
        TestSumNode b = new();
        TestSumNode c = new();
        b.Values.Pins.First().ConnectTo(a.Result);
        c.Values.Pins.First().ConnectTo(b.Result);

        a.IsInLoop(c).ShouldBeTrue();
        c.IsInLoop(a).ShouldBeFalse();
    }

    [Fact]
    public void IsInLoop_LooksThroughPlainInputPinsToo()
    {
        TestNode a = new();
        OutputPin<Numeric> output = a.CreateOutputPin<Numeric>();
        TestNode b = new();
        b.CreateInputPin<Numeric>().ConnectTo(output);

        a.IsInLoop(b).ShouldBeTrue();
        b.IsInLoop(a).ShouldBeFalse();
    }

    #endregion

    #region Organize

    [Fact]
    public void Organize_PutsNodesInColumnsLeftOfTheExitNode()
    {
        NodeScript<Numeric> script = CreateScript<Numeric>();
        TestNumericNode first = Nodes.AddNode<TestNumericNode>(script);
        TestNumericNode second = Nodes.AddNode<TestNumericNode>(script);
        TestSumNode sum = ScriptNodes.AddNode<TestSumNode>(script);
        TestNumericNode loose = Nodes.AddNode<TestNumericNode>(script);
        sum.Values.Pins.ElementAt(0).ConnectTo(first.Output);
        sum.Values.Pins.ElementAt(1).ConnectTo(second.Output);
        script.GetExitNode().Input.ConnectTo(sum.Result);

        script.Organize();

        // Columns are 120 wide with 160 between them, each column is centered vertically around 0 with 20 between nodes
        Position(script.GetExitNode()).ShouldBe((0, -36));
        Position(sum).ShouldBe((-280, -62));
        Position(first).ShouldBe((-560, -82));
        Position(second).ShouldBe((-560, 10));
        // Nodes that don't lead to the exit node go to its right
        Position(loose).ShouldBe((200, -36));
    }

    [Fact]
    public void Organize_PutsANodeInTheColumnOfItsFurthestUse()
    {
        NodeScript<Numeric> script = CreateScript<Numeric>();
        TestNumericNode number = Nodes.AddNode<TestNumericNode>(script);
        TestSumNode inner = ScriptNodes.AddNode<TestSumNode>(script);
        TestSumNode outer = ScriptNodes.AddNode<TestSumNode>(script);
        inner.Values.Pins.ElementAt(0).ConnectTo(number.Output);
        outer.Values.Pins.ElementAt(0).ConnectTo(inner.Result);
        outer.Values.Pins.ElementAt(1).ConnectTo(number.Output);
        script.GetExitNode().Input.ConnectTo(outer.Result);

        script.Organize();

        outer.X.ShouldBe(-280);
        inner.X.ShouldBe(-560);
        number.X.ShouldBe(-840);
    }

    private static (double X, double Y) Position(INode node)
    {
        return (node.X, node.Y);
    }

    #endregion
}

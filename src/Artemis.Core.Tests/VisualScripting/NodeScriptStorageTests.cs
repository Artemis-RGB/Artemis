using Artemis.Core.Tests.Infrastructure.Plugins;
using Artemis.Core.Tests.Infrastructure.VisualScripting;
using Artemis.Storage.Entities.Profile.Nodes;

namespace Artemis.Core.Tests.VisualScripting;

public class NodeScriptStorageTests : ScriptTestBase
{
    [Fact]
    public void Save_DescribesEveryNode()
    {
        NodeScript<bool> script = CreateScript<bool>("My script");
        TestBooleanNode node = Nodes.AddNode<TestBooleanNode>(script);
        node.X = 10;
        node.Y = 20;
        node.Value = true;

        script.Save();

        script.Entity.Name.ShouldBe("My script");
        script.Entity.Description.ShouldBe("A test script");
        script.Entity.Nodes.Count.ShouldBe(2);

        NodeEntity exitEntity = script.Entity.Nodes.Single(n => n.IsExitNode);
        exitEntity.Id.ShouldBe(script.GetExitNode().Id);
        exitEntity.ProviderId.ShouldBe(Constants.CorePluginFeature.Id);
        exitEntity.Type.ShouldBe("ExitNode`1");

        NodeEntity nodeEntity = script.Entity.Nodes.Single(n => !n.IsExitNode);
        nodeEntity.Id.ShouldBe(node.Id);
        nodeEntity.ProviderId.ShouldBe(Nodes.Id);
        nodeEntity.Type.ShouldBe(nameof(TestBooleanNode));
        nodeEntity.Name.ShouldBe("Test boolean");
        nodeEntity.Description.ShouldBe("Outputs a boolean controlled by the test");
        nodeEntity.X.ShouldBe(10);
        nodeEntity.Y.ShouldBe(20);
        nodeEntity.Storage.ShouldBe("true");
    }

    [Fact]
    public void Save_DescribesConnectionsFromTheInputSide()
    {
        NodeScript<Numeric> script = CreateScript<Numeric>();
        TestNumericNode number = Nodes.AddNode<TestNumericNode>(script);
        TestSumNode sum = ScriptNodes.AddNode<TestSumNode>(script);
        sum.Values.Pins.ElementAt(1).ConnectTo(number.Output);
        script.GetExitNode().Input.ConnectTo(sum.Result);

        script.Save();

        script.Entity.Connections.Count.ShouldBe(2);

        // The input pin is the source, the output pin it reads from is the target. -1 means the pin isn't in a collection.
        NodeConnectionEntity intoExit = script.Entity.Connections.Single(c => c.SourceNode == script.GetExitNode().Id);
        intoExit.SourcePinCollectionId.ShouldBe(-1);
        intoExit.SourcePinId.ShouldBe(0);
        intoExit.SourceType.ShouldBe(nameof(Numeric));
        intoExit.TargetNode.ShouldBe(sum.Id);
        intoExit.TargetPinCollectionId.ShouldBe(-1);
        intoExit.TargetPinId.ShouldBe(0);

        NodeConnectionEntity intoSum = script.Entity.Connections.Single(c => c.SourceNode == sum.Id);
        intoSum.SourcePinCollectionId.ShouldBe(0);
        intoSum.SourcePinId.ShouldBe(1);
        intoSum.TargetNode.ShouldBe(number.Id);
        intoSum.TargetPinCollectionId.ShouldBe(-1);
        intoSum.TargetPinId.ShouldBe(0);
    }

    [Fact]
    public void Save_StoresHowManyPinsEachCollectionHas()
    {
        NodeScript<Numeric> script = CreateScript<Numeric>();
        TestSumNode sum = ScriptNodes.AddNode<TestSumNode>(script);
        sum.Values.Add(sum.Values.CreatePin());

        script.Save();

        NodePinCollectionEntity collection = script.Entity.Nodes.Single(n => n.Id == sum.Id).PinCollections.ShouldHaveSingleItem();
        collection.Id.ShouldBe(0);
        collection.Direction.ShouldBe((int) PinDirection.Input);
        collection.Amount.ShouldBe(3);
    }

    [Fact]
    public void NodePositions_SurviveSavingAndLoading()
    {
        NodeScript<bool> script = CreateScript<bool>();
        TestBooleanNode node = Nodes.AddNode<TestBooleanNode>(script);
        node.X = 120.5;
        node.Y = -40;
        script.GetExitNode().X = 300;
        script.GetExitNode().Y = 15;

        NodeScript<bool> loaded = SaveAndLoad(script);

        TestBooleanNode loadedNode = loaded.Nodes.OfType<TestBooleanNode>().Single();
        loadedNode.Id.ShouldBe(node.Id);
        loadedNode.X.ShouldBe(120.5);
        loadedNode.Y.ShouldBe(-40);
        loaded.GetExitNode().X.ShouldBe(300);
        loaded.GetExitNode().Y.ShouldBe(15);
    }

    [Fact]
    public void LoadedNodes_AreNoLongerMarkedAsLoading()
    {
        NodeScript<bool> script = CreateScript<bool>();
        Nodes.AddNode<TestBooleanNode>(script);

        NodeScript<bool> loaded = SaveAndLoad(script);

        loaded.Nodes.ShouldAllBe(n => !n.IsLoading);
    }

    [Fact]
    public void LoadedNodes_GetTheirNodeData()
    {
        NodeScript<bool> script = CreateScript<bool>();
        Nodes.AddNode<TestBooleanNode>(script);

        NodeScript<bool> loaded = SaveAndLoad(script);

        NodeData nodeData = loaded.Nodes.OfType<TestBooleanNode>().Single().NodeData.ShouldNotBeNull();
        nodeData.Provider.ShouldBe(Nodes);
    }

    [Fact]
    public void PinCollections_AndTheirConnections_SurviveSavingAndLoading()
    {
        NodeScript<Numeric> script = CreateScript<Numeric>();
        TestNumericNode number = Nodes.AddNode<TestNumericNode>(script);
        TestSumNode sum = ScriptNodes.AddNode<TestSumNode>(script);
        sum.Values.Add(sum.Values.CreatePin());
        sum.Values.Add(sum.Values.CreatePin());
        sum.Values.Pins.ElementAt(3).ConnectTo(number.Output);
        script.GetExitNode().Input.ConnectTo(sum.Result);
        number.Value = 6;

        NodeScript<Numeric> loaded = SaveAndLoad(script);
        loaded.Run();

        TestSumNode loadedSum = loaded.Nodes.OfType<TestSumNode>().Single();
        loadedSum.Values.Count().ShouldBe(4);
        loadedSum.Values.Pins.ElementAt(3).ConnectedTo.ShouldHaveSingleItem().Node.ShouldBeOfType<TestNumericNode>();
        loaded.Result.ShouldBe(new Numeric(6));
    }

    [Fact]
    public void ObjectStorage_SurvivesSavingAndLoading()
    {
        NodeScript<bool> script = CreateScript<bool>();
        TestSettingsNode node = ScriptNodes.AddNode<TestSettingsNode>(script);
        node.Storage = new TestNodeSettings {Name = "Settings", Amount = 1.5, Numbers = [1, 2, 3]};

        NodeScript<bool> loaded = SaveAndLoad(script);

        TestNodeSettings settings = loaded.Nodes.OfType<TestSettingsNode>().Single().Storage.ShouldNotBeNull();
        settings.Name.ShouldBe("Settings");
        settings.Amount.ShouldBe(1.5);
        settings.Numbers.ShouldBe([1, 2, 3]);
    }

    [Fact]
    public void NodeWithUnreadableStorage_StillLoads_WithEmptyStorage()
    {
        NodeScript<bool> script = CreateScript<bool>();
        TestBooleanNode node = Nodes.AddNode<TestBooleanNode>(script);
        script.GetExitNode().Input.ConnectTo(node.Output);
        node.Value = true;
        script.Save();
        script.Entity.Nodes.Single(n => n.Id == node.Id).Storage = "{not json";

        NodeScript<bool> loaded = Load<bool>(script.Entity);
        loaded.Run();

        loaded.Nodes.OfType<TestBooleanNode>().Single().Value.ShouldBeFalse();
        loaded.ExitNodeConnected.ShouldBeTrue();
        loaded.Result.ShouldBeFalse();
    }

    [Fact]
    public void NodeThatFailsToInitialize_StillLoads_ButIsBroken()
    {
        NodeScript<Numeric> script = CreateScript<Numeric>();
        TestThrowingNode node = ScriptNodes.AddNode<TestThrowingNode>(script);
        node.FailsOn = TestThrowingNode.FailureMode.Initialize;

        NodeScript<Numeric> loaded = SaveAndLoad(script);

        TestThrowingNode loadedNode = loaded.Nodes.OfType<TestThrowingNode>().Single();
        loadedNode.BrokenState.ShouldBe("Failed to initialize");
        loadedNode.BrokenStateException.ShouldBeOfType<InvalidOperationException>();
    }

    [Fact]
    public void NodesOfAMissingPlugin_AreLeftOut_AndTheirConnectionsDropped()
    {
        TestPlugin missingPlugin = new();
        ScriptTestNodeProvider missingNodes = missingPlugin.AddFeature(new ScriptTestNodeProvider());
        NodeScript<string> script = CreateScript<string>();
        TestTextNode node = missingNodes.AddNode<TestTextNode>(script);
        script.GetExitNode().Input.ConnectTo(node.Output);
        node.Value = "Hello";
        script.Save();
        missingPlugin.Dispose();

        NodeScript<string> loaded = Load<string>(script.Entity);
        loaded.Run();

        loaded.Nodes.ShouldHaveSingleItem().IsExitNode.ShouldBeTrue();
        loaded.ExitNodeConnected.ShouldBeFalse();
        loaded.Result.ShouldBeNull();
    }

    [Fact]
    public void NodesOfAMissingPlugin_AreLostWhenTheScriptIsSavedAgain()
    {
        // Pinned as-is: a script loaded while a plugin is missing forgets that plugin's nodes the next time it is saved.
        // Saving a profile while a plugin is disabled permanently removes its nodes from the profile's scripts.
        TestPlugin missingPlugin = new();
        ScriptTestNodeProvider missingNodes = missingPlugin.AddFeature(new ScriptTestNodeProvider());
        NodeScript<string> script = CreateScript<string>();
        TestTextNode node = missingNodes.AddNode<TestTextNode>(script);
        script.GetExitNode().Input.ConnectTo(node.Output);
        script.Save();
        missingPlugin.Dispose();

        NodeScript<string> loaded = Load<string>(script.Entity);
        loaded.Entity.Nodes.Count.ShouldBe(2);
        loaded.Save();

        loaded.Entity.Nodes.ShouldHaveSingleItem().IsExitNode.ShouldBeTrue();
        loaded.Entity.Connections.ShouldBeEmpty();
    }

    [Fact]
    public void NodesOfAMissingPlugin_AppearOnceThePluginIsEnabled()
    {
        // Save the script with nodes from one plugin, then point the saved nodes at a plugin that isn't enabled yet, as
        // if the profile came from a machine where that plugin is installed
        TestPlugin savingPlugin = new();
        ScriptTestNodeProvider savingNodes = savingPlugin.AddFeature(new ScriptTestNodeProvider());
        NodeScript<string> script = CreateScript<string>();
        TestTextNode node = savingNodes.AddNode<TestTextNode>(script);
        script.GetExitNode().Input.ConnectTo(node.Output);
        node.Value = "Hello";
        script.Save();
        savingPlugin.Dispose();

        TestPlugin laterPlugin = new();
        foreach (NodeEntity nodeEntity in script.Entity.Nodes.Where(n => n.ProviderId == ScriptTestNodeProvider.IdFor(savingPlugin)))
            nodeEntity.ProviderId = ScriptTestNodeProvider.IdFor(laterPlugin);
        NodeScript<string> loaded = Load<string>(script.Entity);
        loaded.Nodes.Count().ShouldBe(1);

        try
        {
            laterPlugin.AddFeature(new ScriptTestNodeProvider());
            loaded.Run();

            loaded.Nodes.OfType<TestTextNode>().ShouldHaveSingleItem().Value.ShouldBe("Hello");
            loaded.ExitNodeConnected.ShouldBeTrue();
            loaded.Result.ShouldBe("Hello");
        }
        finally
        {
            laterPlugin.Dispose();
        }
    }

    [Fact]
    public void UnregisteredNodes_AreLeftOutWhenLoading()
    {
        NodeScript<bool> script = CreateScript<bool>();
        TestNode node = new();
        node.CreateOutputPin<bool>();
        script.AddNode(node);
        script.GetExitNode().Input.ConnectTo(node.Pins.Single());

        NodeScript<bool> loaded = SaveAndLoad(script);

        script.Entity.Nodes.Single(n => n.Id == node.Id).ProviderId.ShouldBe(Constants.CorePluginFeature.Id);
        loaded.Nodes.ShouldHaveSingleItem().IsExitNode.ShouldBeTrue();
        loaded.ExitNodeConnected.ShouldBeFalse();
    }

    [Fact]
    public void IncompatibleConnections_AreDroppedWhenLoading()
    {
        // In code any two pins can be connected, loading only restores connections between compatible pins
        NodeScript<float> script = CreateScript<float>();
        TestNumericNode node = Nodes.AddNode<TestNumericNode>(script);
        script.GetExitNode().Input.ConnectTo(node.Output);
        script.ExitNodeConnected.ShouldBeTrue();

        NodeScript<float> loaded = SaveAndLoad(script);

        loaded.ExitNodeConnected.ShouldBeFalse();
        loaded.Nodes.OfType<TestNumericNode>().Single().Output.ConnectedTo.ShouldBeEmpty();
    }

    [Fact]
    public void InputConnectedToTwoOutputs_KeepsOnlyTheLastConnectionWhenLoading()
    {
        // Pinned as-is: in code an input can be connected to several outputs, it then reads from the first one. Loading
        // keeps only the last connection, so after loading the script reads from a different node than before.
        NodeScript<Numeric> script = CreateScript<Numeric>();
        TestNumericNode first = Nodes.AddNode<TestNumericNode>(script);
        TestNumericNode second = Nodes.AddNode<TestNumericNode>(script);
        first.Value = 1;
        second.Value = 2;
        script.GetExitNode().Input.ConnectTo(first.Output);
        script.GetExitNode().Input.ConnectTo(second.Output);
        script.Run();
        script.Result.ShouldBe(new Numeric(1));

        NodeScript<Numeric> loaded = SaveAndLoad(script);
        loaded.Run();

        loaded.GetExitNode().Input.ConnectedTo.ShouldHaveSingleItem().Node.Id.ShouldBe(second.Id);
        loaded.Result.ShouldBe(new Numeric(2));
    }

    [Fact]
    public void LoadConnections_CanRunAgainWithoutDuplicatingConnections()
    {
        NodeScript<bool> script = CreateScript<bool>();
        TestBooleanNode node = Nodes.AddNode<TestBooleanNode>(script);
        script.GetExitNode().Input.ConnectTo(node.Output);

        NodeScript<bool> loaded = SaveAndLoad(script);
        loaded.LoadConnections();
        loaded.LoadConnections();

        loaded.GetExitNode().Input.ConnectedTo.Count.ShouldBe(1);
        loaded.Nodes.OfType<TestBooleanNode>().Single().Output.ConnectedTo.Count.ShouldBe(1);
    }

    [Fact]
    public void Load_AppliesTheEntityToExistingNodes()
    {
        NodeScript<Numeric> script = CreateScript<Numeric>();
        TestSumNode sum = ScriptNodes.AddNode<TestSumNode>(script);
        sum.X = 5;
        script.Save();

        sum.X = 50;
        sum.Values.Add(sum.Values.CreatePin());
        sum.Values.Add(sum.Values.CreatePin());
        script.Load();

        script.Nodes.OfType<TestSumNode>().ShouldHaveSingleItem().ShouldBeSameAs(sum);
        sum.X.ShouldBe(5);
        sum.Values.Count().ShouldBe(2);
    }

    [Fact]
    public void Load_AddsPinsToCollectionsThatHaveTooFew()
    {
        NodeScript<Numeric> script = CreateScript<Numeric>();
        TestSumNode sum = ScriptNodes.AddNode<TestSumNode>(script);
        script.Save();
        script.Entity.Nodes.Single(n => n.Id == sum.Id).PinCollections[0].Amount = 5;

        script.Load();

        sum.Values.Count().ShouldBe(5);
    }

    [Fact]
    public void Load_RemovesAndDisposesNodesThatAreNotInTheEntity()
    {
        NodeScript<bool> script = CreateScript<bool>();
        script.Save();
        TestDisposableNode node = new();
        script.AddNode(node);
        List<INode> removed = [];
        script.NodeRemoved += (_, e) => removed.Add(e.Value);

        script.Load();

        script.Nodes.ShouldNotContain(node);
        removed.ShouldBe([node]);
        node.IsDisposed.ShouldBeTrue();
    }

    [Fact]
    public void DefaultNodes_AreAddedToNewScripts()
    {
        TestDefaultNode defaultNode = new();

        NodeScript<Numeric> script = CreateScript<Numeric>(defaultNodes: [defaultNode]);

        script.Nodes.ShouldBe([defaultNode, script.GetExitNode()]);
        defaultNode.IsDefaultNode.ShouldBeTrue();
        defaultNode.IsExitNode.ShouldBeFalse();
        defaultNode.Id.ShouldBe(TestDefaultNode.NodeId);
        defaultNode.Name.ShouldBe("Test default node");
        defaultNode.Description.ShouldBe("A node every script starts with");
    }

    [Fact]
    public void DefaultNodes_KeepTheirPositionAndConnections_WhenLoadedWithTheScript()
    {
        TestDefaultNode defaultNode = new() {X = 25, Y = 35};
        NodeScript<Numeric> script = CreateScript<Numeric>(defaultNodes: [defaultNode]);
        script.GetExitNode().Input.ConnectTo(defaultNode.Output);

        TestDefaultNode loadedDefaultNode = new() {Value = 9};
        NodeScript<Numeric> loaded = SaveAndLoad(script, [loadedDefaultNode]);
        loaded.Run();

        loadedDefaultNode.X.ShouldBe(25);
        loadedDefaultNode.Y.ShouldBe(35);
        loaded.Result.ShouldBe(new Numeric(9));
    }

    [Fact]
    public void DefaultNodes_AreLeftOut_WhenTheScriptIsLoadedWithoutThem()
    {
        // Default nodes aren't registered, the script only finds them when it is given them again
        TestDefaultNode defaultNode = new();
        NodeScript<Numeric> script = CreateScript<Numeric>(defaultNodes: [defaultNode]);
        script.GetExitNode().Input.ConnectTo(defaultNode.Output);

        NodeScript<Numeric> loaded = SaveAndLoad(script);

        loaded.Nodes.ShouldHaveSingleItem().IsExitNode.ShouldBeTrue();
        loaded.ExitNodeConnected.ShouldBeFalse();
    }
}

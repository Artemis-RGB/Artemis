using Artemis.Core.Tests.Infrastructure.VisualScripting;

namespace Artemis.Core.Tests.VisualScripting;

public class NodeTests : ScriptTestBase
{
    #region Pins

    [Fact]
    public void CreatingPins_AddsThemToTheNode_AndRaisesPinAdded()
    {
        TestNode node = new();
        List<IPin> added = [];
        node.PinAdded += (_, e) => added.Add(e.Value);

        InputPin<Numeric> typedInput = node.CreateInputPin<Numeric>("A");
        InputPin input = node.CreateInputPin(typeof(string), "B");
        OutputPin<Numeric> typedOutput = node.CreateOutputPin<Numeric>("C");
        OutputPin output = node.CreateOutputPin(typeof(string), "D");

        node.Pins.ShouldBe([typedInput, input, typedOutput, output]);
        added.ShouldBe([typedInput, input, typedOutput, output]);
    }

    [Fact]
    public void RemovePin_RemovesAndDisconnectsThePin_AndRaisesPinRemoved()
    {
        TestNode node = new();
        TestNode other = new();
        InputPin<Numeric> input = node.CreateInputPin<Numeric>();
        OutputPin<Numeric> output = other.CreateOutputPin<Numeric>();
        input.ConnectTo(output);
        List<IPin> removed = [];
        node.PinRemoved += (_, e) => removed.Add(e.Value);

        node.RemovePin(input).ShouldBeTrue();

        node.Pins.ShouldBeEmpty();
        output.ConnectedTo.ShouldBeEmpty();
        removed.ShouldBe([input]);
    }

    [Fact]
    public void RemovePin_OfAnotherNode_ReturnsFalse()
    {
        TestNode node = new();
        TestNode other = new();
        OutputPin<Numeric> output = other.CreateOutputPin<Numeric>();

        node.RemovePin(output).ShouldBeFalse();
        other.Pins.ShouldBe([output]);
    }

    [Fact]
    public void AddPin_PutsARemovedPinBack()
    {
        TestNode node = new();
        InputPin<Numeric> input = node.CreateInputPin<Numeric>();
        node.RemovePin(input);
        List<IPin> added = [];
        node.PinAdded += (_, e) => added.Add(e.Value);

        node.AddPin(input);
        node.AddPin(input);

        node.Pins.ShouldBe([input]);
        added.ShouldBe([input]);
    }

    [Fact]
    public void AddPin_RejectsPinsOfAnotherNode()
    {
        TestNode node = new();
        TestNode other = new();

        Should.Throw<ArtemisCoreException>(() => node.AddPin(other.CreateInputPin<Numeric>()));
    }

    [Fact]
    public void CreateOrAddInputPin_TurnsNumberTypesIntoNumeric()
    {
        TestNode node = new();

        InputPin pin = node.CreateOrAddInputPin(typeof(int), "Amount");

        pin.Type.ShouldBe(typeof(Numeric));
        pin.Name.ShouldBe("Amount");
        node.Pins.ShouldBe([pin]);
    }

    [Fact]
    public void CreateOrAddInputPin_ReusesARemovedPin()
    {
        TestNode node = new();
        InputPin first = node.CreateOrAddInputPin(typeof(string), "Text");
        node.RemovePin(first);

        InputPin second = node.CreateOrAddInputPin(typeof(bool), "Flag");
        InputPin third = node.CreateOrAddInputPin(typeof(bool), "Other flag");

        second.ShouldBeSameAs(first);
        second.Type.ShouldBe(typeof(bool));
        second.Name.ShouldBe("Flag");
        third.ShouldNotBeSameAs(first);
        node.Pins.ShouldBe([second, third]);
    }

    [Fact]
    public void CreateOrAddOutputPin_TurnsNumberTypesIntoNumeric_AndReusesARemovedPin()
    {
        TestNode node = new();
        OutputPin first = node.CreateOrAddOutputPin(typeof(double), "Amount");
        first.Type.ShouldBe(typeof(Numeric));
        node.RemovePin(first);

        OutputPin second = node.CreateOrAddOutputPin(typeof(string), "Text");

        second.ShouldBeSameAs(first);
        second.Type.ShouldBe(typeof(string));
        second.Name.ShouldBe("Text");
        node.Pins.ShouldBe([second]);
    }

    [Fact]
    public void CreateOrAddPin_OnlyReusesPinsItCreatedItself()
    {
        TestNode node = new();
        InputPin created = node.CreateInputPin(typeof(string));
        node.RemovePin(created);

        InputPin pin = node.CreateOrAddInputPin(typeof(string), "Text");

        pin.ShouldNotBeSameAs(created);
    }

    #endregion

    #region Evaluating

    [Fact]
    public void Reset_ResetsEveryPin_AndRaisesResetting()
    {
        TestNode node = new();
        OutputPin<Numeric> output = node.CreateOutputPin<Numeric>();
        output.Value = new Numeric(1);
        int resetting = 0;
        node.Resetting += (_, _) => resetting++;

        node.Reset();

        output.IsEvaluated.ShouldBeFalse();
        resetting.ShouldBe(1);
    }

    [Fact]
    public void ReadingAnOutput_EvaluatesTheNodeOnlyOnce()
    {
        TestNode node = new();
        OutputPin<Numeric> output = node.CreateOutputPin<Numeric>();
        node.OnEvaluate = _ => output.Value = new Numeric(2);

        output.Value.ShouldBe(new Numeric(2));
        output.Value.ShouldBe(new Numeric(2));

        node.Evaluations.ShouldBe(1);
    }

    [Fact]
    public void ReadingAnOutput_ThatTheNodeDoesNotSet_EvaluatesTheNodeEveryTime()
    {
        TestNode node = new();
        OutputPin<Numeric> output = node.CreateOutputPin<Numeric>();

        _ = output.Value;
        _ = output.Value;

        node.Evaluations.ShouldBe(2);
    }

    [Fact]
    public void TryEvaluate_WhenEvaluatingFails_BreaksTheNode()
    {
        TestNode node = new() {OnEvaluate = _ => throw new InvalidOperationException("Oops")};
        int changes = 0;
        node.BrokenStateChanged += (_, _) => changes++;

        node.TryEvaluate();

        node.BrokenState.ShouldBe("Failed to evaluate");
        node.BrokenStateException.ShouldBeOfType<InvalidOperationException>().Message.ShouldBe("Oops");
        node.BrokenDisplayName.ShouldBe("Test node");
        node.GetBrokenHierarchy().ShouldBe([node]);
        changes.ShouldBe(1);
    }

    [Fact]
    public void TryEvaluate_WhenEvaluatingWorksAgain_RepairsTheNode()
    {
        bool fail = true;
        TestNode node = new() {OnEvaluate = _ => { if (fail) throw new InvalidOperationException(); }};
        node.TryEvaluate();

        fail = false;
        node.TryEvaluate();

        node.BrokenState.ShouldBeNull();
        node.BrokenStateException.ShouldBeNull();
    }

    [Fact]
    public void TryInitialize_WhenInitializingFails_BreaksTheNode()
    {
        TestThrowingNode node = new() {FailsOn = TestThrowingNode.FailureMode.Initialize};

        node.TryInitialize(CreateScript<bool>());

        node.BrokenState.ShouldBe("Failed to initialize");
    }

    [Fact]
    public void FailingNode_InAScript_SetsItsOutputsToTheirDefaultValues()
    {
        // Rather than letting the value from the last run that worked flow on
        NodeScript<Numeric> script = CreateScript<Numeric>();
        TestThrowingNode node = ScriptNodes.AddNode<TestThrowingNode>(script);
        script.GetExitNode().Input.ConnectTo(node.Output);
        node.Value = 5;
        script.Run();

        node.Value = 6;
        node.FailsOn = TestThrowingNode.FailureMode.Evaluate;
        script.Run();

        script.Result.ShouldBe(new Numeric(0));
        node.BrokenState.ShouldBe("Failed to evaluate");
        script.GetExitNode().BrokenState.ShouldBeNull();
    }

    [Fact]
    public void FailingNode_InAScript_IsEvaluatedOncePerRun()
    {
        // A failing node sets its outputs to their default values, so the other pins reading from it don't try again
        NodeScript<Numeric> script = CreateScript<Numeric>();
        TestNode failing = new() {OnEvaluate = _ => throw new InvalidOperationException()};
        OutputPin<Numeric> output = failing.CreateOutputPin<Numeric>();
        script.AddNode(failing);
        TestSumNode sum = ScriptNodes.AddNode<TestSumNode>(script);
        sum.Values.Pins.ElementAt(0).ConnectTo(output);
        sum.Values.Pins.ElementAt(1).ConnectTo(output);
        script.GetExitNode().Input.ConnectTo(sum.Result);

        script.Run();

        failing.Evaluations.ShouldBe(1);
        sum.BrokenState.ShouldBeNull();
    }

    [Fact]
    public void ReadingAnInputOfTheWrongType_BreaksTheNodeReadingIt()
    {
        TestNode source = new();
        OutputPin<float> output = source.CreateOutputPin<float>();
        source.OnEvaluate = _ => output.Value = 1f;
        TestNode reader = new();
        InputPin input = reader.CreateInputPin(typeof(Numeric));
        reader.OnEvaluate = _ => input.Value.ShouldNotBeNull();
        input.ConnectTo(output);

        reader.TryEvaluate();

        reader.BrokenState.ShouldBe("Failed to evaluate");
        reader.BrokenStateException.ShouldBeOfType<ArgumentException>();
    }

    #endregion

    #region Storage

    [Fact]
    public void Storage_RaisesStorageModifiedOnlyWhenItChanges()
    {
        TestBooleanNode node = new();
        int modified = 0;
        List<string?> changed = [];
        node.StorageModified += (_, _) => modified++;
        node.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        node.Value = true;
        node.Value = true;
        node.Value = false;

        modified.ShouldBe(2);
        changed.ShouldBe([nameof(Node<bool>.Storage), nameof(Node<bool>.Storage)]);
    }

    [Fact]
    public void Storage_IsSerializedAsJson()
    {
        TestSettingsNode node = new() {Storage = new TestNodeSettings {Name = "A", Amount = 2, Numbers = [3]}};

        TestSettingsNode copy = new();
        copy.DeserializeStorage(node.SerializeStorage());

        copy.Storage.ShouldNotBeNull().Name.ShouldBe("A");
        copy.Storage.Amount.ShouldBe(2);
        copy.Storage.Numbers.ShouldBe([3]);
    }

    [Fact]
    public void DeserializingNull_ClearsTheStorage()
    {
        TestTextNode node = new() {Value = "Hello"};

        node.DeserializeStorage("null");

        node.Value.ShouldBeNull();
    }

    [Fact]
    public void DeserializingInvalidJson_Throws()
    {
        TestBooleanNode node = new();

        Should.Throw<System.Text.Json.JsonException>(() => node.DeserializeStorage("{not json"));
    }

    [Fact]
    public void NodeWithoutStorage_StoresNothing()
    {
        TestNode node = new();

        node.SerializeStorage().ShouldBeEmpty();
        node.DeserializeStorage("anything");
    }

    #endregion

    #region Other

    [Fact]
    public void NewNodes_GetTheirOwnId()
    {
        new TestNode().Id.ShouldNotBe(new TestNode().Id);
        new TestNode().Id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void Node_RaisesPropertyChangedForItsProperties()
    {
        TestNode node = new();
        List<string?> changed = [];
        node.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        node.Name = "Name";
        node.Description = "Description";
        node.HelpUrl = "https://example.com";
        node.X = 1;
        node.Y = 2;
        node.Id = Guid.NewGuid();

        changed.ShouldBe([nameof(INode.Name), nameof(INode.Description), nameof(INode.HelpUrl), nameof(INode.X), nameof(INode.Y), nameof(INode.Id)]);
    }

    [Fact]
    public void Node_WithoutNodeData_DependsOnNoFeatures()
    {
        new TestNode().GetFeatureDependencies().ShouldBeEmpty();
    }

    [Fact]
    public void RegisteredNode_DependsOnItsProvider()
    {
        TestSumNode node = ScriptNodes.AddNode<TestSumNode>(CreateScript<bool>());

        node.GetFeatureDependencies().ShouldBe([ScriptNodes]);
    }

    [Fact]
    public void ViewModel_IsCreatedThroughThePluginContainer()
    {
        NodeScript<bool> script = CreateScript<bool>();
        TestViewModelNode node = ScriptNodes.AddNode<TestViewModelNode>(script);

        ICustomNodeViewModel viewModel = node.GetCustomViewModel(script);

        TestNodeViewModel testViewModel = viewModel.ShouldBeOfType<TestNodeViewModel>();
        testViewModel.Node.ShouldBe(node);
        testViewModel.Script.ShouldBe(script);
        node.ViewModelPosition.ShouldBe(CustomNodeViewModelPosition.BetweenPinsTop);
    }

    [Fact]
    public void ViewModel_OfANodeWithoutNodeData_CannotBeCreated()
    {
        TestViewModelNode node = new();

        Should.Throw<ArtemisCoreException>(() => node.GetViewModel(CreateScript<bool>()));
    }

    #endregion
}

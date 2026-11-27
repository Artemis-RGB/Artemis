using Artemis.Core.Internal;
using Artemis.Core.Tests.Infrastructure.Plugins.Modules;
using Artemis.Core.Tests.Infrastructure.VisualScripting;

namespace Artemis.Core.Tests.VisualScripting;

/// <summary>
///     The start nodes event conditions put in their scripts, tested on their own
/// </summary>
public class EventConditionStartNodeTests : ScriptTestBase
{
    private readonly TestModule _module;

    public EventConditionStartNodeTests()
    {
        _module = Plugin.AddFeature(new TestModule());
    }

    [Fact]
    public void EventStartNode_IsADefaultNode()
    {
        EventConditionEventStartNode node = new();

        node.IsDefaultNode.ShouldBeTrue();
        node.Id.ShouldBe(EventConditionEventStartNode.NodeId);
        node.Name.ShouldBe("Event Arguments");
        node.Pins.ShouldBeEmpty();
    }

    [Fact]
    public void EventStartNode_CreatesAPinPerEventArgument()
    {
        EventConditionEventStartNode node = new();

        node.CreatePins(_module.DataModel.EventWithArgs);

        // The trigger time of the arguments is ignored
        OutputPin pin = node.Pins.ShouldHaveSingleItem().ShouldBeOfType<OutputPin>();
        pin.Name.ShouldBe("Value");
        pin.Type.ShouldBe(typeof(Numeric));
    }

    [Fact]
    public void EventStartNode_OutputsTheArgumentsOfTheLastEvent()
    {
        EventConditionEventStartNode node = new();
        node.CreatePins(_module.DataModel.EventWithArgs);
        OutputPin pin = (OutputPin) node.Pins.Single();
        pin.ConnectTo(new TestNode().CreateInputPin<Numeric>());

        _module.DataModel.EventWithArgs.Trigger(new TestEventArgs {Value = 4});
        node.Evaluate();

        pin.Value.ShouldBe(new Numeric(4));
    }

    [Fact]
    public void EventStartNode_BeforeAnyEvent_OutputsNothing()
    {
        EventConditionEventStartNode node = new();
        node.CreatePins(_module.DataModel.EventWithArgs);

        node.Evaluate();

        node.Pins.Single().IsEvaluated.ShouldBeFalse();
    }

    [Fact]
    public void EventStartNode_ForAnotherEvent_ReplacesItsPins()
    {
        EventConditionEventStartNode node = new();
        node.CreatePins(_module.DataModel.EventWithArgs);

        node.CreatePins(_module.DataModel.Event);
        node.Pins.ShouldBeEmpty();

        node.CreatePins(null);
        node.Pins.ShouldBeEmpty();
    }

    [Fact]
    public void ValueChangedStartNode_IsADefaultNodeWithTwoObjectPins()
    {
        EventConditionValueChangedStartNode node = new();

        node.IsDefaultNode.ShouldBeTrue();
        node.Id.ShouldBe(EventConditionValueChangedStartNode.NodeId);
        node.Pins.ShouldBe([node.NewValue, node.OldValue]);
        node.NewValue.Type.ShouldBe(typeof(object));
        node.OldValue.Type.ShouldBe(typeof(object));
    }

    [Theory]
    [InlineData("Integer", typeof(Numeric))]
    [InlineData("Float", typeof(Numeric))]
    [InlineData("Text", typeof(string))]
    [InlineData("Boolean", typeof(bool))]
    [InlineData("DoesNotExist", typeof(object))]
    public void ValueChangedStartNode_TakesTheTypeOfThePath(string path, Type expected)
    {
        EventConditionValueChangedStartNode node = new();
        using DataModelPath dataModelPath = new(_module.DataModel, path);

        node.UpdateOutputPins(dataModelPath);

        node.NewValue.Type.ShouldBe(expected);
        node.OldValue.Type.ShouldBe(expected);
    }

    [Fact]
    public void ValueChangedStartNode_OutputsNumbersAsNumeric()
    {
        EventConditionValueChangedStartNode node = new();
        using DataModelPath dataModelPath = new(_module.DataModel, "Integer");
        node.UpdateOutputPins(dataModelPath);

        node.UpdateValues(5, 3);
        node.Evaluate();

        node.NewValue.Value.ShouldBe(new Numeric(5));
        node.OldValue.Value.ShouldBe(new Numeric(3));
    }

    [Fact]
    public void ValueChangedStartNode_OutputsOtherValuesAsTheyAre()
    {
        EventConditionValueChangedStartNode node = new();
        using DataModelPath dataModelPath = new(_module.DataModel, "Text");
        node.UpdateOutputPins(dataModelPath);

        node.UpdateValues("new", null);
        node.Evaluate();

        node.NewValue.Value.ShouldBe("new");
        node.OldValue.Value.ShouldBeNull();
    }
}

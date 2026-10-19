using Artemis.Core.Tests.Infrastructure.VisualScripting;

namespace Artemis.Core.Tests.VisualScripting;

public class PinCollectionTests
{
    private readonly TestNode _node = new();
    private readonly TestNode _source = new("Source");

    [Fact]
    public void InputPinCollection_StartsWithTheRequestedNumberOfPins()
    {
        InputPinCollection<Numeric> collection = _node.CreateInputPinCollection<Numeric>("Values", 3);

        collection.Name.ShouldBe("Values");
        collection.Node.ShouldBe(_node);
        collection.Direction.ShouldBe(PinDirection.Input);
        collection.Type.ShouldBe(typeof(Numeric));
        collection.Count().ShouldBe(3);
        collection.Pins.ShouldAllBe(p => p.Node == _node && p.Direction == PinDirection.Input);
    }

    [Fact]
    public void OutputPinCollection_StartsWithTheRequestedNumberOfPins()
    {
        OutputPinCollection<string> collection = _node.CreateOutputPinCollection<string>("Texts", 2);

        collection.Direction.ShouldBe(PinDirection.Output);
        collection.Type.ShouldBe(typeof(string));
        collection.Pins.Count.ShouldBe(2);
        collection.Pins.ShouldAllBe(p => p is OutputPin<string>);
    }

    [Fact]
    public void CreatingACollection_AddsItToTheNode_AndRaisesPinCollectionAdded()
    {
        List<IPinCollection> added = [];
        _node.PinCollectionAdded += (_, e) => added.Add(e.Value);

        InputPinCollection<Numeric> first = _node.CreateInputPinCollection<Numeric>();
        InputPinCollection second = _node.CreateInputPinCollection(typeof(object));
        OutputPinCollection<Numeric> third = _node.CreateOutputPinCollection<Numeric>();

        _node.PinCollections.ShouldBe([first, second, third]);
        added.ShouldBe([first, second, third]);
        first.Count().ShouldBe(1);
    }

    [Fact]
    public void Add_AddsThePin_AndRaisesPinAdded()
    {
        InputPinCollection<Numeric> collection = _node.CreateInputPinCollection<Numeric>(initialCount: 0);
        List<IPin> added = [];
        collection.PinAdded += (_, e) => added.Add(e.Value);

        IPin pin = collection.CreatePin();
        collection.Add(pin);

        collection.Pins.ShouldBe([pin]);
        added.ShouldBe([pin]);
    }

    [Fact]
    public void CreatePin_DoesNotAddThePin()
    {
        InputPinCollection<Numeric> collection = _node.CreateInputPinCollection<Numeric>(initialCount: 0);

        IPin pin = collection.CreatePin();

        pin.ShouldBeOfType<InputPin<Numeric>>();
        collection.ShouldBeEmpty();
    }

    [Fact]
    public void Add_APinThatIsAlreadyInTheCollection_DoesNothing()
    {
        InputPinCollection<Numeric> collection = _node.CreateInputPinCollection<Numeric>();
        int added = 0;
        collection.PinAdded += (_, _) => added++;

        collection.Add(collection.Pins.First());

        collection.Count().ShouldBe(1);
        added.ShouldBe(0);
    }

    [Fact]
    public void Add_RejectsPinsOfTheOtherDirection()
    {
        InputPinCollection<Numeric> collection = _node.CreateInputPinCollection<Numeric>();

        Should.Throw<ArtemisCoreException>(() => collection.Add(_node.CreateOutputPin<Numeric>()));
    }

    [Fact]
    public void Add_RejectsPinsOfAnotherType()
    {
        InputPinCollection<Numeric> collection = _node.CreateInputPinCollection<Numeric>();

        Should.Throw<ArtemisCoreException>(() => collection.Add(_node.CreateInputPin<float>()));
    }

    [Fact]
    public void Remove_RemovesThePin_AndRaisesPinRemoved()
    {
        InputPinCollection<Numeric> collection = _node.CreateInputPinCollection<Numeric>(initialCount: 2);
        IPin pin = collection.Pins.ElementAt(1);
        List<IPin> removed = [];
        collection.PinRemoved += (_, e) => removed.Add(e.Value);

        collection.Remove(pin).ShouldBeTrue();

        collection.Count().ShouldBe(1);
        removed.ShouldBe([pin]);
    }

    [Fact]
    public void Remove_APinThatIsNotInTheCollection_ReturnsFalse()
    {
        InputPinCollection<Numeric> collection = _node.CreateInputPinCollection<Numeric>();
        int removed = 0;
        collection.PinRemoved += (_, _) => removed++;

        collection.Remove(collection.CreatePin()).ShouldBeFalse();

        removed.ShouldBe(0);
    }

    [Fact]
    public void Remove_LeavesTheConnectionsOfThePinAlone()
    {
        // The editor disconnects a pin itself before removing it (see RemovePin in Artemis.UI.Shared)
        InputPinCollection<Numeric> collection = _node.CreateInputPinCollection<Numeric>();
        OutputPin<Numeric> output = _source.CreateOutputPin<Numeric>();
        IPin pin = collection.Pins.First();
        pin.ConnectTo(output);

        collection.Remove(pin);

        pin.ConnectedTo.ShouldBe([output]);
        output.ConnectedTo.ShouldBe([pin]);
    }

    [Fact]
    public void Reset_ResetsEveryPin()
    {
        OutputPinCollection<Numeric> collection = _node.CreateOutputPinCollection<Numeric>(initialCount: 2);
        foreach (OutputPin<Numeric> pin in collection.Cast<OutputPin<Numeric>>())
            pin.Value = new Numeric(1);

        collection.Reset();

        collection.ShouldAllBe(p => !p.IsEvaluated);
    }

    [Fact]
    public void Node_Reset_ResetsItsPinCollections()
    {
        OutputPinCollection<Numeric> collection = _node.CreateOutputPinCollection<Numeric>();
        ((OutputPin<Numeric>) collection.Pins[0]).Value = new Numeric(1);

        _node.Reset();

        collection.Pins[0].IsEvaluated.ShouldBeFalse();
    }

    [Fact]
    public void TypedValues_LeaveOutPinsWithoutAValue()
    {
        InputPinCollection<string> collection = _node.CreateInputPinCollection<string>(initialCount: 3);
        OutputPin<string> output = _source.CreateOutputPin<string>();
        collection.Pins.ElementAt(1).ConnectTo(output);
        output.Value = "Hello";

        _node.Reset();
        collection.Values.ShouldBe(["Hello"]);
    }

    [Fact]
    public void TypedValues_ForAValueType_IncludeDefaultsOfUnconnectedPins()
    {
        InputPinCollection<Numeric> collection = _node.CreateInputPinCollection<Numeric>(initialCount: 2);
        OutputPin<Numeric> output = _source.CreateOutputPin<Numeric>();
        collection.Pins.ElementAt(0).ConnectTo(output);
        output.Value = new Numeric(4);

        _node.Reset();
        collection.Values.ShouldBe([new Numeric(4), new Numeric(0)]);
    }

    [Fact]
    public void UntypedCollection_CreatesPinsOfItsType()
    {
        InputPinCollection collection = _node.CreateInputPinCollection(typeof(string), "Texts", 2);

        collection.Type.ShouldBe(typeof(string));
        collection.Pins.ShouldAllBe(p => p is InputPin && p.Type == typeof(string));
    }

    [Fact]
    public void UntypedValues_LeaveOutPinsWithoutAValue()
    {
        InputPinCollection collection = _node.CreateInputPinCollection(typeof(object), initialCount: 2);
        OutputPin<Numeric> output = _source.CreateOutputPin<Numeric>();
        collection.Pins[1].ConnectTo(output);
        output.Value = new Numeric(4);

        collection.Values.Cast<object>().ShouldBe([new Numeric(4)]);
    }

    [Fact]
    public void UntypedCollection_ChangeType_ChangesEveryPin_AndDropsIncompatibleConnections()
    {
        InputPinCollection collection = _node.CreateInputPinCollection(typeof(object), initialCount: 2);
        OutputPin<Numeric> number = _source.CreateOutputPin<Numeric>();
        OutputPin<string> text = _source.CreateOutputPin<string>();
        collection.Pins[0].ConnectTo(number);
        collection.Pins[1].ConnectTo(text);
        List<string?> changed = [];
        collection.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        collection.ChangeType(typeof(Numeric));

        collection.Type.ShouldBe(typeof(Numeric));
        collection.ShouldAllBe(p => p.Type == typeof(Numeric));
        collection.Pins[0].ConnectedTo.ShouldBe([number]);
        collection.Pins[1].ConnectedTo.ShouldBeEmpty();
        collection.CreatePin().Type.ShouldBe(typeof(Numeric));
        changed.ShouldBe([nameof(IPinCollection.Type)]);
    }

    [Fact]
    public void UntypedCollection_ChangeType_ToTheSameType_DoesNothing()
    {
        InputPinCollection collection = _node.CreateInputPinCollection(typeof(object));
        int changed = 0;
        collection.PropertyChanged += (_, _) => changed++;

        collection.ChangeType(typeof(object));

        changed.ShouldBe(0);
    }

    [Fact]
    public void RemovePinCollection_DisconnectsItsPins_AndRaisesPinCollectionRemoved()
    {
        InputPinCollection<Numeric> collection = _node.CreateInputPinCollection<Numeric>();
        OutputPin<Numeric> output = _source.CreateOutputPin<Numeric>();
        collection.Pins.Single().ConnectTo(output);
        List<IPinCollection> removed = [];
        _node.PinCollectionRemoved += (_, e) => removed.Add(e.Value);

        _node.RemovePinCollection(collection).ShouldBeTrue();

        _node.PinCollections.ShouldBeEmpty();
        output.ConnectedTo.ShouldBeEmpty();
        removed.ShouldBe([collection]);
    }

    [Fact]
    public void RemovePinCollection_OfAnotherNode_ReturnsFalse()
    {
        InputPinCollection<Numeric> collection = _source.CreateInputPinCollection<Numeric>();

        _node.RemovePinCollection(collection).ShouldBeFalse();
    }
}

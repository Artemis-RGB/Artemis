using System.ComponentModel;
using Artemis.Core.Tests.Infrastructure.Plugins.Modules;
using Artemis.Core.Tests.Infrastructure.VisualScripting;

namespace Artemis.Core.Tests.VisualScripting;

public class PinTests
{
    private readonly TestNode _source = new("Source");
    private readonly TestNode _target = new("Target");

    #region Connecting

    [Fact]
    public void ConnectTo_ConnectsBothPins()
    {
        OutputPin<Numeric> output = _source.CreateOutputPin<Numeric>();
        InputPin<Numeric> input = _target.CreateInputPin<Numeric>();

        input.ConnectTo(output);

        input.ConnectedTo.ShouldBe([output]);
        output.ConnectedTo.ShouldBe([input]);
    }

    [Fact]
    public void ConnectTo_RaisesPinConnectedOnBothPins()
    {
        OutputPin<Numeric> output = _source.CreateOutputPin<Numeric>();
        InputPin<Numeric> input = _target.CreateInputPin<Numeric>();
        List<IPin> inputConnected = [];
        List<IPin> outputConnected = [];
        input.PinConnected += (_, e) => inputConnected.Add(e.Value);
        output.PinConnected += (_, e) => outputConnected.Add(e.Value);

        input.ConnectTo(output);

        inputConnected.ShouldBe([output]);
        outputConnected.ShouldBe([input]);
    }

    [Fact]
    public void ConnectTo_RaisesPropertyChangedForConnectedTo()
    {
        OutputPin<Numeric> output = _source.CreateOutputPin<Numeric>();
        InputPin<Numeric> input = _target.CreateInputPin<Numeric>();
        List<string?> changed = [];
        input.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        input.ConnectTo(output);

        changed.ShouldContain(nameof(IPin.ConnectedTo));
    }

    [Fact]
    public void Output_CanFeedSeveralInputs()
    {
        OutputPin<Numeric> output = _source.CreateOutputPin<Numeric>();
        InputPin<Numeric> first = _target.CreateInputPin<Numeric>();
        InputPin<Numeric> second = _target.CreateInputPin<Numeric>();
        _source.OnEvaluate = _ => output.Value = new Numeric(3);

        first.ConnectTo(output);
        second.ConnectTo(output);

        output.ConnectedTo.ShouldBe([first, second]);
        _target.Reset();
        first.Value.ShouldBe(new Numeric(3));
        second.Value.ShouldBe(new Numeric(3));
    }

    [Fact]
    public void ConnectTo_DoesNotCheckTypesOrDirections()
    {
        // The editor decides what may be connected, in code anything goes
        InputPin<string> text = _source.CreateInputPin<string>();
        InputPin<Numeric> number = _target.CreateInputPin<Numeric>();

        text.ConnectTo(number);

        text.ConnectedTo.ShouldBe([number]);
        number.ConnectedTo.ShouldBe([text]);
    }

    [Fact]
    public void ConnectTo_DoesNotLimitInputsToOneConnection()
    {
        // The editor disconnects an input before connecting it again (see ConnectPins in Artemis.UI.Shared). An input
        // with several connections reads from the first.
        OutputPin<Numeric> first = _source.CreateOutputPin<Numeric>();
        OutputPin<Numeric> second = _source.CreateOutputPin<Numeric>();
        InputPin<Numeric> input = _target.CreateInputPin<Numeric>();
        _source.OnEvaluate = _ =>
        {
            first.Value = new Numeric(1);
            second.Value = new Numeric(2);
        };

        input.ConnectTo(first);
        input.ConnectTo(second);

        input.ConnectedTo.ShouldBe([first, second]);
        _target.Reset();
        input.Value.ShouldBe(new Numeric(1));
    }

    [Fact]
    public void ConnectTo_TheSamePinTwice_ConnectsThemOnce()
    {
        OutputPin<Numeric> output = _source.CreateOutputPin<Numeric>();
        InputPin<Numeric> input = _target.CreateInputPin<Numeric>();

        input.ConnectTo(output);
        input.ConnectTo(output);

        input.ConnectedTo.ShouldBe([output]);
        output.ConnectedTo.ShouldBe([input]);

        input.DisconnectFrom(output);

        input.ConnectedTo.ShouldBeEmpty();
        output.ConnectedTo.ShouldBeEmpty();
    }

    [Fact]
    public void DisconnectFrom_DisconnectsBothPins_AndRaisesPinDisconnectedOnBoth()
    {
        OutputPin<Numeric> output = _source.CreateOutputPin<Numeric>();
        InputPin<Numeric> input = _target.CreateInputPin<Numeric>();
        input.ConnectTo(output);
        List<IPin> inputDisconnected = [];
        List<IPin> outputDisconnected = [];
        input.PinDisconnected += (_, e) => inputDisconnected.Add(e.Value);
        output.PinDisconnected += (_, e) => outputDisconnected.Add(e.Value);

        input.DisconnectFrom(output);

        input.ConnectedTo.ShouldBeEmpty();
        output.ConnectedTo.ShouldBeEmpty();
        inputDisconnected.ShouldBe([output]);
        outputDisconnected.ShouldBe([input]);
    }

    [Fact]
    public void DisconnectAll_DisconnectsEveryConnection()
    {
        OutputPin<Numeric> output = _source.CreateOutputPin<Numeric>();
        InputPin<Numeric> first = _target.CreateInputPin<Numeric>();
        InputPin<Numeric> second = _target.CreateInputPin<Numeric>();
        output.ConnectTo(first);
        output.ConnectTo(second);
        List<IPin> outputDisconnected = [];
        int firstDisconnected = 0;
        output.PinDisconnected += (_, e) => outputDisconnected.Add(e.Value);
        first.PinDisconnected += (_, _) => firstDisconnected++;

        output.DisconnectAll();

        output.ConnectedTo.ShouldBeEmpty();
        first.ConnectedTo.ShouldBeEmpty();
        second.ConnectedTo.ShouldBeEmpty();
        outputDisconnected.ShouldBe([first, second]);
        firstDisconnected.ShouldBe(1);
    }

    [Fact]
    public void DisconnectAll_OnAPinWithoutConnections_RaisesNothing()
    {
        InputPin<Numeric> input = _target.CreateInputPin<Numeric>();
        int raised = 0;
        input.PinDisconnected += (_, _) => raised++;
        input.PropertyChanged += (_, _) => raised++;

        input.DisconnectAll();

        raised.ShouldBe(0);
    }

    #endregion

    #region Type compatibility

    [Theory]
    [InlineData(typeof(Numeric), typeof(Numeric), true)]
    [InlineData(typeof(Numeric), typeof(float), false)]
    [InlineData(typeof(Numeric), typeof(int), false)]
    [InlineData(typeof(object), typeof(string), true)]
    [InlineData(typeof(object), typeof(Numeric), true)]
    [InlineData(typeof(string), typeof(object), false)]
    [InlineData(typeof(Enum), typeof(TestEnum), true)]
    [InlineData(typeof(TestEnum), typeof(Enum), false)]
    [InlineData(typeof(TestEnum), typeof(DayOfWeek), false)]
    public void InputPin_AcceptsValuesThatFitItsType(Type pinType, Type valueType, bool compatible)
    {
        InputPin pin = _target.CreateInputPin(pinType);

        pin.IsTypeCompatible(valueType).ShouldBe(compatible);
    }

    [Theory]
    [InlineData(typeof(Numeric), typeof(Numeric), true)]
    [InlineData(typeof(Numeric), typeof(float), false)]
    [InlineData(typeof(string), typeof(object), true)]
    [InlineData(typeof(object), typeof(string), false)]
    [InlineData(typeof(TestEnum), typeof(Enum), true)]
    [InlineData(typeof(Enum), typeof(TestEnum), false)]
    [InlineData(typeof(TestEnum), typeof(DayOfWeek), false)]
    public void OutputPin_FitsInputsThatTakeItsType(Type pinType, Type inputType, bool compatible)
    {
        OutputPin pin = _source.CreateOutputPin(pinType);

        pin.IsTypeCompatible(inputType).ShouldBe(compatible);
    }

    [Fact]
    public void IsTypeCompatible_OnlyMatchesASpecificEnumToAnyEnumWhenForgiving()
    {
        InputPin input = _target.CreateInputPin(typeof(Enum));
        OutputPin output = _source.CreateOutputPin(typeof(TestEnum));

        input.IsTypeCompatible(typeof(TestEnum), false).ShouldBeFalse();
        output.IsTypeCompatible(typeof(Enum), false).ShouldBeFalse();
        input.IsTypeCompatible(typeof(TestEnum), true).ShouldBeTrue();
        output.IsTypeCompatible(typeof(Enum), true).ShouldBeTrue();
    }

    [Fact]
    public void Pins_KnowWhetherTheyAreNumeric()
    {
        _target.CreateInputPin<Numeric>().IsNumeric.ShouldBeTrue();
        _target.CreateInputPin(typeof(Numeric)).IsNumeric.ShouldBeTrue();
        _source.CreateOutputPin<Numeric>().IsNumeric.ShouldBeTrue();
        _source.CreateOutputPin(typeof(Numeric)).IsNumeric.ShouldBeTrue();
        _target.CreateInputPin<float>().IsNumeric.ShouldBeFalse();
        _source.CreateOutputPin(typeof(int)).IsNumeric.ShouldBeFalse();
    }

    #endregion

    #region Values

    [Fact]
    public void InputPin_WithoutConnection_HasTheDefaultValue()
    {
        _target.CreateInputPin<Numeric>().Value.ShouldBe(new Numeric(0));
        _target.CreateInputPin<string>().Value.ShouldBeNull();
        _target.CreateInputPin(typeof(Numeric)).Value.ShouldBe(new Numeric(0));
        _target.CreateInputPin(typeof(bool)).Value.ShouldBe(false);
        _target.CreateInputPin(typeof(string)).Value.ShouldBeNull();
    }

    [Fact]
    public void ReadingAnInput_EvaluatesTheNodeOfTheConnectedOutput()
    {
        OutputPin<Numeric> output = _source.CreateOutputPin<Numeric>();
        InputPin<Numeric> input = _target.CreateInputPin<Numeric>();
        input.ConnectTo(output);
        _source.OnEvaluate = _ => output.Value = new Numeric(5);

        _target.Reset();
        input.Value.ShouldBe(new Numeric(5));
        _source.Evaluations.ShouldBe(1);
    }

    [Fact]
    public void Values_AreKeptUntilThePinsAreReset()
    {
        OutputPin<Numeric> output = _source.CreateOutputPin<Numeric>();
        InputPin<Numeric> input = _target.CreateInputPin<Numeric>();
        input.ConnectTo(output);
        float value = 1;
        _source.OnEvaluate = _ => output.Value = new Numeric(value);
        _target.Reset();
        input.Value.ShouldBe(new Numeric(1));

        value = 2;
        input.Value.ShouldBe(new Numeric(1));
        _source.Evaluations.ShouldBe(1);

        _source.Reset();
        _target.Reset();
        input.Value.ShouldBe(new Numeric(2));
        _source.Evaluations.ShouldBe(2);
    }

    [Fact]
    public void Reset_MarksThePinAsNotEvaluated()
    {
        OutputPin<Numeric> output = _source.CreateOutputPin<Numeric>();
        output.Value = new Numeric(1);
        output.IsEvaluated.ShouldBeTrue();

        output.Reset();

        output.IsEvaluated.ShouldBeFalse();
    }

    [Fact]
    public void SettingAValue_RaisesPropertyChangedForPinValue()
    {
        OutputPin<Numeric> output = _source.CreateOutputPin<Numeric>();
        List<string?> changed = [];
        output.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        output.Value = new Numeric(1);

        changed.ShouldContain(nameof(IPin.PinValue));
        output.PinValue.ShouldBe(new Numeric(1));
    }

    [Fact]
    public void TypedInput_DoesNotConvertOtherNumberTypes()
    {
        // Numbers only flow between pins as Numeric, a Numeric input connected to a float output reads the default value
        OutputPin<float> output = _source.CreateOutputPin<float>();
        InputPin<Numeric> input = _target.CreateInputPin<Numeric>();
        input.ConnectTo(output);
        output.Value = 5f;

        input.Value.ShouldBe(new Numeric(0));
    }

    [Fact]
    public void TypedInput_ReadsAMatchingValueFromAnObjectOutput()
    {
        OutputPin output = _source.CreateOutputPin(typeof(object));
        InputPin<Numeric> input = _target.CreateInputPin<Numeric>();
        input.ConnectTo(output);
        output.Value = new Numeric(5);

        _target.Reset();
        input.Value.ShouldBe(new Numeric(5));

        _target.Reset();
        output.Value = 5;
        input.Value.ShouldBe(new Numeric(0));
    }

    [Fact]
    public void ObjectInput_ReadsAnyValue()
    {
        OutputPin<Numeric> output = _source.CreateOutputPin<Numeric>();
        InputPin input = _target.CreateInputPin(typeof(object));
        input.ConnectTo(output);
        output.Value = new Numeric(5);

        input.Value.ShouldBe(new Numeric(5));
    }

    [Fact]
    public void UntypedInput_ReadingAValueOfTheWrongType_Throws()
    {
        OutputPin<float> output = _source.CreateOutputPin<float>();
        InputPin input = _target.CreateInputPin(typeof(Numeric));
        input.ConnectTo(output);
        output.Value = 5f;

        Should.Throw<ArgumentException>(() => input.Value);
    }

    [Fact]
    public void UntypedInput_ForAValueType_TurnsNullIntoTheDefaultValue()
    {
        OutputPin output = _source.CreateOutputPin(typeof(object));
        InputPin input = _target.CreateInputPin(typeof(int));
        input.ConnectTo(output);
        output.Value = null;

        input.Value.ShouldBe(0);
    }

    [Fact]
    public void UntypedInput_ForAReferenceType_PassesNullOn()
    {
        OutputPin output = _source.CreateOutputPin(typeof(string));
        InputPin input = _target.CreateInputPin(typeof(string));
        input.ConnectTo(output);
        output.Value = null;

        input.Value.ShouldBeNull();
    }

    [Fact]
    public void UntypedOutput_RejectsValuesOfTheWrongType()
    {
        OutputPin output = _source.CreateOutputPin(typeof(Numeric));

        Should.Throw<ArgumentException>(() => output.Value = 5f);
    }

    [Fact]
    public void UntypedOutput_ForAValueType_TurnsNullIntoTheDefaultValue()
    {
        OutputPin output = _source.CreateOutputPin(typeof(Numeric));
        output.Value = new Numeric(3);

        output.Value = null;

        output.Value.ShouldBe(new Numeric(0));
    }

    [Fact]
    public void NewTypedInput_ReadsItsConnection_LikeAnUntypedInput()
    {
        OutputPin<Numeric> output = _source.CreateOutputPin<Numeric>();
        InputPin<Numeric> typed = _target.CreateInputPin<Numeric>();
        InputPin untyped = _target.CreateInputPin(typeof(Numeric));
        typed.ConnectTo(output);
        untyped.ConnectTo(output);
        output.Value = new Numeric(5);

        typed.IsEvaluated.ShouldBeFalse();
        typed.Value.ShouldBe(new Numeric(5));
        untyped.Value.ShouldBe(new Numeric(5));
    }

    [Fact]
    public void UntypedOutput_StartsWithTheDefaultValueOfItsType()
    {
        _source.CreateOutputPin(typeof(Numeric)).Value.ShouldBe(new Numeric(0));
        _source.CreateOutputPin(typeof(string)).Value.ShouldBeNull();
    }

    #endregion

    #region Changing types

    [Fact]
    public void InputPin_ChangeType_KeepsCompatibleConnectionsOnly()
    {
        OutputPin<Numeric> number = _source.CreateOutputPin<Numeric>();
        OutputPin<string> text = _source.CreateOutputPin<string>();
        InputPin first = _target.CreateInputPin(typeof(object));
        InputPin second = _target.CreateInputPin(typeof(object));
        first.ConnectTo(number);
        second.ConnectTo(text);

        first.ChangeType(typeof(Numeric));
        second.ChangeType(typeof(Numeric));

        first.Type.ShouldBe(typeof(Numeric));
        first.IsNumeric.ShouldBeTrue();
        first.ConnectedTo.ShouldBe([number]);
        second.ConnectedTo.ShouldBeEmpty();
        text.ConnectedTo.ShouldBeEmpty();
    }

    [Fact]
    public void InputPin_ChangeType_RaisesPropertyChangedForType()
    {
        InputPin input = _target.CreateInputPin(typeof(object));
        List<string?> changed = [];
        input.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        input.ChangeType(typeof(Numeric));

        changed.ShouldContain(nameof(IPin.Type));
        changed.ShouldContain(nameof(IPin.IsNumeric));
    }

    [Fact]
    public void ChangeType_ToTheSameType_LeavesConnectionsAlone()
    {
        OutputPin output = _source.CreateOutputPin(typeof(Numeric));
        InputPin<Numeric> input = _target.CreateInputPin<Numeric>();
        input.ConnectTo(output);
        int disconnects = 0;
        output.PinDisconnected += (_, _) => disconnects++;

        output.ChangeType(typeof(Numeric));

        disconnects.ShouldBe(0);
        output.ConnectedTo.ShouldBe([input]);
    }

    [Fact]
    public void OutputPin_ChangeType_KeepsCompatibleConnectionsOnly_AndResetsItsValue()
    {
        OutputPin output = _source.CreateOutputPin(typeof(object));
        InputPin<object> objectInput = _target.CreateInputPin<object>();
        InputPin<Numeric> numericInput = _target.CreateInputPin<Numeric>();
        output.ConnectTo(objectInput);
        output.ConnectTo(numericInput);
        output.Value = "Hello";

        output.ChangeType(typeof(string));

        output.Type.ShouldBe(typeof(string));
        output.ConnectedTo.ShouldBe([objectInput]);
        numericInput.ConnectedTo.ShouldBeEmpty();
        output.Value.ShouldBeNull();
    }

    [Fact]
    public void OutputPin_ChangeType_ToAValueType_UsesItsDefaultValue()
    {
        OutputPin output = _source.CreateOutputPin(typeof(object));

        output.ChangeType(typeof(Numeric));

        output.Value.ShouldBe(new Numeric(0));
        output.IsNumeric.ShouldBeTrue();
    }

    #endregion

    [Fact]
    public void Pin_KnowsItsNodeNameAndDirection()
    {
        InputPin<Numeric> input = _target.CreateInputPin<Numeric>("Amount");
        OutputPin output = _source.CreateOutputPin(typeof(string), "Text");

        input.Node.ShouldBe(_target);
        input.Name.ShouldBe("Amount");
        input.Direction.ShouldBe(PinDirection.Input);
        input.Type.ShouldBe(typeof(Numeric));
        output.Node.ShouldBe(_source);
        output.Name.ShouldBe("Text");
        output.Direction.ShouldBe(PinDirection.Output);
        output.Type.ShouldBe(typeof(string));
    }

    [Fact]
    public void Pin_RaisesPropertyChangedWhenRenamed()
    {
        InputPin<Numeric> input = _target.CreateInputPin<Numeric>("Amount");
        PropertyChangedEventArgs? args = null;
        input.PropertyChanged += (_, e) => args = e;

        input.Name = "Total";

        args.ShouldNotBeNull().PropertyName.ShouldBe(nameof(IPin.Name));
    }
}

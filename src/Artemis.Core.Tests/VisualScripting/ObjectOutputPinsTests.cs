using Artemis.Core.Modules;
using Artemis.Core.Tests.Infrastructure.VisualScripting;

namespace Artemis.Core.Tests.VisualScripting;

/// <remarks>
///     <see cref="ScriptTestBase" /> registers a type color for text, so text properties get a pin
/// </remarks>
public class ObjectOutputPinsTests : ScriptTestBase
{
    private readonly TestNode _node = new();
    private readonly TestNode _reader = new("Reader");

    [Fact]
    public void ChangeType_ToAnObject_CreatesAPinPerUsableProperty()
    {
        ObjectOutputPins pins = new(_node);

        pins.ChangeType(typeof(TestObject));

        pins.CurrentType.ShouldBe(typeof(TestObject));
        pins.Pins.Select(p => (p.Name, p.Type)).ShouldBe([
            ("Number", typeof(Numeric)),
            ("Some amount", typeof(Numeric)),
            ("Text", typeof(string))
        ]);
        _node.Pins.ShouldBe(pins.Pins);
    }

    [Fact]
    public void ChangeType_SkipsIgnoredProperties_AndPropertiesWithoutATypeColor()
    {
        ObjectOutputPins pins = new(_node);

        pins.ChangeType(typeof(TestObject));

        pins.Pins.ShouldNotContain(p => p.Name == "Hidden");
        pins.Pins.ShouldNotContain(p => p.Name == "Flag");
    }

    [Fact]
    public void ChangeType_ToANumberType_CreatesASingleNumericPin()
    {
        ObjectOutputPins pins = new(_node);

        pins.ChangeType(typeof(int));

        OutputPin pin = pins.Pins.ShouldHaveSingleItem();
        pin.Name.ShouldBe("Item");
        pin.Type.ShouldBe(typeof(Numeric));
    }

    [Fact]
    public void ChangeType_ToText_CreatesASingleTextPin()
    {
        ObjectOutputPins pins = new(_node);

        pins.ChangeType(typeof(string));

        pins.Pins.ShouldHaveSingleItem().Type.ShouldBe(typeof(string));
    }

    [Fact]
    public void ChangeType_ToNothing_RemovesThePins()
    {
        ObjectOutputPins pins = new(_node);
        pins.ChangeType(typeof(TestObject));

        pins.ChangeType(null);

        pins.CurrentType.ShouldBeNull();
        pins.Pins.ShouldBeEmpty();
        _node.Pins.ShouldBeEmpty();
    }

    [Fact]
    public void ChangeType_ToTheSameType_KeepsThePins()
    {
        ObjectOutputPins pins = new(_node);
        pins.ChangeType(typeof(TestObject));
        List<OutputPin> before = pins.Pins.ToList();

        pins.ChangeType(typeof(TestObject));

        pins.Pins.ShouldBe(before);
    }

    [Fact]
    public void ChangeType_ReusesThePinsItRemoved()
    {
        ObjectOutputPins pins = new(_node);
        pins.ChangeType(typeof(TestObject));
        OutputPin first = pins.Pins[0];

        pins.ChangeType(typeof(string));

        pins.Pins.ShouldHaveSingleItem().ShouldBeSameAs(first);
        first.Name.ShouldBe("Item");
        first.Type.ShouldBe(typeof(string));
    }

    [Fact]
    public void SetCurrentValue_ForANumberType_OutputsANumeric()
    {
        ObjectOutputPins pins = new(_node);
        pins.ChangeType(typeof(int));

        pins.SetCurrentValue(5);

        pins.Pins[0].Value.ShouldBe(new Numeric(5));
    }

    [Fact]
    public void SetCurrentValue_ForANumberType_TurnsNullIntoZero()
    {
        ObjectOutputPins pins = new(_node);
        pins.ChangeType(typeof(int));
        pins.SetCurrentValue(5);

        pins.SetCurrentValue(null);

        pins.Pins[0].Value.ShouldBe(new Numeric(0));
    }

    [Fact]
    public void SetCurrentValue_ForAnObject_OnlyFillsConnectedPins()
    {
        ObjectOutputPins pins = new(_node);
        pins.ChangeType(typeof(TestObject));
        OutputPin number = pins.Pins.Single(p => p.Name == "Number");
        OutputPin text = pins.Pins.Single(p => p.Name == "Text");
        number.ConnectTo(_reader.CreateInputPin<Numeric>());

        pins.SetCurrentValue(new TestObject {Number = 3, Text = "Hello"});

        number.Value.ShouldBe(new Numeric(3));
        text.Value.ShouldBeNull();
    }

    [Fact]
    public void SetCurrentValue_ForAnObject_TurnsNullIntoDefaultValues()
    {
        ObjectOutputPins pins = new(_node);
        pins.ChangeType(typeof(TestObject));
        OutputPin number = pins.Pins.Single(p => p.Name == "Number");
        OutputPin text = pins.Pins.Single(p => p.Name == "Text");
        number.ConnectTo(_reader.CreateInputPin<Numeric>());
        text.ConnectTo(_reader.CreateInputPin<string>());
        pins.SetCurrentValue(new TestObject {Number = 3, Text = "Hello"});

        pins.SetCurrentValue(null);

        number.Value.ShouldBe(new Numeric(0));
        text.Value.ShouldBeNull();
    }

    [Fact]
    public void SetCurrentValue_BeforeChoosingAType_Throws()
    {
        ObjectOutputPins pins = new(_node);

        Should.Throw<ArtemisCoreException>(() => pins.SetCurrentValue(5));
    }

    [Fact]
    public void SetCurrentValue_OfAnotherType_Throws()
    {
        ObjectOutputPins pins = new(_node);
        pins.ChangeType(typeof(TestObject));

        Should.Throw<ArtemisCoreException>(() => pins.SetCurrentValue("Hello"));
    }

    public class TestObject
    {
        public int Number { get; set; }
        public double SomeAmount { get; set; }
        public string? Text { get; set; }
        public bool Flag { get; set; }

        [DataModelIgnore]
        public int Hidden { get; set; }
    }
}

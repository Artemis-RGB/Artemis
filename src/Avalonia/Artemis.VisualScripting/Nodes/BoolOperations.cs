using System.Collections;
using Artemis.Core;
using Artemis.VisualScripting.Nodes.CustomViewModels;

namespace Artemis.VisualScripting.Nodes;

[Node("Greater than", "Checks if the first input is greater than the second.", "Operators", InputType = typeof(object), OutputType = typeof(bool))]
public class GreaterThanNode : Node
{
    #region Constructors

    public GreaterThanNode()
        : base("Greater than", "Checks if the first input is greater than the second.")
    {
        Input1 = CreateInputPin<object>();
        Input2 = CreateInputPin<object>();
        Result = CreateOutputPin<bool>();
    }

    #endregion

    #region Methods

    public override void Evaluate()
    {
        if (Input1.Value is Numeric numeric1 && Input2.Value is Numeric numeric2)
        {
            Result.Value = numeric1 > numeric2;
            return;
        }

        if (Input2.Value != null && Input1.Value != null && Input1.Value.IsNumber() && Input2.Value.IsNumber())
        {
            Result.Value = Convert.ToSingle(Input1.Value) > Convert.ToSingle(Input2.Value);
            return;
        }

        try
        {
            Result.Value = Comparer.DefaultInvariant.Compare(Input1.Value, Input2.Value) == 1;
        }
        catch
        {
            Result.Value = false;
        }
    }

    #endregion

    #region Properties & Fields

    public InputPin<object> Input1 { get; }
    public InputPin<object> Input2 { get; }

    public OutputPin<bool> Result { get; }

    #endregion
}

[Node("Less than", "Checks if the first input is less than the second.", "Operators", InputType = typeof(object), OutputType = typeof(bool))]
public class LessThanNode : Node
{
    #region Constructors

    public LessThanNode()
        : base("Less than", "Checks if the first input is less than the second.")
    {
        Input1 = CreateInputPin<object>();
        Input2 = CreateInputPin<object>();
        Result = CreateOutputPin<bool>();
    }

    #endregion

    #region Methods

    public override void Evaluate()
    {
        if (Input1.Value is Numeric numeric1 && Input2.Value is Numeric numeric2)
        {
            Result.Value = numeric1 < numeric2;
            return;
        }

        if (Input2.Value != null && Input1.Value != null && Input1.Value.IsNumber() && Input2.Value.IsNumber())
        {
            Result.Value = Convert.ToSingle(Input1.Value) < Convert.ToSingle(Input2.Value);
            return;
        }

        try
        {
            Result.Value = Comparer.DefaultInvariant.Compare(Input1.Value, Input2.Value) == -1;
        }
        catch
        {
            Result.Value = false;
        }
    }

    #endregion

    #region Properties & Fields

    public InputPin<object> Input1 { get; }
    public InputPin<object> Input2 { get; }

    public OutputPin<bool> Result { get; }

    #endregion
}

[Node("Equals", "Checks if the two inputs are equals.", "Operators", InputType = typeof(bool), OutputType = typeof(bool))]
public class EqualsNode : Node
{
    #region Constructors

    public EqualsNode()
        : base("Equals", "Checks if the two inputs are equals.")
    {
        Input1 = CreateInputPin<object>();
        Input2 = CreateInputPin<object>();
        Result = CreateOutputPin<bool>();
    }

    #endregion

    #region Methods

    public override void Evaluate()
    {
        try
        {
            Result.Value = Equals(Input1.Value, Input2.Value);
        }
        catch
        {
            Result.Value = false;
        }
    }

    #endregion

    #region Properties & Fields

    public InputPin<object> Input1 { get; }
    public InputPin<object> Input2 { get; }

    public OutputPin<bool> Result { get; }

    #endregion
}

[Node("Negate", "Negates the boolean.", "Operators", InputType = typeof(bool), OutputType = typeof(bool))]
public class NegateNode : Node
{
    #region Constructors

    public NegateNode()
        : base("Negate", "Negates the boolean.")
    {
        Input = CreateInputPin<bool>();
        Output = CreateOutputPin<bool>();
    }

    #endregion

    #region Methods

    public override void Evaluate()
    {
        Output.Value = !Input.Value;
    }

    #endregion

    #region Properties & Fields

    public InputPin<bool> Input { get; }
    public OutputPin<bool> Output { get; }

    #endregion
}

[Node("And", "Checks if all inputs are true.", "Operators", InputType = typeof(bool), OutputType = typeof(bool))]
public class AndNode : Node
{
    #region Constructors

    public AndNode()
        : base("And", "Checks if all inputs are true.")
    {
        Input = CreateInputPinCollection<bool>();
        Result = CreateOutputPin<bool>();
    }

    #endregion

    #region Methods

    public override void Evaluate()
    {
        Result.Value = Input.Values.All(v => v);
    }

    #endregion

    #region Properties & Fields

    public InputPinCollection<bool> Input { get; set; }
    public OutputPin<bool> Result { get; }

    #endregion
}

[Node("Or", "Checks if any inputs are true.", "Operators", InputType = typeof(bool), OutputType = typeof(bool))]
public class OrNode : Node
{
    #region Constructors

    public OrNode()
        : base("Or", "Checks if any inputs are true.")
    {
        Input = CreateInputPinCollection<bool>();
        Result = CreateOutputPin<bool>();
    }

    #endregion

    #region Methods

    public override void Evaluate()
    {
        Result.Value = Input.Values.Any(v => v);
    }

    #endregion

    #region Properties & Fields

    public InputPinCollection<bool> Input { get; set; }
    public OutputPin<bool> Result { get; }

    #endregion
}

[Node("Exclusive Or", "Checks if one of the inputs is true.", "Operators", InputType = typeof(bool), OutputType = typeof(bool))]
public class XorNode : Node
{
    #region Constructors

    public XorNode()
        : base("Exclusive Or", "Checks if one of the inputs is true.")
    {
        Input = CreateInputPinCollection<bool>();
        Result = CreateOutputPin<bool>();
    }

    #endregion

    #region Methods

    public override void Evaluate()
    {
        Result.Value = Input.Values.Count(v => v) == 1;
    }

    #endregion

    #region Properties & Fields

    public InputPinCollection<bool> Input { get; set; }
    public OutputPin<bool> Result { get; }

    #endregion
}

[Node("Enum Equals", "Determines the equality between an input and a selected enum value", "Operators", InputType = typeof(Enum), OutputType = typeof(bool))]
public class EnumEqualsNode : Node<Enum, EnumEqualsNodeCustomViewModel>
{
    public EnumEqualsNode() : base("Enum Equals", "Determines the equality between an input and a selected enum value")
    {
        InputPin = CreateInputPin<Enum>();
        OutputPin = CreateOutputPin<bool>();
    }

    public InputPin<Enum> InputPin { get; }
    public OutputPin<bool> OutputPin { get; }

    #region Overrides of Node

    /// <inheritdoc />
    public override void Evaluate()
    {
        OutputPin.Value = InputPin.Value != null && InputPin.Value.Equals(Storage);
    }

    #endregion
}
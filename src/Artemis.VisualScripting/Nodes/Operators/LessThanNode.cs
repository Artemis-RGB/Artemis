using System.Collections;
using Artemis.Core;

namespace Artemis.VisualScripting.Nodes.Operators;

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
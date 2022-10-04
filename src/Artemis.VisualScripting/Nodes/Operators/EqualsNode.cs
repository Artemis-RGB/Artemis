using Artemis.Core;

namespace Artemis.VisualScripting.Nodes.Operators;

[Node("Equals", "Checks if the two inputs are equals.", "Operators", InputType = typeof(bool), OutputType = typeof(bool))]
public class EqualsNode : Node
{
    #region Constructors

    public EqualsNode()
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
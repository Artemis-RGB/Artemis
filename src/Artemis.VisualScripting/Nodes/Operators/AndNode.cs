using Artemis.Core;

namespace Artemis.VisualScripting.Nodes.Operators;

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
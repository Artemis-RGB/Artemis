using Artemis.Core;

namespace Artemis.VisualScripting.Nodes.Operators;

[Node("Or", "Checks if any inputs are true.", "Operators", InputType = typeof(bool), OutputType = typeof(bool))]
public class OrNode : Node
{
    #region Constructors

    public OrNode()
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
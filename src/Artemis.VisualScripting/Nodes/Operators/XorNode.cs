using Artemis.Core;

namespace Artemis.VisualScripting.Nodes.Operators;

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
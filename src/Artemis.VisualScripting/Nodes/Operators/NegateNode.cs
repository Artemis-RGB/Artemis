using Artemis.Core;

namespace Artemis.VisualScripting.Nodes.Operators;

[Node("Negate", "Negates the boolean.", "Operators", InputType = typeof(bool), OutputType = typeof(bool))]
public class NegateNode : Node
{
    #region Constructors

    public NegateNode()
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
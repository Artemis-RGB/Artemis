using Artemis.Core;

namespace Artemis.VisualScripting.Nodes.Mathematics;

[Node("Round", "Outputs a rounded numeric value.", "Mathematics", InputType = typeof(Numeric), OutputType = typeof(Numeric))]
public class RoundNode : Node
{
    public RoundNode()
    {
        Input = CreateInputPin<Numeric>();
        Output = CreateOutputPin<Numeric>();
    }

    public OutputPin<Numeric> Output { get; set; }
    public InputPin<Numeric> Input { get; set; }

    #region Overrides of Node

    /// <inheritdoc />
    public override void Evaluate()
    {
        Output.Value = new Numeric(MathF.Round(Input.Value, MidpointRounding.AwayFromZero));
    }

    #endregion
}
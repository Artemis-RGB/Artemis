using Artemis.Core;

namespace Artemis.VisualScripting.Nodes.Timing;

[Node("Edge", "Outputs true on each edge when the input changes", "Timing", InputType = typeof(bool), OutputType = typeof(bool))]
public class EdgeNode : Node
{
    #region Properties & Fields

    private bool _lastInput;

    public InputPin<bool> Input { get; }
    public OutputPin<bool> Output { get; }

    #endregion

    #region Constructors

    public EdgeNode()
    {
        Input = CreateInputPin<bool>();
        Output = CreateOutputPin<bool>();
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public override void Evaluate()
    {
        bool input = Input.Value;

        Output.Value = input != _lastInput;

        _lastInput = input;
    }

    #endregion
}
using Artemis.Core;

namespace Artemis.VisualScripting.Nodes.Timing;

[Node("FlipFlop", "Inverts the output when the input changes from false to true", "Timing", InputType = typeof(bool), OutputType = typeof(bool))]
public class FlipFlopNode : Node
{
    #region Properties & Fields

    private bool _lastInput;
    private bool _currentValue;

    public InputPin<bool> Input { get; }
    public OutputPin<bool> Output { get; }

    #endregion

    #region Constructors

    public FlipFlopNode()
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
        if (input && !_lastInput)
        {
            _currentValue = !_currentValue;
            Output.Value = _currentValue;
        }

        _lastInput = input;
    }

    #endregion
}
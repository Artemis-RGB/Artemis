namespace Artemis.Core.Tests.Infrastructure.VisualScripting;

/// <summary>
///     A node that outputs whether its input is greater than a stored number, used to look at event arguments in event
///     conditions. The number is stored, so it survives saving and loading a script when the node is created through
///     <see cref="Plugins.Nodes.DataModelTestNodeProvider" />.
/// </summary>
[Node("Test greater than", "Outputs whether the input is greater than a number", "Tests", InputType = typeof(Numeric), OutputType = typeof(bool))]
public class TestGreaterThanNode : Node<float>
{
    public TestGreaterThanNode()
    {
        Input = CreateInputPin<Numeric>();
        Output = CreateOutputPin<bool>();
    }

    public InputPin<Numeric> Input { get; }
    public OutputPin<bool> Output { get; }

    /// <summary>
    ///     Gets or sets the number the input must be greater than
    /// </summary>
    public float Threshold
    {
        get => Storage;
        set => Storage = value;
    }

    /// <summary>
    ///     Gets the last input the node saw
    /// </summary>
    public float LastInput { get; private set; }

    public override void Evaluate()
    {
        LastInput = Input.Value;
        Output.Value = LastInput > Threshold;
    }
}

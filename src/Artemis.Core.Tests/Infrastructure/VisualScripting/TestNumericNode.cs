namespace Artemis.Core.Tests.Infrastructure.VisualScripting;

/// <summary>
///     A node that outputs a number the test controls. The value is stored, so it survives saving and loading a script
///     when the node is created through <see cref="Plugins.Nodes.TestNodeProvider" />.
/// </summary>
[Node("Test number", "Outputs a number controlled by the test", "Tests", OutputType = typeof(Numeric))]
public class TestNumericNode : Node<float>
{
    public TestNumericNode()
    {
        Output = CreateOutputPin<Numeric>();
    }

    public OutputPin<Numeric> Output { get; }

    /// <summary>
    ///     Gets or sets the value the node outputs
    /// </summary>
    public float Value
    {
        get => Storage;
        set => Storage = value;
    }

    /// <summary>
    ///     Gets the number of times the node was evaluated
    /// </summary>
    public int Evaluations { get; private set; }

    public override void Evaluate()
    {
        Evaluations++;
        Output.Value = new Numeric(Value);
    }
}

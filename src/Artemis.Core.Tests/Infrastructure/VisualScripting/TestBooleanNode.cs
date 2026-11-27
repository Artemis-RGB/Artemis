namespace Artemis.Core.Tests.Infrastructure.VisualScripting;

/// <summary>
///     A node that outputs a boolean the test controls. The value is stored, so it survives saving and loading a script
///     when the node is created through <see cref="Plugins.Nodes.TestNodeProvider" />.
/// </summary>
[Node("Test boolean", "Outputs a boolean controlled by the test", "Tests", OutputType = typeof(bool))]
public class TestBooleanNode : Node<bool>
{
    public TestBooleanNode()
    {
        Output = CreateOutputPin<bool>();
    }

    public OutputPin<bool> Output { get; }

    /// <summary>
    ///     Gets or sets the value the node outputs
    /// </summary>
    public bool Value
    {
        get => Storage;
        set => Storage = value;
    }

    public override void Evaluate()
    {
        Output.Value = Value;
    }
}

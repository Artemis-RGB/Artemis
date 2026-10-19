namespace Artemis.Core.Tests.Infrastructure.VisualScripting;

/// <summary>
///     A node that outputs text the test controls. The text is stored, so it survives saving and loading a script.
/// </summary>
[Node("Test text", "Outputs text controlled by the test")]
public class TestTextNode : Node<string>
{
    public TestTextNode()
    {
        Output = CreateOutputPin<string>();
    }

    public OutputPin<string> Output { get; }

    public string? Value
    {
        get => Storage;
        set => Storage = value;
    }

    public override void Evaluate()
    {
        Output.Value = Value;
    }
}

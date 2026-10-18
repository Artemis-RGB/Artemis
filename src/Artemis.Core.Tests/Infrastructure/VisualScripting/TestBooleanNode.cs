namespace Artemis.Core.Tests.Infrastructure.VisualScripting;

/// <summary>
///     A node that outputs a boolean the test controls
/// </summary>
public class TestBooleanNode : Node
{
    public TestBooleanNode() : base("Test boolean", "Outputs a boolean controlled by the test")
    {
        Output = CreateOutputPin<bool>();
    }

    public OutputPin<bool> Output { get; }

    /// <summary>
    ///     Gets or sets the value the node outputs
    /// </summary>
    public bool Value { get; set; }

    public override void Evaluate()
    {
        Output.Value = Value;
    }
}

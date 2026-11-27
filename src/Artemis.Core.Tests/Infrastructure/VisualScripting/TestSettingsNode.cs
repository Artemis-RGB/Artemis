namespace Artemis.Core.Tests.Infrastructure.VisualScripting;

/// <summary>
///     A node that stores an object rather than a single value, to see what survives saving and loading. It also sets
///     its own name and help URL, which the node data should leave alone.
/// </summary>
[Node("Test settings", "Stores settings", "Tests", "https://example.com/node-from-attribute")]
public class TestSettingsNode : Node<TestNodeSettings>
{
    public TestSettingsNode() : base("Own name", "")
    {
        HelpUrl = "https://example.com/own-help";
    }

    public override void Evaluate()
    {
    }
}

public class TestNodeSettings
{
    public string? Name { get; set; }
    public double Amount { get; set; }
    public List<int> Numbers { get; set; } = [];
}

namespace Artemis.Core.Tests.Infrastructure.VisualScripting;

/// <summary>
///     A node that outputs a number and can be told to fail while evaluating or initializing. What it does is stored, so
///     it survives saving and loading a script.
/// </summary>
[Node("Test throwing", "Fails when told to", "Tests", OutputType = typeof(Numeric))]
public class TestThrowingNode : Node<TestThrowingNode.FailureMode>
{
    public TestThrowingNode()
    {
        Output = CreateOutputPin<Numeric>();
    }

    public OutputPin<Numeric> Output { get; }

    /// <summary>
    ///     Gets or sets the number the node outputs while it isn't failing
    /// </summary>
    public float Value { get; set; }

    /// <summary>
    ///     Gets or sets when the node fails
    /// </summary>
    public FailureMode FailsOn
    {
        get => Storage;
        set => Storage = value;
    }

    public override void Initialize(INodeScript script)
    {
        if (FailsOn == FailureMode.Initialize)
            throw new InvalidOperationException("Test node failed to initialize");
    }

    public override void Evaluate()
    {
        if (FailsOn == FailureMode.Evaluate)
            throw new InvalidOperationException("Test node failed to evaluate");
        Output.Value = new Numeric(Value);
    }

    public enum FailureMode
    {
        Never,
        Evaluate,
        Initialize
    }
}

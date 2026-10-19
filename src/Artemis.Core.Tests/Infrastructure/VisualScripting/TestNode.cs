namespace Artemis.Core.Tests.Infrastructure.VisualScripting;

/// <summary>
///     A node without pins of its own that isn't registered anywhere. Tests add the pins they need and decide what
///     evaluating does. Because it isn't registered, it disappears when a script using it is loaded again.
/// </summary>
public class TestNode : Node
{
    public TestNode(string name = "Test node") : base(name, "")
    {
    }

    /// <summary>
    ///     Gets or sets what the node does when it is evaluated
    /// </summary>
    public Action<TestNode>? OnEvaluate { get; set; }

    /// <summary>
    ///     Gets the number of times the node was evaluated
    /// </summary>
    public int Evaluations { get; private set; }

    public override void Evaluate()
    {
        Evaluations++;
        OnEvaluate?.Invoke(this);
    }
}

/// <summary>
///     A default node, the kind of node a script is created with and that can't be deleted, such as the start node of
///     an event condition.
/// </summary>
public class TestDefaultNode : DefaultNode
{
    public static readonly Guid NodeId = new("8B3C35B2-6F6B-4C4A-9D4C-2A6A3F0F1E01");

    public TestDefaultNode() : base(NodeId, "Test default node", "A node every script starts with")
    {
        Output = CreateOutputPin<Numeric>();
    }

    public OutputPin<Numeric> Output { get; }

    public float Value { get; set; }

    public override void Evaluate()
    {
        Output.Value = new Numeric(Value);
    }
}

/// <summary>
///     A node that tracks whether it was disposed.
/// </summary>
public class TestDisposableNode : TestNode, IDisposable
{
    public bool IsDisposed { get; private set; }

    public void Dispose()
    {
        IsDisposed = true;
    }
}

namespace Artemis.Core.Tests.Infrastructure.VisualScripting;

/// <summary>
///     A node with a custom view model, the view model is created through the plugin's container.
/// </summary>
[Node("Test view model")]
public class TestViewModelNode : Node<int, TestNodeViewModel>
{
    public override void Evaluate()
    {
    }
}

public class TestNodeViewModel : ICustomNodeViewModel
{
    public TestNodeViewModel(TestViewModelNode node, NodeScript script)
    {
        Node = node;
        Script = script;
    }

    public INode Node { get; }
    public NodeScript Script { get; }

    public event EventHandler? NodeModified
    {
        add { }
        remove { }
    }
}

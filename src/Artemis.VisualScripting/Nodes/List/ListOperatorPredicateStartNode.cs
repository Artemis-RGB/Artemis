using Artemis.Core;

namespace Artemis.VisualScripting.Nodes.List;

public class ListOperatorPredicateStartNode : DefaultNode
{
    internal static readonly Guid NodeId = new("9A714CF3-8D02-4CC3-A1AC-73833F82D7C6");
    private readonly ObjectOutputPins _objectOutputPins;

    public ListOperatorPredicateStartNode() : base(NodeId, "List item", "Contains the current list item")
    {
        _objectOutputPins = new ObjectOutputPins(this);
    }

    public object? Item { get; set; }

    public override void Evaluate()
    {
        if (Item != null)
            _objectOutputPins.SetCurrentValue(Item);
    }

    public void ChangeType(Type? type)
    {
        _objectOutputPins.ChangeType(type);
    }
}
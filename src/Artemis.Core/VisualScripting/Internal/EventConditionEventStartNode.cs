using System;

namespace Artemis.Core.Internal;

internal class EventConditionEventStartNode : DefaultNode
{
    internal static readonly Guid NodeId = new("278735FE-69E9-4A73-A6B8-59E83EE19305");
    private readonly ObjectOutputPins _objectOutputPins;
    private IDataModelEvent? _dataModelEvent;

    public EventConditionEventStartNode() : base(NodeId, "Event Arguments", "Contains the event arguments that triggered the evaluation")
    {
        _objectOutputPins = new ObjectOutputPins(this);
    }

    public void SetDataModelEvent(IDataModelEvent? dataModelEvent)
    {
    }

    public void CreatePins(IDataModelEvent? dataModelEvent)
    {
        if (_dataModelEvent == dataModelEvent)
            return;

        _dataModelEvent = dataModelEvent;
        _objectOutputPins.ChangeType(dataModelEvent?.ArgumentsType);
    }

    public override void Evaluate()
    {
        if (_dataModelEvent?.LastEventArgumentsUntyped == null)
            return;

        _objectOutputPins.SetCurrentValue(_dataModelEvent.LastEventArgumentsUntyped);
    }
}
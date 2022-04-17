using System;
using Artemis.Core.VisualScripting.Internal;

namespace Artemis.Core.Internal;

internal class EventConditionValueChangedStartNode : DefaultNode, IEventConditionNode
{
    internal static readonly Guid NodeId = new("F9A270DB-A231-4800-BAB3-DC1F96856756");
    private object? _newValue;
    private object? _oldValue;

    public EventConditionValueChangedStartNode() : base(NodeId, "Changed values", "Contains the old and new values of the property chat was changed.")
    {
        NewValue = CreateOutputPin(typeof(object), "New value");
        OldValue = CreateOutputPin(typeof(object), "Old value");
    }

    public OutputPin NewValue { get; }
    public OutputPin OldValue { get; }

    public void UpdateOutputPins(DataModelPath dataModelPath)
    {
        Type? type = dataModelPath?.GetPropertyType();
        if (Numeric.IsTypeCompatible(type))
            type = typeof(Numeric);
        type ??= typeof(object);

        if (NewValue.Type != type)
            NewValue.ChangeType(type);
        if (OldValue.Type != type)
            OldValue.ChangeType(type);
    }

    public void UpdateValues(object? newValue, object? oldValue)
    {
        _newValue = newValue;
        _oldValue = oldValue;
    }

    /// <inheritdoc />
    public override void Evaluate()
    {
        NewValue.Value = _newValue;
        OldValue.Value = _oldValue;
    }
}
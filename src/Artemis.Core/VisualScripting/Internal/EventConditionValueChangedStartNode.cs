using System;

namespace Artemis.Core.Internal;

internal class EventConditionValueChangedStartNode : DefaultNode
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
        Type? type = dataModelPath.GetPropertyType();
        if (type == null)
            type = typeof(object);
        else if (Numeric.IsTypeCompatible(type))
            type = typeof(Numeric);

        if (NewValue.Type != type)
            NewValue.ChangeType(type);
        if (OldValue.Type != type)
            OldValue.ChangeType(type);
    }

    public void UpdateValues(object? newValue, object? oldValue)
    {
        _newValue = NewValue.IsNumeric ? new Numeric(newValue) : newValue;
        _oldValue = OldValue.IsNumeric ? new Numeric(oldValue) : oldValue;
    }

    /// <inheritdoc />
    public override void Evaluate()
    {
        NewValue.Value = _newValue;
        OldValue.Value = _oldValue;
    }
}
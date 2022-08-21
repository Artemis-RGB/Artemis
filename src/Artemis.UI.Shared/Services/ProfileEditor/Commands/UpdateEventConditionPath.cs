using System;
using Artemis.Core;
using Artemis.UI.Shared.Services.NodeEditor;

namespace Artemis.UI.Shared.Services.ProfileEditor.Commands;

/// <summary>
///     Represents a profile editor command that can be used to update an event condition's trigger mode.
/// </summary>
public class UpdateEventConditionPath : IProfileEditorCommand, IDisposable
{
    private readonly EventCondition _eventCondition;
    private readonly DataModelPath? _oldValue;
    private readonly NodeConnectionStore? _store;
    private readonly DataModelPath? _value;
    private bool _executed;

    /// <summary>
    ///     Creates a new instance of the <see cref="UpdateEventTriggerMode" /> class.
    /// </summary>
    public UpdateEventConditionPath(EventCondition eventCondition, DataModelPath? value)
    {
        _eventCondition = eventCondition;
        _value = value;
        _oldValue = eventCondition.EventPath;

        INode? startNode = eventCondition.GetStartNode();
        if (startNode != null)
            _store = new NodeConnectionStore(startNode);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_executed)
            _oldValue?.Dispose();
        else
            _value?.Dispose();
    }

    /// <inheritdoc />
    public string DisplayName => "Update event path";

    /// <inheritdoc />
    public void Execute()
    {
        // Store old connections
        _store?.Store();

        // Change the end node 
        _eventCondition.EventPath = _value;
        _eventCondition.UpdateEventNode();

        _executed = true;
    }

    /// <inheritdoc />
    public void Undo()
    {
        // Change the end node
        _eventCondition.EventPath = _oldValue;
        _eventCondition.UpdateEventNode();

        // Restore old connections
        _store?.Restore();

        _executed = false;
    }
}
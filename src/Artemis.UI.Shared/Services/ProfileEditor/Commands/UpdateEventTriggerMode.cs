using Artemis.Core;

namespace Artemis.UI.Shared.Services.ProfileEditor.Commands;

/// <summary>
///     Represents a profile editor command that can be used to update an event condition's trigger mode.
/// </summary>
public class UpdateEventTriggerMode : IProfileEditorCommand
{
    private readonly EventCondition _eventCondition;
    private readonly EventTriggerMode _oldValue;
    private readonly EventTriggerMode _value;

    /// <summary>
    ///     Creates a new instance of the <see cref="UpdateEventTriggerMode" /> class.
    /// </summary>
    public UpdateEventTriggerMode(EventCondition eventCondition, EventTriggerMode value)
    {
        _eventCondition = eventCondition;
        _value = value;
        _oldValue = eventCondition.TriggerMode;
    }

    /// <inheritdoc />
    public string DisplayName => "Update event trigger mode";

    /// <inheritdoc />
    public void Execute()
    {
        _eventCondition.TriggerMode = _value;
    }

    /// <inheritdoc />
    public void Undo()
    {
        _eventCondition.TriggerMode = _oldValue;
    }
}
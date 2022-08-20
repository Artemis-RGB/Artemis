using Artemis.Core;

namespace Artemis.UI.Shared.Services.ProfileEditor.Commands;

/// <summary>
///     Represents a profile editor command that can be used to update an event condition's overlap mode.
/// </summary>
public class UpdateEventOverlapMode : IProfileEditorCommand
{
    private readonly EventCondition _eventCondition;
    private readonly EventOverlapMode _value;
    private readonly EventOverlapMode _oldValue;

    /// <summary>
    ///     Creates a new instance of the <see cref="UpdateEventOverlapMode" /> class.
    /// </summary>
    public UpdateEventOverlapMode(EventCondition eventCondition, EventOverlapMode value)
    {
        _eventCondition = eventCondition;
        _value = value;
        _oldValue = eventCondition.OverlapMode;
    }

    /// <inheritdoc />
    public string DisplayName => "Update event overlap mode";

    /// <inheritdoc />
    public void Execute()
    {
        _eventCondition.OverlapMode = _value;
    }

    /// <inheritdoc />
    public void Undo()
    {
        _eventCondition.OverlapMode = _oldValue;
    }
}
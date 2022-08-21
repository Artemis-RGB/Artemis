using Artemis.Core;

namespace Artemis.UI.Shared.Services.ProfileEditor.Commands;

/// <summary>
///     Represents a profile editor command that can be used to update an event condition's overlap mode.
/// </summary>
public class UpdateEventToggleOffMode : IProfileEditorCommand
{
    private readonly EventCondition _eventCondition;
    private readonly EventToggleOffMode _oldValue;
    private readonly EventToggleOffMode _value;

    /// <summary>
    ///     Creates a new instance of the <see cref="UpdateEventOverlapMode" /> class.
    /// </summary>
    public UpdateEventToggleOffMode(EventCondition eventCondition, EventToggleOffMode value)
    {
        _eventCondition = eventCondition;
        _value = value;
        _oldValue = eventCondition.ToggleOffMode;
    }

    /// <inheritdoc />
    public string DisplayName => "Update event toggle off mode";

    /// <inheritdoc />
    public void Execute()
    {
        _eventCondition.ToggleOffMode = _value;
    }

    /// <inheritdoc />
    public void Undo()
    {
        _eventCondition.ToggleOffMode = _oldValue;
    }
}
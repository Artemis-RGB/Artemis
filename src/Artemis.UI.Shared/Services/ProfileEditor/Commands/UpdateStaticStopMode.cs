using Artemis.Core;

namespace Artemis.UI.Shared.Services.ProfileEditor.Commands;

/// <summary>
///     Represents a profile editor command that can be used to update an static condition's play mode.
/// </summary>
public class UpdateStaticStopMode : IProfileEditorCommand
{
    private readonly StaticStopMode _oldValue;
    private readonly StaticCondition _staticCondition;
    private readonly StaticStopMode _value;

    /// <summary>
    ///     Creates a new instance of the <see cref="UpdateEventTriggerMode" /> class.
    /// </summary>
    public UpdateStaticStopMode(StaticCondition staticCondition, StaticStopMode value)
    {
        _staticCondition = staticCondition;
        _value = value;
        _oldValue = staticCondition.StopMode;
    }

    /// <inheritdoc />
    public string DisplayName => "Update condition stop mode";

    /// <inheritdoc />
    public void Execute()
    {
        _staticCondition.StopMode = _value;
    }

    /// <inheritdoc />
    public void Undo()
    {
        _staticCondition.StopMode = _oldValue;
    }
}
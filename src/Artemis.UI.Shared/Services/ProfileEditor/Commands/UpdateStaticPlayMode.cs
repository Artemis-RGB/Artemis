using Artemis.Core;

namespace Artemis.UI.Shared.Services.ProfileEditor.Commands;

/// <summary>
///     Represents a profile editor command that can be used to update an static condition's play mode.
/// </summary>
public class UpdateStaticPlayMode : IProfileEditorCommand
{
    private readonly StaticPlayMode _oldValue;
    private readonly StaticCondition _staticCondition;
    private readonly StaticPlayMode _value;

    /// <summary>
    ///     Creates a new instance of the <see cref="UpdateEventTriggerMode" /> class.
    /// </summary>
    public UpdateStaticPlayMode(StaticCondition staticCondition, StaticPlayMode value)
    {
        _staticCondition = staticCondition;
        _value = value;
        _oldValue = staticCondition.PlayMode;
    }

    /// <inheritdoc />
    public string DisplayName => "Update condition play mode";

    /// <inheritdoc />
    public void Execute()
    {
        _staticCondition.PlayMode = _value;
    }

    /// <inheritdoc />
    public void Undo()
    {
        _staticCondition.PlayMode = _oldValue;
    }
}
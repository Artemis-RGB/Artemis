using System;
using Artemis.Core;

namespace Artemis.UI.Shared.Services.ProfileEditor.Commands;

/// <summary>
///     Represents a profile editor command that can be used to change the condition type of a profile element.
/// </summary>
public class ChangeConditionType : IProfileEditorCommand, IDisposable
{
    private readonly ICondition _condition;
    private readonly ICondition _oldCondition;
    private readonly RenderProfileElement _profileElement;
    private bool _executed;

    /// <summary>
    ///     Creates a new instance of the <see cref="ChangeConditionType" /> class.
    /// </summary>
    /// <param name="profileElement">The profile element whose condition type to change.</param>
    /// <param name="condition">The new condition type.</param>
    public ChangeConditionType(RenderProfileElement profileElement, ICondition condition)
    {
        _profileElement = profileElement;
        _condition = condition;
        _oldCondition = _profileElement.DisplayCondition;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_executed)
            _oldCondition?.Dispose();
        else
            _condition?.Dispose();
    }

    /// <inheritdoc />
    public string DisplayName => "Change element display condition type";

    /// <inheritdoc />
    public void Execute()
    {
        _profileElement.DisplayCondition = _condition;
        _executed = true;
    }

    /// <inheritdoc />
    public void Undo()
    {
        _profileElement.DisplayCondition = _oldCondition;
        _executed = false;
    }
}
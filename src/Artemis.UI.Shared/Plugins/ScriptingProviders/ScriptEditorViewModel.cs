using Artemis.Core.ScriptingProviders;

namespace Artemis.UI.Shared.ScriptingProviders;

/// <summary>
///     Represents a Stylet view model containing a script editor
/// </summary>
public class ScriptEditorViewModel : ActivatableViewModelBase, IScriptEditorViewModel
{
    private Script? _script;

    /// <summary>
    ///     Creates a new instance of <see cref="ScriptEditorViewModel" />
    /// </summary>
    /// <param name="scriptType">The script type this view model was created for</param>
    public ScriptEditorViewModel(ScriptType scriptType)
    {
        ScriptType = scriptType;
    }

    /// <summary>
    ///     Called just before the script is changed to a different one
    /// </summary>
    /// <param name="script">The script to display or <see langword="null" /> if no script is to be displayed</param>
    protected virtual void OnScriptChanging(Script? script)
    {
    }

    /// <summary>
    ///     Called after the script was changed to a different one
    /// </summary>
    /// <param name="script">The script to display or <see langword="null" /> if no script is to be displayed</param>
    protected virtual void OnScriptChanged(Script? script)
    {
    }

    /// <inheritdoc />
    public ScriptType ScriptType { get; }

    /// <inheritdoc />
    public Script? Script
    {
        get => _script;
        internal set => RaiseAndSetIfChanged(ref _script, value);
    }

    /// <inheritdoc />
    public void ChangeScript(Script? script)
    {
        OnScriptChanging(script);
        Script = script;
        OnScriptChanged(script);
    }
}
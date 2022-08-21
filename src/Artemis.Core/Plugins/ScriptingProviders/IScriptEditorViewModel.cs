namespace Artemis.Core.ScriptingProviders;

/// <summary>
///     Represents a view model containing a script editor
/// </summary>
public interface IScriptEditorViewModel
{
    /// <summary>
    ///     Gets the script type this view model was created for
    /// </summary>
    ScriptType ScriptType { get; }

    /// <summary>
    ///     Gets the script this editor is editing
    /// </summary>
    Script? Script { get; }

    /// <summary>
    ///     Called whenever the view model must display a different script
    /// </summary>
    /// <param name="script">The script to display or <see langword="null" /> if no script is to be displayed</param>
    void ChangeScript(Script? script);
}
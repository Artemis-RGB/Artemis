namespace Artemis.UI.Shared.Services.ProfileEditor;

/// <summary>
///     Represents a mode of focus in the editor.
/// </summary>
public enum ProfileEditorFocusMode
{
    /// <summary>
    ///     Disable focusing.
    /// </summary>
    None,

    /// <summary>
    ///     Focus the folder of the current element.
    /// </summary>
    Folder,

    /// <summary>
    ///     Focus the current element.
    /// </summary>
    Selection
}
namespace Artemis.Core.ScriptingProviders
{
    /// <summary>
    ///     Allows you to implement and register your own scripting provider
    /// </summary>
    public abstract class ScriptingProvider : PluginFeature
    {
        /// <summary>
        ///     Called when the UI needs a script editor for a <see cref="ProfileScript" />
        /// </summary>
        /// <param name="script">The script the editor must edit</param>
        public abstract IScriptEditorViewModel GetProfileScriptEditor(ProfileScript script);

        /// <summary>
        ///     Called when the UI needs a script editor for a <see cref="LayerScript" />
        /// </summary>
        /// <param name="script">The script the editor must edit</param>
        public abstract IScriptEditorViewModel GetLayerScriptScriptEditor(LayerScript script);

        /// <summary>
        ///     Called when the UI needs a script editor for a <see cref="PropertyScript" />
        /// </summary>
        /// <param name="script">The script the editor must edit</param>
        public abstract IScriptEditorViewModel GetPropertyScriptEditor(PropertyScript script);

        /// <summary>
        ///     Called when the UI needs a script editor for a <see cref="Script" />
        /// </summary>
        /// <param name="script">The script the editor must edit</param>
        public abstract IScriptEditorViewModel GetScriptEditor(Script script);
    }

    /// <summary>
    ///     Represents a view model containing a script editor
    /// </summary>
    public interface IScriptEditorViewModel
    {
        /// <summary>
        ///     Gets the script this editor is editing
        /// </summary>
        Script Script { get; }
    }
}
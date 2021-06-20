using Artemis.Core.ScriptingProviders;
using Stylet;

namespace Artemis.UI.Shared.ScriptingProviders
{
    /// <summary>
    ///     Represents a Stylet view model containing a script editor
    /// </summary>
    public class ScriptEditorViewModel : Screen, IScriptEditorViewModel
    {
        /// <inheritdoc />
        public ScriptEditorViewModel(Script script)
        {
            Script = script;
        }

        /// <inheritdoc />
        public Script Script { get; }
    }
}
using Artemis.Core.ScriptingProviders;
using Artemis.UI.Shared.ScriptingProviders;
using Stylet;

namespace Artemis.UI.Screens.Scripting
{
    public class ScriptConfigurationViewModel : Conductor<IScriptEditorViewModel>
    {
        public ScriptConfigurationViewModel(ScriptConfiguration scriptConfiguration)
        {
            ScriptConfiguration = scriptConfiguration;
            Script = ScriptConfiguration.Script;
        }

        public Script Script { get; set; }
        public ScriptConfiguration ScriptConfiguration { get; }

        #region Overrides of Screen

        /// <inheritdoc />
        protected override void OnActivate()
        {
            ActiveItem = Script switch
            {
                GlobalScript globalScript => Script.ScriptingProvider.CreateGlobalScriptEditor(globalScript),
                LayerScript layerScript => Script.ScriptingProvider.CreateLayerScriptScriptEditor(layerScript),
                ProfileScript profileScript => Script.ScriptingProvider.CreateProfileScriptEditor(profileScript),
                PropertyScript propertyScript => Script.ScriptingProvider.CreatePropertyScriptEditor(propertyScript),
                _ => new UnknownScriptEditorViewModel(null)
            };

            base.OnActivate();
        }

        /// <inheritdoc />
        protected override void OnClose()
        {
            ActiveItem = null;
            base.OnClose();
        }

        #endregion
    }

    public class UnknownScriptEditorViewModel : ScriptEditorViewModel
    {
        /// <inheritdoc />
        public UnknownScriptEditorViewModel(Script script) : base(script)
        {
        }
    }
}
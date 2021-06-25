using System.Collections.Generic;
using System.Threading.Tasks;
using Artemis.Core.ScriptingProviders;
using Artemis.Core.Services;
using Artemis.UI.Screens.Scripting.Dialogs;
using Artemis.UI.Shared.ScriptingProviders;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.Scripting
{
    public class ScriptConfigurationViewModel : Conductor<IScriptEditorViewModel>
    {
        private readonly IScriptingService _scriptingService;
        private readonly IDialogService _dialogService;

        public ScriptConfigurationViewModel(ScriptConfiguration scriptConfiguration, IScriptingService scriptingService, IDialogService dialogService)
        {
            _scriptingService = scriptingService;
            _dialogService = dialogService;
            ScriptConfiguration = scriptConfiguration;
            Script = ScriptConfiguration.Script;

            ActiveItem = Script switch
            {
                GlobalScript globalScript => Script.ScriptingProvider.CreateGlobalScriptEditor(globalScript),
                LayerScript layerScript => Script.ScriptingProvider.CreateLayerScriptScriptEditor(layerScript),
                ProfileScript profileScript => Script.ScriptingProvider.CreateProfileScriptEditor(profileScript),
                PropertyScript propertyScript => Script.ScriptingProvider.CreatePropertyScriptEditor(propertyScript),
                _ => new UnknownScriptEditorViewModel(null)
            };
        }

        public Script Script { get; set; }
        public ScriptConfiguration ScriptConfiguration { get; }

        public async Task ViewProperties()
        {
            object result = await _dialogService.ShowDialog<ScriptConfigurationEditViewModel>(new Dictionary<string, object>
            {
                {"scriptConfiguration", ScriptConfiguration}
            });

            if (result is nameof(ScriptConfigurationEditViewModel.Delete))
                await Delete();
        }

        private async Task Delete()
        {
            bool result = await _dialogService.ShowConfirmDialogAt("ScriptsDialog", "Delete script", $"Are you sure you want to delete '{ScriptConfiguration.Name}'?");
            if (!result)
                return;

            switch (Script)
            {
                case GlobalScript globalScript:
                    _scriptingService.DeleteScript(ScriptConfiguration);
                    break;
                case LayerScript layerScript:
                    layerScript.Layer.ScriptConfigurations.Remove(ScriptConfiguration);
                    break;
                case ProfileScript profileScript:
                    profileScript.Profile.ScriptConfigurations.Remove(ScriptConfiguration);
                    break;
                case PropertyScript propertyScript:
                    propertyScript.LayerProperty.ScriptConfigurations.Remove(ScriptConfiguration);
                    break;
            }

            ScriptConfiguration.DiscardPendingChanges();
            ScriptConfiguration.Script?.Dispose();
            RequestClose();
        }
    }

    public class UnknownScriptEditorViewModel : ScriptEditorViewModel
    {
        /// <inheritdoc />
        public UnknownScriptEditorViewModel(Script script) : base(script)
        {
        }
    }
}
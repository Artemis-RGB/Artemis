using System;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.ScriptingProviders;
using Artemis.Core.Services;
using Artemis.UI.Screens.Scripting.Dialogs;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.Scripting
{
    public class ScriptsDialogViewModel : Conductor<ScriptConfigurationViewModel>.Collection.OneActive
    {
        private readonly IScriptingService _scriptingService;
        private readonly IDialogService _dialogService;
        public Profile Profile { get; }
        public Layer Layer { get; }
        public ILayerProperty LayerProperty { get; }

        public ScriptsDialogViewModel(Profile profile, IScriptingService scriptingService, IDialogService dialogService)
        {
            _scriptingService = scriptingService;
            _dialogService = dialogService;

            DisplayName = "Artemins | Profile Scripts";
            Profile = profile ?? throw new ArgumentNullException(nameof(profile));

            Items.AddRange(Profile.ScriptConfigurations.Select(s => new ScriptConfigurationViewModel(s)));
        }

        public ScriptsDialogViewModel(Layer layer, IDialogService dialogService)
        {
            _dialogService = dialogService;
            DisplayName = "Artemins | Layer Scripts";
            Layer = layer ?? throw new ArgumentNullException(nameof(layer));

            Items.AddRange(Layer.ScriptConfigurations.Select(s => new ScriptConfigurationViewModel(s)));
        }

        public ScriptsDialogViewModel(ILayerProperty layerProperty, IDialogService dialogService)
        {
            _dialogService = dialogService;
            DisplayName = "Artemins | Layer Property Scripts";
            LayerProperty = layerProperty ?? throw new ArgumentNullException(nameof(layerProperty));

            Items.AddRange(LayerProperty.ScriptConfigurations.Select(s => new ScriptConfigurationViewModel(s)));
        }

        public async Task AddScriptConfiguration()
        {
            object result = await _dialogService.ShowDialogAt<ScriptConfigurationCreateViewModel>("ScriptsDialog");
            if (result is not ScriptConfiguration scriptConfiguration)
                return;

            if (Profile != null)
            {
                Profile.ScriptConfigurations.Add(scriptConfiguration);
                _scriptingService.CreateScriptInstance(Profile, scriptConfiguration);
            }
            else if (Layer != null)
            {
                Layer.ScriptConfigurations.Add(scriptConfiguration);
                _scriptingService.CreateScriptInstance(Layer, scriptConfiguration);
            }
            else if (LayerProperty != null)
            {
                LayerProperty.ScriptConfigurations.Add(scriptConfiguration);
                _scriptingService.CreateScriptInstance(LayerProperty, scriptConfiguration);
            }

            Items.Add(new ScriptConfigurationViewModel(scriptConfiguration));
        }
    }
}
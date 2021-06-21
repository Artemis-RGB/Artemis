using System;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.ScriptingProviders;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.Scripting.Dialogs;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.Scripting
{
    public class ScriptsDialogViewModel : Conductor<ScriptConfigurationViewModel>.Collection.OneActive
    {
        private readonly IScriptingService _scriptingService;
        private readonly IDialogService _dialogService;
        private readonly IScriptVmFactory _scriptVmFactory;
        public Profile Profile { get; }
        public Layer Layer { get; }
        public ILayerProperty LayerProperty { get; }

        public ScriptsDialogViewModel(Profile profile, IScriptingService scriptingService, IDialogService dialogService, IScriptVmFactory scriptVmFactory)
        {
            _scriptingService = scriptingService;
            _dialogService = dialogService;
            _scriptVmFactory = scriptVmFactory;

            DisplayName = "Artemis | Profile Scripts";
            Profile = profile ?? throw new ArgumentNullException(nameof(profile));

            Items.AddRange(Profile.ScriptConfigurations.Select(scriptVmFactory.ScriptConfigurationViewModel));
        }

        public ScriptsDialogViewModel(Layer layer, IDialogService dialogService)
        {
            _dialogService = dialogService;
            DisplayName = "Artemins | Layer Scripts";
            Layer = layer ?? throw new ArgumentNullException(nameof(layer));

            Items.AddRange(Layer.ScriptConfigurations.Select(_scriptVmFactory.ScriptConfigurationViewModel));
        }

        public ScriptsDialogViewModel(ILayerProperty layerProperty, IDialogService dialogService)
        {
            _dialogService = dialogService;
            DisplayName = "Artemins | Layer Property Scripts";
            LayerProperty = layerProperty ?? throw new ArgumentNullException(nameof(layerProperty));

            Items.AddRange(Layer.ScriptConfigurations.Select(_scriptVmFactory.ScriptConfigurationViewModel));
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

            Items.Add(_scriptVmFactory.ScriptConfigurationViewModel(scriptConfiguration));
        }
    }
}
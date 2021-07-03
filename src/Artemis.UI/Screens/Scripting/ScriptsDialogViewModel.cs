using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
    public class ScriptsDialogViewModel : Conductor<IScriptEditorViewModel>
    {
        private readonly IScriptingService _scriptingService;
        private readonly IDialogService _dialogService;
        private readonly IProfileService _profileService;
        private readonly IProfileEditorService _profileEditorService;
        private readonly IScriptVmFactory _scriptVmFactory;
        private readonly Dictionary<ScriptingProvider, IScriptEditorViewModel> _providerViewModels = new();
        private ScriptConfigurationViewModel _selectedScript;

        public ScriptsDialogViewModel(Profile profile,
            IScriptingService scriptingService,
            IDialogService dialogService,
            IProfileService profileService,
            IProfileEditorService profileEditorService,
            IScriptVmFactory scriptVmFactory)
        {
            _scriptingService = scriptingService;
            _dialogService = dialogService;
            _profileService = profileService;
            _profileEditorService = profileEditorService;
            _scriptVmFactory = scriptVmFactory;

            DisplayName = "Artemis | Profile Scripts";
            ScriptType = ScriptType.Profile;
            Profile = profile ?? throw new ArgumentNullException(nameof(profile));

            ScriptConfigurations.AddRange(Profile.ScriptConfigurations.Select(scriptVmFactory.ScriptConfigurationViewModel));
            ScriptConfigurations.CollectionChanged += ItemsOnCollectionChanged;
        }

        public ScriptType ScriptType { get; }
        public Profile Profile { get; } 
        public BindableCollection<ScriptConfigurationViewModel> ScriptConfigurations { get; } = new();
        public bool HasScripts => ScriptConfigurations.Any();

        public ScriptConfigurationViewModel SelectedScript
        {
            get => _selectedScript;
            set
            {
                if (!SetAndNotify(ref _selectedScript, value)) return;
                SetupScriptEditor(_selectedScript?.ScriptConfiguration);
            }
        }

        private void SetupScriptEditor(ScriptConfiguration scriptConfiguration)
        {
            if (scriptConfiguration == null)
            {
                ActiveItem = null;
                return;
            }

            // The script is null if the provider is missing
            if (scriptConfiguration.Script == null)
            {
                ActiveItem = null;
                return;
            }

            if (!_providerViewModels.TryGetValue(scriptConfiguration.Script.ScriptingProvider, out IScriptEditorViewModel viewModel))
            {
                viewModel = scriptConfiguration.Script.ScriptingProvider.CreateScriptEditor(ScriptType);
                _providerViewModels.Add(scriptConfiguration.Script.ScriptingProvider, viewModel);
            }

            ActiveItem = viewModel;
            ActiveItem.ChangeScript(scriptConfiguration.Script);
        }
        
        private void ItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            NotifyOfPropertyChange(nameof(HasScripts));
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

            ScriptConfigurationViewModel viewModel = _scriptVmFactory.ScriptConfigurationViewModel(scriptConfiguration);
            ScriptConfigurations.Add(viewModel);
            SelectedScript = viewModel;
        }

        public async Task ViewProperties(ScriptConfigurationViewModel scriptConfigurationViewModel)
        {
            object result = await _dialogService.ShowDialogAt<ScriptConfigurationEditViewModel>(
                "ScriptsDialog",
                new Dictionary<string, object> {{"scriptConfiguration", scriptConfigurationViewModel.ScriptConfiguration}}
            );

            if (result is nameof(ScriptConfigurationEditViewModel.Delete))
                await Delete(scriptConfigurationViewModel);
        }

        private async Task Delete(ScriptConfigurationViewModel scriptConfigurationViewModel)
        {
            bool result = await _dialogService.ShowConfirmDialogAt(
                "ScriptsDialog",
                "Delete script",
                $"Are you sure you want to delete '{scriptConfigurationViewModel.ScriptConfiguration.Name}'?"
            );
            if (!result)
                return;

            switch (scriptConfigurationViewModel.Script)
            {
                case GlobalScript:
                    _scriptingService.DeleteScript(scriptConfigurationViewModel.ScriptConfiguration);
                    break;
                case ProfileScript profileScript:
                    profileScript.Profile.ScriptConfigurations.Remove(scriptConfigurationViewModel.ScriptConfiguration);
                    break;
            }

            scriptConfigurationViewModel.ScriptConfiguration.DiscardPendingChanges();
            scriptConfigurationViewModel.ScriptConfiguration.Script?.Dispose();

            SelectedScript = null;
            ScriptConfigurations.Remove(scriptConfigurationViewModel);
        }

        #region Overrides of Screen

        /// <inheritdoc />
        protected override void OnInitialActivate()
        {
            SelectedScript = ScriptConfigurations.FirstOrDefault();
            base.OnInitialActivate();
        }

        /// <inheritdoc />
        protected override void OnClose()
        {
            if (Profile != null)
            {
                if (_profileEditorService.SelectedProfile == Profile)
                    _profileEditorService.SaveSelectedProfileConfiguration();
                else
                    _profileService.SaveProfile(Profile, false);
            }

            ScriptConfigurations.CollectionChanged -= ItemsOnCollectionChanged;
            base.OnClose();
        }

        #endregion
    }
}
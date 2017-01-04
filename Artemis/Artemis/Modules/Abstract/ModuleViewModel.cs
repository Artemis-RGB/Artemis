using Artemis.Events;
using Artemis.Managers;
using Artemis.Services;
using Artemis.ViewModels.Profiles;
using Caliburn.Micro;
using Ninject;
using Ninject.Extensions.Logging;
using Ninject.Parameters;

namespace Artemis.Modules.Abstract
{
    public abstract class ModuleViewModel : Screen
    {
        private readonly ModuleManager _moduleManager;
        private readonly MainManager _mainManager;
        private ModuleSettings _settings;

        public ModuleViewModel(MainManager mainManager, ModuleModel moduleModel, IKernel kernel)
        {
            _mainManager = mainManager;
            _moduleManager = mainManager.ModuleManager;
            ModuleModel = moduleModel;

            Settings = moduleModel.Settings;
            IParameter[] args =
            {
                new ConstructorArgument("mainManager", _mainManager),
                new ConstructorArgument("moduleModel", ModuleModel),
                new ConstructorArgument("lastProfile", Settings.LastProfile)
            };
            ProfileEditor = kernel.Get<ProfileEditorViewModel>(args);

            _mainManager.EnabledChanged += MainManagerOnEnabledChanged;
            _moduleManager.EffectChanged += ModuleManagerOnModuleChanged;
        }

        public ProfileEditorViewModel ProfileEditor { get; set; }
        public ModuleModel ModuleModel { get; }

        [Inject]
        public ILogger Logger { get; set; }

        [Inject]
        public MetroDialogService DialogService { get; set; }

        public ModuleSettings Settings
        {
            get { return _settings; }
            set
            {
                if (Equals(value, _settings)) return;
                _settings = value;
                NotifyOfPropertyChange(() => Settings);
            }
        }

        public virtual bool IsModuleActive => _moduleManager.ActiveModule == ModuleModel;
        public abstract bool UsesProfileEditor { get; }

        private void MainManagerOnEnabledChanged(object sender, EnabledChangedEventArgs e)
        {
            NotifyOfPropertyChange(() => IsModuleActive);
            UpdateIsEnabled();
        }

        private void ModuleManagerOnModuleChanged(object sender, ModuleChangedEventArgs e)
        {
            NotifyOfPropertyChange(() => IsModuleActive);
            UpdateIsEnabled();
        }

        private void UpdateIsEnabled()
        {
            if (ModuleModel.IsBoundToProcess || ModuleModel.IsOverlay)
                return;

            Settings.IsEnabled = IsModuleActive;
            Settings.Save();
            NotifyOfPropertyChange(() => Settings);
        }

        public virtual void ToggleModule()
        {
            Settings.IsEnabled = !Settings.IsEnabled;
            Settings.Save();
            NotifyOfPropertyChange(() => Settings);

            // On process-bound modules, only set the module model
            if (ModuleModel.IsBoundToProcess || ModuleModel.IsOverlay)
                return;

            // On other modules, activate them if necessary
            if (IsModuleActive && !Settings.IsEnabled)
                _moduleManager.ClearActiveModule();
            else if (!IsModuleActive && Settings.IsEnabled)
                _moduleManager.ChangeActiveModule(ModuleModel, _mainManager.LoopManager);
        }

        public virtual void SaveSettings()
        {
            Settings?.Save();
            if (UsesProfileEditor)
                ProfileEditor.SaveSelectedProfile();
            if (!IsModuleActive)
                return;

            // Restart the module if it's currently running to apply settings.
            _moduleManager.ChangeActiveModule(ModuleModel);
        }

        public virtual async void ResetSettings()
        {
            var resetConfirm = await DialogService.ShowQuestionMessageBox("Reset module settings",
                "Are you sure you wish to reset this module's settings? \nAny changes you made will be lost.");

            if (!resetConfirm.Value)
                return;

            Settings.Reset(true);
            NotifyOfPropertyChange(() => Settings);

            SaveSettings();
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            ProfileEditor.Activate();
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
            ProfileEditor.Deactivate();
        }
    }
}
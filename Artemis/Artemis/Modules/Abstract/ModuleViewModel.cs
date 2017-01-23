using Artemis.DAL;
using Artemis.Events;
using Artemis.Managers;
using Artemis.Services;
using Artemis.Settings;
using Artemis.ViewModels;
using Caliburn.Micro;
using Ninject;
using Ninject.Extensions.Logging;
using Ninject.Parameters;

namespace Artemis.Modules.Abstract
{
    public abstract class ModuleViewModel : Screen
    {
        private readonly MainManager _mainManager;
        private readonly ModuleManager _moduleManager;
        private readonly GeneralSettings _generalSettings;
        private ModuleSettings _settings;

        public ModuleViewModel(MainManager mainManager, ModuleModel moduleModel, IKernel kernel)
        {
            _mainManager = mainManager;
            _moduleManager = mainManager.ModuleManager;
            _generalSettings = SettingsProvider.Load<GeneralSettings>();
            ModuleModel = moduleModel;
            Settings = moduleModel.Settings;

            _mainManager.EnabledChanged += MainManagerOnEnabledChanged;
            _moduleManager.EffectChanged += ModuleManagerOnModuleChanged;

            // ReSharper disable once VirtualMemberCallInConstructor
            if (!UsesProfileEditor)
                return;

            IParameter[] args =
            {
                new ConstructorArgument("mainManager", _mainManager),
                new ConstructorArgument("moduleModel", ModuleModel),
                new ConstructorArgument("lastProfile", Settings.LastProfile)
            };
            ProfileEditor = kernel.Get<ProfileEditorViewModel>(args);
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

        public virtual bool IsModuleEnabled
        {
            get
            {
                if (!ModuleModel.IsGeneral)
                    return Settings.IsEnabled;
                return _generalSettings.LastModule == ModuleModel.Name;
            }
        }

        public abstract bool UsesProfileEditor { get; }

        private void MainManagerOnEnabledChanged(object sender, EnabledChangedEventArgs e)
        {
            UpdatedEnabledSetting();
            NotifyOfPropertyChange(() => IsModuleActive);
        }

        private void ModuleManagerOnModuleChanged(object sender, ModuleChangedEventArgs e)
        {
            UpdatedEnabledSetting();
            NotifyOfPropertyChange(() => IsModuleActive);
            NotifyOfPropertyChange(() => IsModuleEnabled);
        }

        private void UpdatedEnabledSetting()
        {
            if (!ModuleModel.IsGeneral || !_moduleManager.ActiveModule.IsGeneral || Settings.IsEnabled == IsModuleActive)
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
            if (!ModuleModel.IsGeneral)
            {
                NotifyOfPropertyChange(() => IsModuleActive);
                NotifyOfPropertyChange(() => IsModuleEnabled);
                return;
            }

            // On other modules, activate them if necessary
            if (IsModuleActive && !Settings.IsEnabled)
                _moduleManager.ClearActiveModule();
            else if (!IsModuleActive && Settings.IsEnabled)
                _moduleManager.ChangeActiveModule(ModuleModel, _mainManager.LoopManager);

            NotifyOfPropertyChange(() => IsModuleActive);
            NotifyOfPropertyChange(() => IsModuleEnabled);
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
            ProfileEditor?.OnActivate();
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
            ProfileEditor?.OnDeactivate(close);
        }
    }
}
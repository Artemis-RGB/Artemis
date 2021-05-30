using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.ProfileEditor.Conditions;
using Artemis.UI.Shared.Services;
using FluentValidation;
using MaterialDesignThemes.Wpf;
using Stylet;

namespace Artemis.UI.Screens.Sidebar.Dialogs
{
    public class ProfileEditViewModel : DialogViewModelBase
    {
        private readonly DataModelConditionGroup _dataModelConditionGroup;
        private readonly ProfileConfiguration _profileConfiguration;
        private readonly IProfileService _profileService;
        private readonly IDialogService _dialogService;
        private bool _initializing;
        private string _profileName;
        private ProfileIconViewModel _selectedIcon;
        private ProfileModuleViewModel _selectedModule;
        private readonly List<Module> _modules;

        public ProfileEditViewModel(ProfileConfiguration profileConfiguration,
            IProfileService profileService,
            IPluginManagementService pluginManagementService,
            IDialogService dialogService,
            IDataModelConditionsVmFactory dataModelConditionsVmFactory,
            IModelValidator<ProfileEditViewModel> validator) : base(validator)
        {
            _profileConfiguration = profileConfiguration;
            _profileService = profileService;
            _dialogService = dialogService;
            _dataModelConditionGroup = _profileConfiguration.ActivationCondition ?? new DataModelConditionGroup(null);
            _modules = _profileConfiguration.Module != null ? new List<Module> {_profileConfiguration.Module} : new List<Module>();

            Icons = new BindableCollection<ProfileIconViewModel>();
            Modules = new BindableCollection<ProfileModuleViewModel>(
                pluginManagementService.GetFeaturesOfType<Module>().Where(m => !m.IsAlwaysAvailable).Select(m => new ProfileModuleViewModel(m))
            );
            Initializing = true;
            ActivationConditionViewModel = dataModelConditionsVmFactory.DataModelConditionGroupViewModel(_dataModelConditionGroup, ConditionGroupType.General, _modules);
            ActivationConditionViewModel.ConductWith(this);
            ActivationConditionViewModel.IsRootGroup = true;

            _profileName = _profileConfiguration.Name;
            _selectedModule = Modules.FirstOrDefault(m => m.Module == _profileConfiguration.Module);

            Task.Run(() =>
            {
                Icons.AddRange(Enum.GetValues<PackIconKind>()
                    .GroupBy(e => e)
                    .Select(g => g.First())
                    .Select(e => new ProfileIconViewModel(e))
                    .ToList());
                SelectedIcon = Icons.FirstOrDefault(i => i.Icon.ToString() == _profileConfiguration.Icon);
                Initializing = false;
            });
        }

        public BindableCollection<ProfileIconViewModel> Icons { get; }
        public BindableCollection<ProfileModuleViewModel> Modules { get; }

        public bool Initializing
        {
            get => _initializing;
            set => SetAndNotify(ref _initializing, value);
        }

        public string ProfileName
        {
            get => _profileName;
            set => SetAndNotify(ref _profileName, value);
        }

        public ProfileIconViewModel SelectedIcon
        {
            get => _selectedIcon;
            set => SetAndNotify(ref _selectedIcon, value);
        }

        public ProfileModuleViewModel SelectedModule
        {
            get => _selectedModule;
            set
            {
                if (!SetAndNotify(ref _selectedModule, value)) return;
                _modules.Clear();
                if (value != null)
                    _modules.Add(value.Module);
                ActivationConditionViewModel.UpdateModules();
            }
        }

        public DataModelConditionGroupViewModel ActivationConditionViewModel { get; }

        public async Task Delete()
        {
            Session.Close(false);
            if (await _dialogService.ShowConfirmDialog("Delete profile", "Are you sure you want to delete this profile?\r\nThis cannot be undone."))
                _profileService.RemoveProfileConfiguration(_profileConfiguration);
        }

        public async Task Accept()
        {
            await ValidateAsync();

            if (HasErrors)
                return;

            _profileConfiguration.Name = ProfileName;
            _profileConfiguration.Icon = SelectedIcon.Icon.ToString();
            _profileConfiguration.Module = SelectedModule?.Module;

            if (_dataModelConditionGroup.Children.Any())
                _profileConfiguration.ActivationCondition = _dataModelConditionGroup;

            _profileService.SaveProfileCategory(_profileConfiguration.Category);

            Session.Close(true);
        }
    }

    public class ProfileEditViewModelValidator : AbstractValidator<ProfileEditViewModel>
    {
        public ProfileEditViewModelValidator()
        {
            RuleFor(m => m.ProfileName).NotEmpty().WithMessage("Profile name may not be empty");
        }
    }
}
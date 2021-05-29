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
    public class ProfileCreateViewModel : DialogViewModelBase
    {
        private readonly DataModelConditionGroup _dataModelConditionGroup;
        private readonly ProfileCategory _profileCategory;
        private readonly IProfileService _profileService;
        private bool _initializing;
        private string _profileName;
        private ProfileIconViewModel _selectedIcon;
        private ProfileModuleViewModel _selectedModule;
        private readonly List<Module> _modules;

        public ProfileCreateViewModel(ProfileCategory profileCategory,
            IProfileService profileService,
            IPluginManagementService pluginManagementService,
            IDataModelConditionsVmFactory dataModelConditionsVmFactory,
            IModelValidator<ProfileCreateViewModel> validator) : base(validator)
        {
            _profileCategory = profileCategory;
            _profileService = profileService;
            _dataModelConditionGroup = new DataModelConditionGroup(null);
            _modules = new List<Module>();

            Icons = new BindableCollection<ProfileIconViewModel>();
            Modules = new BindableCollection<ProfileModuleViewModel>(
                pluginManagementService.GetFeaturesOfType<Module>().Where(m => !m.IsAlwaysAvailable).Select(m => new ProfileModuleViewModel(m))
            );
            Initializing = true;
            ActivationConditionViewModel = dataModelConditionsVmFactory.DataModelConditionGroupViewModel(_dataModelConditionGroup, ConditionGroupType.General, _modules);
            ActivationConditionViewModel.IsRootGroup = true;
            ActivationConditionViewModel.ConductWith(this);

            Task.Run(() =>
            {
                Icons.AddRange(Enum.GetValues<PackIconKind>()
                    .GroupBy(e => e)
                    .Select(g => g.First())
                    .Select(e => new ProfileIconViewModel(e))
                    .ToList());
                SelectedIcon = Icons.FirstOrDefault();
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

        public async Task Accept()
        {
            await ValidateAsync();

            if (HasErrors)
                return;

            ProfileConfiguration profileConfiguration = _profileService.CreateProfileConfiguration(_profileCategory, ProfileName, SelectedIcon.Icon.ToString());
            profileConfiguration.Module = SelectedModule?.Module;
            if (_dataModelConditionGroup.Children.Any())
                profileConfiguration.ActivationCondition = _dataModelConditionGroup;

            _profileService.SaveProfileCategory(_profileCategory);

            Session.Close();
        }
    }

    public class ProfileCreateViewModelValidator : AbstractValidator<ProfileCreateViewModel>
    {
        public ProfileCreateViewModelValidator()
        {
            RuleFor(m => m.ProfileName).NotEmpty().WithMessage("Profile name may not be empty");
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.ProfileEditor.Conditions;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using FluentValidation;
using MaterialDesignThemes.Wpf;
using Ookii.Dialogs.Wpf;
using Stylet;

namespace Artemis.UI.Screens.Sidebar.Dialogs.ProfileEdit
{
    public class ProfileEditViewModel : DialogViewModelBase
    {
        private readonly DataModelConditionGroup _dataModelConditionGroup;
        private readonly IProfileService _profileService;
        private readonly IDialogService _dialogService;
        private bool _initializing;
        private string _profileName;
        private ProfileIconViewModel _selectedIcon;
        private ProfileModuleViewModel _selectedModule;
        private readonly List<Module> _modules;
        private ProfileConfigurationIconType _selectedIconType;
        private Stream _selectedImage;
        private BitmapImage _selectedBitmap;

        public ProfileEditViewModel(ProfileConfiguration profileConfiguration,
            bool isNew,
            IProfileService profileService,
            IPluginManagementService pluginManagementService,
            IDialogService dialogService,
            ISidebarVmFactory sidebarVmFactory,
            IDataModelConditionsVmFactory dataModelConditionsVmFactory,
            IModelValidator<ProfileEditViewModel> validator) : base(validator)
        {
            ProfileConfiguration = profileConfiguration;
            IsNew = isNew;

            _profileService = profileService;
            _dialogService = dialogService;
            _dataModelConditionGroup = ProfileConfiguration.ActivationCondition ?? new DataModelConditionGroup(null);
            _modules = ProfileConfiguration.Module != null ? new List<Module> {ProfileConfiguration.Module} : new List<Module>();

            IconTypes = new BindableCollection<ValueDescription>(EnumUtilities.GetAllValuesAndDescriptions(typeof(ProfileConfigurationIconType)));
            Icons = new BindableCollection<ProfileIconViewModel>();
            Modules = new BindableCollection<ProfileModuleViewModel>(
                pluginManagementService.GetFeaturesOfType<Module>().Where(m => !m.IsAlwaysAvailable).Select(m => new ProfileModuleViewModel(m))
            );
            Initializing = true;
            ActivationConditionViewModel = dataModelConditionsVmFactory.DataModelConditionGroupViewModel(_dataModelConditionGroup, ConditionGroupType.General, _modules);
            ActivationConditionViewModel.ConductWith(this);
            ActivationConditionViewModel.IsRootGroup = true;
            ModuleActivationRequirementsViewModel = new ModuleActivationRequirementsViewModel(sidebarVmFactory);
            ModuleActivationRequirementsViewModel.ConductWith(this);
            ModuleActivationRequirementsViewModel.SetModule(ProfileConfiguration.Module);

            _profileName = ProfileConfiguration.Name;
            _selectedIconType = ProfileConfiguration.Icon.IconType;
            _selectedModule = Modules.FirstOrDefault(m => m.Module == ProfileConfiguration.Module);

            Task.Run(() =>
            {
                Icons.AddRange(Enum.GetValues<PackIconKind>()
                    .GroupBy(e => e)
                    .Select(g => g.First())
                    .Select(e => new ProfileIconViewModel(e))
                    .ToList());
                SelectedIcon = Icons.FirstOrDefault(i => i.Icon.ToString() == ProfileConfiguration.Icon.MaterialIcon);
                Initializing = false;
            });
        }

        public ProfileConfiguration ProfileConfiguration { get; }
        public bool IsNew { get; }
        public BindableCollection<ValueDescription> IconTypes { get; }
        public BindableCollection<ProfileIconViewModel> Icons { get; }
        public BindableCollection<ProfileModuleViewModel> Modules { get; }
        public bool HasUsableModules => Modules.Any();

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

        public ProfileConfigurationIconType SelectedIconType
        {
            get => _selectedIconType;
            set => SetAndNotify(ref _selectedIconType, value);
        }

        public Stream SelectedImage
        {
            get => _selectedImage;
            set => SetAndNotify(ref _selectedImage, value);
        }

        public BitmapImage SelectedBitmap
        {
            get => _selectedBitmap;
            set => SetAndNotify(ref _selectedBitmap, value);
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
                ModuleActivationRequirementsViewModel.SetModule(value?.Module);
            }
        }

        public DataModelConditionGroupViewModel ActivationConditionViewModel { get; }
        public ModuleActivationRequirementsViewModel ModuleActivationRequirementsViewModel { get; }

        public void Delete()
        {
            Session.Close(nameof(Delete));
        }

        public async Task Accept()
        {
            await ValidateAsync();

            if (HasErrors)
                return;

            ProfileConfiguration.Name = ProfileName;
            ProfileConfiguration.Icon.IconType = SelectedIconType;
            ProfileConfiguration.Icon.MaterialIcon = SelectedIcon?.Icon.ToString();
            ProfileConfiguration.Icon.FileIcon = SelectedImage;

            ProfileConfiguration.Module = SelectedModule?.Module;

            if (_dataModelConditionGroup.Children.Any())
                ProfileConfiguration.ActivationCondition = _dataModelConditionGroup;

            _profileService.SaveProfileCategory(ProfileConfiguration.Category);

            Session.Close(nameof(Accept));
        }

        public void SelectIconFile()
        {
            VistaOpenFileDialog dialog = new();
            dialog.Filter = "All Graphics Types|*.bmp;*.jpg;*.jpeg;*.png;*.tif;*.tiff|BMP |*.bmp|GIF|*.gif|JPG|*.jpg;*.jpeg|PNG|*.png|TIFF|*.tif;*.tiff";
            dialog.Title = "Select profile icon";
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                SelectedImage = File.OpenRead(dialog.FileName);
                Execute.PostToUIThread(LoadIconBitmap);
            }
        }

        private void LoadIconBitmap()
        {
            BitmapImage selectedBitmap = new();
            selectedBitmap.BeginInit();
            selectedBitmap.StreamSource = SelectedImage;
            selectedBitmap.CacheOption = BitmapCacheOption.OnLoad;
            selectedBitmap.EndInit();
            selectedBitmap.Freeze();
            SelectedBitmap = selectedBitmap;
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using FluentValidation;
using MaterialDesignThemes.Wpf;
using Ookii.Dialogs.Wpf;
using Stylet;
using EnumUtilities = Artemis.UI.Shared.EnumUtilities;

namespace Artemis.UI.Screens.Sidebar.Dialogs.ProfileEdit
{
    public class ProfileEditViewModel : DialogViewModelBase
    {
        private readonly List<Module> _modules;
        private readonly IProfileService _profileService;
        private bool _changedImage;
        private bool _initializing;
        private string _profileName;
        private ProfileIconViewModel _selectedIcon;
        private ProfileConfigurationIconType _selectedIconType;
        private Stream _selectedImage;
        private ProfileModuleViewModel _selectedModule;
        private string _selectedIconPath;

        public ProfileEditViewModel(ProfileConfiguration profileConfiguration, bool isNew,
            IProfileService profileService,
            IPluginManagementService pluginManagementService,
            ISidebarVmFactory sidebarVmFactory,
            IModelValidator<ProfileEditViewModel> validator) : base(validator)
        {
            ProfileConfiguration = profileConfiguration;
            IsNew = isNew;

            _profileService = profileService;
            _modules = ProfileConfiguration.Module != null ? new List<Module> {ProfileConfiguration.Module} : new List<Module>();

            IconTypes = new BindableCollection<ValueDescription>(EnumUtilities.GetAllValuesAndDescriptions(typeof(ProfileConfigurationIconType)));
            HotkeyModes = new BindableCollection<ValueDescription>(EnumUtilities.GetAllValuesAndDescriptions(typeof(ProfileConfigurationHotkeyMode)));
            Icons = new BindableCollection<ProfileIconViewModel>();
            Modules = new BindableCollection<ProfileModuleViewModel>(
                pluginManagementService.GetFeaturesOfType<Module>().Where(m => !m.IsAlwaysAvailable).Select(m => new ProfileModuleViewModel(m))
            );
            Initializing = true;

            ModuleActivationRequirementsViewModel = new ModuleActivationRequirementsViewModel(sidebarVmFactory);
            ModuleActivationRequirementsViewModel.ConductWith(this);
            ModuleActivationRequirementsViewModel.SetModule(ProfileConfiguration.Module);
            EnableHotkeyViewModel = sidebarVmFactory.ProfileConfigurationHotkeyViewModel(ProfileConfiguration, false);
            EnableHotkeyViewModel.ConductWith(this);
            DisableHotkeyViewModel = sidebarVmFactory.ProfileConfigurationHotkeyViewModel(ProfileConfiguration, true);
            DisableHotkeyViewModel.ConductWith(this);

            _profileName = ProfileConfiguration.Name;
            _selectedModule = Modules.FirstOrDefault(m => m.Module == ProfileConfiguration.Module);
            _selectedIconType = ProfileConfiguration.Icon.IconType;
            _selectedImage = ProfileConfiguration.Icon.GetIconStream();

            Task.Run(() =>
            {
                Icons.AddRange(Enum.GetValues<PackIconKind>()
                    .GroupBy(i => i).Select(g => g.First()).Select(i => new ProfileIconViewModel(i))
                    .OrderBy(i => i.IconName)
                    .ToList());
                if (IsNew)
                    SelectedIcon = Icons[new Random().Next(0, Icons.Count - 1)];
                else
                    SelectedIcon = Icons.FirstOrDefault(i => i.Icon.ToString() == ProfileConfiguration.Icon.IconName);
                Initializing = false;
            });
        }

        public ModuleActivationRequirementsViewModel ModuleActivationRequirementsViewModel { get; }
        public ProfileConfigurationHotkeyViewModel EnableHotkeyViewModel { get; }
        public ProfileConfigurationHotkeyViewModel DisableHotkeyViewModel { get; }

        public bool IsNew { get; }
        public ProfileConfiguration ProfileConfiguration { get; }
        public BindableCollection<ValueDescription> IconTypes { get; }
        public BindableCollection<ValueDescription> HotkeyModes { get; }
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
            set
            {
                if (!SetAndNotify(ref _selectedIconType, value)) return;
                SelectedImage = null;
            }
        }

        public ProfileConfigurationHotkeyMode SelectedHotkeyMode
        {
            get => ProfileConfiguration.HotkeyMode;
            set
            {
                ProfileConfiguration.HotkeyMode = value;
                NotifyOfPropertyChange(nameof(SelectedHotkeyMode));
                NotifyOfPropertyChange(nameof(ShowEnableHotkey));
                NotifyOfPropertyChange(nameof(ShowDisableHotkey));

                EnableHotkeyViewModel.UpdateHotkeyDisplay();
                DisableHotkeyViewModel.UpdateHotkeyDisplay();
            }
        }

        public bool ShowEnableHotkey => ProfileConfiguration.HotkeyMode != ProfileConfigurationHotkeyMode.None;
        public bool ShowDisableHotkey => ProfileConfiguration.HotkeyMode == ProfileConfigurationHotkeyMode.EnableDisable;

        public Stream SelectedImage
        {
            get => _selectedImage;
            set => SetAndNotify(ref _selectedImage, value);
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

                ModuleActivationRequirementsViewModel.SetModule(value?.Module);
            }
        }

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
            if (SelectedIconType == ProfileConfigurationIconType.MaterialIcon)
                ProfileConfiguration.Icon.SetIconByName(SelectedIcon?.Icon.ToString());
            else if (_selectedIconPath != null)
            {
                await using FileStream fileStream = File.OpenRead(_selectedIconPath);
                ProfileConfiguration.Icon.SetIconByStream(Path.GetFileName(_selectedIconPath), fileStream);
            }

            ProfileConfiguration.Module = SelectedModule?.Module;

            if (_changedImage)
                _profileService.SaveProfileConfigurationIcon(ProfileConfiguration);
            _profileService.SaveProfileCategory(ProfileConfiguration.Category);

            Session.Close(nameof(Accept));
        }

        public void SelectBitmapFile()
        {
            VistaOpenFileDialog dialog = new()
            {
                Filter = "All Graphics Types|*.bmp;*.jpg;*.jpeg;*.png;*.tif;*.tiff|BMP |*.bmp|GIF|*.gif|JPG|*.jpg;*.jpeg|PNG|*.png|TIFF|*.tif;*.tiff",
                Title = "Select profile icon"
            };
            bool? result = dialog.ShowDialog();
            if (result != true)
                return;

            _changedImage = true;

            // TODO: Scale down to 100x100-ish
            SelectedImage = File.OpenRead(dialog.FileName);
            _selectedIconPath = dialog.FileName;
        }

        public void SelectSvgFile()
        {
            VistaOpenFileDialog dialog = new()
            {
                Filter = "Scalable Vector Graphics|*.svg",
                Title = "Select profile icon"
            };
            bool? result = dialog.ShowDialog();
            if (result != true)
                return;

            _changedImage = true;
            SelectedImage = File.OpenRead(dialog.FileName);
            _selectedIconPath = dialog.FileName;
        }

        #region Overrides of Screen

        protected override void OnInitialActivate()
        {
            _profileService.HotkeysEnabled = false;
            base.OnInitialActivate();
        }

        protected override void OnClose()
        {
            _profileService.HotkeysEnabled = true;
            base.OnClose();
        }

        #endregion
    }

    public class ProfileEditViewModelValidator : AbstractValidator<ProfileEditViewModel>
    {
        public ProfileEditViewModelValidator()
        {
            RuleFor(m => m.ProfileName).NotEmpty().WithMessage("Profile name may not be empty");
        }
    }
}
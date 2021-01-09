using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Core.Services;
using Artemis.UI.Extensions;
using Artemis.UI.Screens.ProfileEditor.Dialogs;
using Artemis.UI.Screens.ProfileEditor.DisplayConditions;
using Artemis.UI.Screens.ProfileEditor.LayerProperties;
using Artemis.UI.Screens.ProfileEditor.ProfileTree;
using Artemis.UI.Screens.ProfileEditor.Visualization;
using Artemis.UI.Shared.Services;
using MaterialDesignThemes.Wpf;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor
{
    public class ProfileEditorViewModel : Screen
    {
        private readonly IModuleService _moduleService;
        private readonly IMessageService _messageService;
        private readonly IProfileEditorService _profileEditorService;
        private readonly IProfileService _profileService;
        private readonly ISettingsService _settingsService;
        private PluginSetting<GridLength> _bottomPanelsHeight;
        private PluginSetting<GridLength> _dataModelConditionsHeight;
        private DisplayConditionsViewModel _displayConditionsViewModel;
        private PluginSetting<GridLength> _elementPropertiesWidth;
        private LayerPropertiesViewModel _layerPropertiesViewModel;
        private BindableCollection<ProfileDescriptor> _profiles;
        private ProfileTreeViewModel _profileTreeViewModel;
        private ProfileViewModel _profileViewModel;
        private ProfileDescriptor _selectedProfile;
        private PluginSetting<GridLength> _sidePanelsWidth;

        public ProfileEditorViewModel(ProfileModule module,
            ProfileViewModel profileViewModel,
            ProfileTreeViewModel profileTreeViewModel,
            DisplayConditionsViewModel dataModelConditionsViewModel,
            LayerPropertiesViewModel layerPropertiesViewModel,
            IProfileEditorService profileEditorService,
            IProfileService profileService,
            IDialogService dialogService,
            ISettingsService settingsService,
            IModuleService moduleService,
            IMessageService messageService)
        {
            _profileEditorService = profileEditorService;
            _profileService = profileService;
            _settingsService = settingsService;
            _moduleService = moduleService;
            _messageService = messageService;

            DisplayName = "PROFILE EDITOR";
            Module = module;
            DialogService = dialogService;

            Profiles = new BindableCollection<ProfileDescriptor>();

            // Populate the panels
            ProfileViewModel = profileViewModel;
            ProfileViewModel.ConductWith(this);
            ProfileTreeViewModel = profileTreeViewModel;
            ProfileTreeViewModel.ConductWith(this);
            DisplayConditionsViewModel = dataModelConditionsViewModel;
            DisplayConditionsViewModel.ConductWith(this);
            LayerPropertiesViewModel = layerPropertiesViewModel;
            LayerPropertiesViewModel.ConductWith(this);
        }

        public ProfileModule Module { get; }
        public IDialogService DialogService { get; }

        public DisplayConditionsViewModel DisplayConditionsViewModel
        {
            get => _displayConditionsViewModel;
            set => SetAndNotify(ref _displayConditionsViewModel, value);
        }

        public LayerPropertiesViewModel LayerPropertiesViewModel
        {
            get => _layerPropertiesViewModel;
            set => SetAndNotify(ref _layerPropertiesViewModel, value);
        }

        public ProfileTreeViewModel ProfileTreeViewModel
        {
            get => _profileTreeViewModel;
            set => SetAndNotify(ref _profileTreeViewModel, value);
        }

        public ProfileViewModel ProfileViewModel
        {
            get => _profileViewModel;
            set => SetAndNotify(ref _profileViewModel, value);
        }

        public BindableCollection<ProfileDescriptor> Profiles
        {
            get => _profiles;
            set => SetAndNotify(ref _profiles, value);
        }

        public PluginSetting<GridLength> SidePanelsWidth
        {
            get => _sidePanelsWidth;
            set => SetAndNotify(ref _sidePanelsWidth, value);
        }

        public PluginSetting<GridLength> DataModelConditionsHeight
        {
            get => _dataModelConditionsHeight;
            set => SetAndNotify(ref _dataModelConditionsHeight, value);
        }

        public PluginSetting<GridLength> BottomPanelsHeight
        {
            get => _bottomPanelsHeight;
            set => SetAndNotify(ref _bottomPanelsHeight, value);
        }

        public PluginSetting<GridLength> ElementPropertiesWidth
        {
            get => _elementPropertiesWidth;
            set => SetAndNotify(ref _elementPropertiesWidth, value);
        }

        public ProfileDescriptor SelectedProfile
        {
            get => _selectedProfile;
            set
            {
                if (!SetAndNotify(ref _selectedProfile, value)) return;
                NotifyOfPropertyChange(nameof(CanDeleteActiveProfile));
                ActivateSelectedProfile();
            }
        }

        public bool CanDeleteActiveProfile => SelectedProfile != null && Profiles.Count > 1;

        public ProfileDescriptor CreateProfile(string name)
        {
            ProfileDescriptor profile = _profileService.CreateProfileDescriptor(Module, name);
            Profiles.Add(profile);
            return profile;
        }

        public async Task AddProfile()
        {
            object result = await DialogService.ShowDialog<ProfileCreateViewModel>();
            if (result is string name)
            {
                ProfileDescriptor newProfile = CreateProfile(name);
                SelectedProfile = newProfile;
            }
        }

        public async Task DeleteProfile(ProfileDescriptor profileDescriptor)
        {
            bool result = await DialogService.ShowConfirmDialog(
                "Delete profile",
                $"Are you sure you want to delete '{profileDescriptor.Name}'? This cannot be undone."
            );

            if (result)
                RemoveProfile(profileDescriptor);
        }

        public async Task DeleteActiveProfile()
        {
            bool result = await DialogService.ShowConfirmDialog(
                "Delete active profile",
                "Are you sure you want to delete your currently active profile? This cannot be undone."
            );

            if (result)
                RemoveProfile(SelectedProfile);
        }

        public async Task RenameActiveProfile()
        {
            if (_profileEditorService.SelectedProfile == null)
                return;

            Profile profile = _profileEditorService.SelectedProfile;
            object result = await DialogService.ShowDialog<ProfileEditViewModel>(new Dictionary<string, object> {{"profile", profile}});
            if (result is string name)
            {
                profile.Name = name;
                _profileEditorService.UpdateSelectedProfile();

                // The descriptors are immutable and need to be reloaded to reflect the name change
                LoadProfiles();
                SelectedProfile = Profiles.FirstOrDefault(p => p.Id == _profileEditorService.SelectedProfile.EntityId) ?? Profiles.FirstOrDefault();
            }
        }

        public void DuplicateActiveProfile()
        {
            string encoded = _profileService.ExportProfile(SelectedProfile);
            ProfileDescriptor copy = _profileService.ImportProfile(encoded, Module, "copy");

            Profiles.Add(copy);
            Profiles.Sort(p => p.Name);
            SelectedProfile = copy;
        }

        public async Task ExportActiveProfile()
        {
            await DialogService.ShowDialog<ProfileExportViewModel>(new Dictionary<string, object>
            {
                {"profileDescriptor", SelectedProfile}
            });
        }

        public async Task ImportProfile()
        {
            object result = await DialogService.ShowDialog<ProfileImportViewModel>(new Dictionary<string, object>
            {
                {"profileModule", Module}
            });

            if (result != null && result is ProfileDescriptor descriptor)
            {
                Profiles.Add(descriptor);
                Profiles.Sort(p => p.Name);
                SelectedProfile = descriptor;
            }
        }

        public void Undo()
        {
            // Expanded status is also undone because undoing works a bit crude, that's annoying
            List<LayerPropertyGroupViewModel> beforeGroups = LayerPropertiesViewModel.GetAllLayerPropertyGroupViewModels();
            List<string> expandedPaths = beforeGroups.Where(g => g.IsExpanded).Select(g => g.LayerPropertyGroup.Path).ToList();
            // Store the focused element so we can restore it later
            IInputElement focusedElement = FocusManager.GetFocusedElement(Window.GetWindow(View));

            if (!_profileEditorService.UndoUpdateProfile())
            {
                _messageService.ShowMessage("Nothing to undo");
                return;
            }

            // Restore the expanded status
            foreach (LayerPropertyGroupViewModel allLayerPropertyGroupViewModel in LayerPropertiesViewModel.GetAllLayerPropertyGroupViewModels())
                allLayerPropertyGroupViewModel.IsExpanded = expandedPaths.Contains(allLayerPropertyGroupViewModel.LayerPropertyGroup.Path);
            // Restore the focused element
            Execute.PostToUIThread(async () =>
            {
                await Task.Delay(50);
                focusedElement?.Focus();
            });

            _messageService.ShowMessage("Undid profile update", "REDO", Redo);
        }

        public void Redo()
        {
            // Expanded status is also undone because undoing works a bit crude, that's annoying
            List<LayerPropertyGroupViewModel> beforeGroups = LayerPropertiesViewModel.GetAllLayerPropertyGroupViewModels();
            List<string> expandedPaths = beforeGroups.Where(g => g.IsExpanded).Select(g => g.LayerPropertyGroup.Path).ToList();
            // Store the focused element so we can restore it later
            IInputElement focusedElement = FocusManager.GetFocusedElement(Window.GetWindow(View));

            if (!_profileEditorService.RedoUpdateProfile())
            {
                _messageService.ShowMessage("Nothing to redo");
                return;
            }

            // Restore the expanded status
            foreach (LayerPropertyGroupViewModel allLayerPropertyGroupViewModel in LayerPropertiesViewModel.GetAllLayerPropertyGroupViewModels())
                allLayerPropertyGroupViewModel.IsExpanded = expandedPaths.Contains(allLayerPropertyGroupViewModel.LayerPropertyGroup.Path);
            // Restore the focused element
            Execute.PostToUIThread(async () =>
            {
                await Task.Delay(50);
                focusedElement?.Focus();
            });

            _messageService.ShowMessage("Redid profile update", "UNDO", Undo);
        }

        protected override void OnInitialActivate()
        {
            LoadWorkspaceSettings();
            Module.IsProfileUpdatingDisabled = true;
            Module.ActiveProfile?.Reset();
            Module.ActiveProfileChanged += ModuleOnActiveProfileChanged;
            LoadProfiles();

            // If the module already has an active profile use that, the override won't trigger a profile change
            if (Module.ActiveProfile != null)
                SelectedProfile = Profiles.FirstOrDefault(d => d.Id == Module.ActiveProfile.EntityId);

            Task.Run(async () => { await _moduleService.SetActiveModuleOverride(Module); });
            base.OnInitialActivate();
        }

        protected override void OnClose()
        {
            SaveWorkspaceSettings();
            Module.IsProfileUpdatingDisabled = false;
            Module.ActiveProfile?.Reset();
            Module.ActiveProfileChanged -= ModuleOnActiveProfileChanged;

            _profileEditorService.ChangeSelectedProfile(null);
            Task.Run(async () => { await _moduleService.SetActiveModuleOverride(null); });
            base.OnClose();
        }

        private void RemoveProfile(ProfileDescriptor profileDescriptor)
        {
            if (SelectedProfile == profileDescriptor && !CanDeleteActiveProfile)
                return;

            int index = Profiles.IndexOf(profileDescriptor);

            // Get a new active profile
            ProfileDescriptor newActiveProfile = index - 1 > -1 ? Profiles[index - 1] : Profiles[index + 1];

            // Activate the new active profile
            SelectedProfile = newActiveProfile;

            // Remove the old one
            Profiles.Remove(profileDescriptor);
            _profileService.DeleteProfile(profileDescriptor);
        }

        private void ActivateSelectedProfile()
        {
            Execute.PostToUIThread(async () =>
            {
                if (SelectedProfile == null)
                    return;

                Task<Profile> changeTask = _profileService.ActivateProfileAnimated(SelectedProfile);
                _profileEditorService.ChangeSelectedProfile(null);
                Profile profile = await changeTask;
                _profileEditorService.ChangeSelectedProfile(profile);
            });
        }

        private void ModuleOnActiveProfileChanged(object sender, EventArgs e)
        {
            if (Module.ActiveProfile == null)
                SelectedProfile = null;
            else
                SelectedProfile = Profiles.FirstOrDefault(d => d.Id == Module.ActiveProfile.EntityId);
        }

        private void LoadWorkspaceSettings()
        {
            SidePanelsWidth = _settingsService.GetSetting("ProfileEditor.SidePanelsWidth", new GridLength(385));
            DataModelConditionsHeight = _settingsService.GetSetting("ProfileEditor.DataModelConditionsHeight", new GridLength(345));
            BottomPanelsHeight = _settingsService.GetSetting("ProfileEditor.BottomPanelsHeight", new GridLength(265));
            ElementPropertiesWidth = _settingsService.GetSetting("ProfileEditor.ElementPropertiesWidth", new GridLength(545));
        }

        private void SaveWorkspaceSettings()
        {
            SidePanelsWidth.Save();
            DataModelConditionsHeight.Save();
            BottomPanelsHeight.Save();
            ElementPropertiesWidth.Save();
        }

        private void LoadProfiles()
        {
            // Get all profiles from the database
            Profiles.Clear();
            Profiles.AddRange(_profileService.GetProfileDescriptors(Module).OrderBy(d => d.Name));
        }
    }
}
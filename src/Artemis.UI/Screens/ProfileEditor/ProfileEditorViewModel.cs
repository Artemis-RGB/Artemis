using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Core.Services;
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
    public class ProfileEditorViewModel : Conductor<ProfileEditorPanelViewModel>.Collection.AllActive
    {
        private readonly IModuleService _moduleService;
        private readonly IProfileEditorService _profileEditorService;
        private readonly IProfileService _profileService;
        private readonly ISettingsService _settingsService;
        private readonly ISnackbarMessageQueue _snackbarMessageQueue;
        private PluginSetting<GridLength> _bottomPanelsHeight;
        private PluginSetting<GridLength> _displayConditionsHeight;
        private DisplayConditionsViewModel _displayConditionsViewModel;
        private PluginSetting<GridLength> _elementPropertiesWidth;
        private LayerPropertiesViewModel _layerPropertiesViewModel;
        private BindableCollection<ProfileDescriptor> _profiles;
        private ProfileTreeViewModel _profileTreeViewModel;
        private ProfileViewModel _profileViewModel;
        private ProfileDescriptor _selectedProfile;
        private PluginSetting<GridLength> _sidePanelsWidth;

        public ProfileEditorViewModel(ProfileModule module,
            ICollection<ProfileEditorPanelViewModel> viewModels,
            IProfileEditorService profileEditorService,
            IProfileService profileService,
            IDialogService dialogService,
            ISettingsService settingsService,
            IModuleService moduleService,
            ISnackbarMessageQueue snackbarMessageQueue)
        {
            _profileEditorService = profileEditorService;
            _profileService = profileService;
            _settingsService = settingsService;
            _moduleService = moduleService;
            _snackbarMessageQueue = snackbarMessageQueue;

            DisplayName = "PROFILE EDITOR";
            Module = module;
            DialogService = dialogService;

            Profiles = new BindableCollection<ProfileDescriptor>();

            // Run this first to let VMs activate without causing constant UI updates
            Items.AddRange(viewModels);

            // Populate the panels
            ProfileViewModel = (ProfileViewModel) viewModels.First(vm => vm is ProfileViewModel);
            ProfileTreeViewModel = (ProfileTreeViewModel) viewModels.First(vm => vm is ProfileTreeViewModel);
            DisplayConditionsViewModel = (DisplayConditionsViewModel) viewModels.First(vm => vm is DisplayConditionsViewModel);
            LayerPropertiesViewModel = (LayerPropertiesViewModel) viewModels.First(vm => vm is LayerPropertiesViewModel);
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

        public PluginSetting<GridLength> DisplayConditionsHeight
        {
            get => _displayConditionsHeight;
            set => SetAndNotify(ref _displayConditionsHeight, value);
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
            var profile = _profileService.CreateProfileDescriptor(Module, name);
            Profiles.Add(profile);
            return profile;
        }

        public async Task AddProfile()
        {
            var result = await DialogService.ShowDialog<ProfileCreateViewModel>();
            if (result is string name)
            {
                var newProfile = CreateProfile(name);
                SelectedProfile = newProfile;
            }
        }

        public async Task DeleteProfile(ProfileDescriptor profileDescriptor)
        {
            var result = await DialogService.ShowConfirmDialog(
                "Delete profile",
                $"Are you sure you want to delete '{profileDescriptor.Name}'? This cannot be undone."
            );

            if (result)
                RemoveProfile(profileDescriptor);
        }

        public async Task DeleteActiveProfile()
        {
            var result = await DialogService.ShowConfirmDialog(
                "Delete active profile",
                "Are you sure you want to delete your currently active profile? This cannot be undone."
            );

            if (result)
                RemoveProfile(SelectedProfile);
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
            await DialogService.ShowDialog<ProfileImportViewModel>(new Dictionary<string, object>
            {
                {"profileModule", Module}
            });
        }

        public void Undo()
        {
            // Expanded status is also undone because undoing works a bit crude, that's annoying
            var beforeGroups = LayerPropertiesViewModel.GetAllLayerPropertyGroupViewModels();
            var expandedPaths = beforeGroups.Where(g => g.IsExpanded).Select(g => g.LayerPropertyGroup.Path).ToList();

            if (!_profileEditorService.UndoUpdateProfile())
            {
                _snackbarMessageQueue.Enqueue("Nothing to undo");
                return;
            }

            // Restore the expanded status
            foreach (var allLayerPropertyGroupViewModel in LayerPropertiesViewModel.GetAllLayerPropertyGroupViewModels())
                allLayerPropertyGroupViewModel.IsExpanded = expandedPaths.Contains(allLayerPropertyGroupViewModel.LayerPropertyGroup.Path);

            _snackbarMessageQueue.Enqueue("Undid profile update", "REDO", Redo);
        }

        public void Redo()
        {
            // Expanded status is also undone because undoing works a bit crude, that's annoying
            var beforeGroups = LayerPropertiesViewModel.GetAllLayerPropertyGroupViewModels();
            var expandedPaths = beforeGroups.Where(g => g.IsExpanded).Select(g => g.LayerPropertyGroup.Path).ToList();

            if (!_profileEditorService.RedoUpdateProfile())
            {
                _snackbarMessageQueue.Enqueue("Nothing to redo");
                return;
            }

            // Restore the expanded status
            foreach (var allLayerPropertyGroupViewModel in LayerPropertiesViewModel.GetAllLayerPropertyGroupViewModels())
                allLayerPropertyGroupViewModel.IsExpanded = expandedPaths.Contains(allLayerPropertyGroupViewModel.LayerPropertyGroup.Path);

            _snackbarMessageQueue.Enqueue("Redid profile update", "UNDO", Undo);
        }

        protected override void OnActivate()
        {
            LoadWorkspaceSettings();
            Module.IsProfileUpdatingDisabled = true;
            Module.ActiveProfileChanged += ModuleOnActiveProfileChanged;
            LoadProfiles();

            // If the module already has an active profile use that, the override won't trigger a profile change
            if (Module.ActiveProfile != null)
                SelectedProfile = Profiles.FirstOrDefault(d => d.Id == Module.ActiveProfile.EntityId);

            Task.Run(async () => { await _moduleService.SetActiveModuleOverride(Module); });
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            SaveWorkspaceSettings();
            Module.IsProfileUpdatingDisabled = false;
            Module.ActiveProfileChanged -= ModuleOnActiveProfileChanged;

            _profileEditorService.ChangeSelectedProfile(null);
            Task.Run(async () => { await _moduleService.SetActiveModuleOverride(null); });
            base.OnDeactivate();
        }

        private void RemoveProfile(ProfileDescriptor profileDescriptor)
        {
            if (SelectedProfile == profileDescriptor && !CanDeleteActiveProfile)
                return;

            var index = Profiles.IndexOf(profileDescriptor);

            // Get a new active profile
            var newActiveProfile = index - 1 > -1 ? Profiles[index - 1] : Profiles[index + 1];

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

                var changeTask = _profileService.ActivateProfileAnimated(SelectedProfile);
                _profileEditorService.ChangeSelectedProfile(null);
                var profile = await changeTask;
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
            DisplayConditionsHeight = _settingsService.GetSetting("ProfileEditor.DisplayConditionsHeight", new GridLength(345));
            BottomPanelsHeight = _settingsService.GetSetting("ProfileEditor.BottomPanelsHeight", new GridLength(265));
            ElementPropertiesWidth = _settingsService.GetSetting("ProfileEditor.ElementPropertiesWidth", new GridLength(545));
        }

        private void SaveWorkspaceSettings()
        {
            SidePanelsWidth.Save();
            DisplayConditionsHeight.Save();
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
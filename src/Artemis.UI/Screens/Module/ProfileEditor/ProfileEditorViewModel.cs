using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Artemis.Core.Models.Profile;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.Models;
using Artemis.Core.Services;
using Artemis.Core.Services.Storage.Interfaces;
using Artemis.UI.Screens.Module.ProfileEditor.Dialogs;
using Artemis.UI.Screens.Module.ProfileEditor.DisplayConditions;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties;
using Artemis.UI.Screens.Module.ProfileEditor.ProfileTree;
using Artemis.UI.Screens.Module.ProfileEditor.Visualization;
using Artemis.UI.Shared.Services.Interfaces;
using MaterialDesignThemes.Wpf;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor
{
    public class ProfileEditorViewModel : Conductor<ProfileEditorPanelViewModel>.Collection.AllActive
    {
        private readonly IProfileEditorService _profileEditorService;
        private readonly IProfileService _profileService;
        private readonly ISettingsService _settingsService;
        private readonly ISnackbarMessageQueue _snackbarMessageQueue;
        private BindableCollection<Profile> _profiles;
        private PluginSetting<GridLength> _sidePanelsWidth;
        private PluginSetting<GridLength> _displayConditionsHeight;
        private PluginSetting<GridLength> _bottomPanelsHeight;
        private PluginSetting<GridLength> _elementPropertiesWidth;
        private ProfileViewModel _profileViewModel;
        private ProfileTreeViewModel _profileTreeViewModel;
        private LayerPropertiesViewModel _layerPropertiesViewModel;
        private DisplayConditionsViewModel _displayConditionsViewModel;

        public ProfileEditorViewModel(ProfileModule module,
            ICollection<ProfileEditorPanelViewModel> viewModels,
            IProfileEditorService profileEditorService,
            IProfileService profileService,
            IDialogService dialogService,
            ISettingsService settingsService,
            ISnackbarMessageQueue snackbarMessageQueue)
        {
            _profileEditorService = profileEditorService;
            _profileService = profileService;
            _settingsService = settingsService;
            _snackbarMessageQueue = snackbarMessageQueue;

            DisplayName = "Profile editor";
            Module = module;
            DialogService = dialogService;

            Profiles = new BindableCollection<Profile>();

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

        public BindableCollection<Profile> Profiles
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

        public Profile SelectedProfile
        {
            get => Module.ActiveProfile;
            set => ChangeSelectedProfile(value);
        }

        public bool CanDeleteActiveProfile => SelectedProfile != null && Profiles.Count > 1;

        public Profile CreateProfile(string name)
        {
            var profile = _profileService.CreateProfile(Module, name);
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

        public async Task DeleteActiveProfile()
        {
            var result = await DialogService.ShowConfirmDialog(
                "Delete active profile",
                "Are you sure you want to delete your currently active profile? This cannot be undone."
            );

            if (!result || !CanDeleteActiveProfile)
                return;

            var profile = SelectedProfile;
            var index = Profiles.IndexOf(profile);

            // Get a new active profile
            var newActiveProfile = index - 1 > -1 ? Profiles[index - 1] : Profiles[index + 1];

            // Activate the new active profile
            SelectedProfile = newActiveProfile;

            // Remove the old one
            Profiles.Remove(profile);
            _profileService.DeleteProfile(profile);
        }

        public void Undo()
        {
            // Expanded status is also undone because undoing works a bit crude, that's annoying
            var beforeGroups = LayerPropertiesViewModel.GetAllLayerPropertyGroupViewModels();
            var expandedPaths = beforeGroups.Where(g => g.IsExpanded).Select(g => g.LayerPropertyGroup.Path).ToList();

            if (!_profileEditorService.UndoUpdateProfile(Module))
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

            if (!_profileEditorService.RedoUpdateProfile(Module))
            {
                _snackbarMessageQueue.Enqueue("Nothing to redo");
                return;
            }

            // Restore the expanded status
            foreach (var allLayerPropertyGroupViewModel in LayerPropertiesViewModel.GetAllLayerPropertyGroupViewModels())
                allLayerPropertyGroupViewModel.IsExpanded = expandedPaths.Contains(allLayerPropertyGroupViewModel.LayerPropertyGroup.Path);

            _snackbarMessageQueue.Enqueue("Redid profile update", "UNDO", Undo);
        }

        protected override void OnInitialActivate()
        {
            LoadWorkspaceSettings();
            Module.IsProfileUpdatingDisabled = true;
            Module.ActiveProfileChanged += ModuleOnActiveProfileChanged;
            Execute.PostToUIThread(LoadProfiles);
            base.OnInitialActivate();
        }

        protected override void OnClose()
        {
            SaveWorkspaceSettings();
            Module.IsProfileUpdatingDisabled = false;
            Module.ActiveProfileChanged -= ModuleOnActiveProfileChanged;
            base.OnClose();
        }

        private void ChangeSelectedProfile(Profile profile)
        {
            var oldProfile = Module.ActiveProfile;
            _profileService.ActivateProfile(Module, profile);

            if (oldProfile != null)
                _profileService.UpdateProfile(oldProfile, false);
            if (profile != null)
                _profileService.UpdateProfile(profile, false);

            if (_profileEditorService.SelectedProfile != profile)
                _profileEditorService.ChangeSelectedProfile(profile);

            NotifyOfPropertyChange(nameof(SelectedProfile));
            NotifyOfPropertyChange(nameof(CanDeleteActiveProfile));
        }

        private void ModuleOnActiveProfileChanged(object sender, EventArgs e)
        {
            if (SelectedProfile == Module.ActiveProfile)
                return;

            SelectedProfile = Profiles.FirstOrDefault(p => p == Module.ActiveProfile);
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
            var profiles = _profileService.GetProfiles(Module);
            // Get the latest active profile, this falls back to just any profile so if null, create a default profile
            var activeProfile = _profileService.GetActiveProfile(Module) ?? _profileService.CreateProfile(Module, "Default");

            // GetActiveProfile can return a duplicate because inactive profiles aren't kept in memory, make sure it's unique in the profiles list
            profiles = profiles.Where(p => p.EntityId != activeProfile.EntityId).ToList();
            profiles.Add(activeProfile);

            // Populate the UI collection
            Profiles.AddRange(profiles.Except(Profiles).ToList());
            Profiles.RemoveRange(Profiles.Except(profiles).ToList());
            var index = 0;
            foreach (var profile in Profiles.OrderBy(p => p.Name).ToList())
            {
                Profiles.Move(Profiles.IndexOf(profile), index);
                index++;
            }

            SelectedProfile = activeProfile;
            if (_profileEditorService.SelectedProfile != activeProfile)
                _profileEditorService.ChangeSelectedProfile(activeProfile);
            if (!activeProfile.IsActivated)
                _profileService.ActivateProfile(Module, activeProfile);
        }
    }
}
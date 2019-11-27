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
using Artemis.UI.Screens.Module.ProfileEditor.ElementProperties;
using Artemis.UI.Screens.Module.ProfileEditor.LayerElements;
using Artemis.UI.Screens.Module.ProfileEditor.ProfileElements;
using Artemis.UI.Screens.Module.ProfileEditor.ProfileElements.ProfileElement;
using Artemis.UI.Screens.Module.ProfileEditor.Visualization;
using Artemis.UI.Services.Interfaces;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor
{
    public class ProfileEditorViewModel : Conductor<ProfileEditorPanelViewModel>.Collection.AllActive
    {
        private readonly IProfileService _profileService;
        private readonly ISettingsService _settingsService;

        public ProfileEditorViewModel(ProfileModule module, ICollection<ProfileEditorPanelViewModel> viewModels, IProfileService profileService,
            IDialogService dialogService, ISettingsService settingsService)
        {
            _profileService = profileService;
            _settingsService = settingsService;

            DisplayName = "Profile editor";
            Module = module;
            DialogService = dialogService;

            DisplayConditionsViewModel = (DisplayConditionsViewModel) viewModels.First(vm => vm is DisplayConditionsViewModel);
            ElementPropertiesViewModel = (ElementPropertiesViewModel) viewModels.First(vm => vm is ElementPropertiesViewModel);
            LayerElementsViewModel = (LayerElementsViewModel) viewModels.First(vm => vm is LayerElementsViewModel);
            ProfileElementsViewModel = (ProfileElementsViewModel) viewModels.First(vm => vm is ProfileElementsViewModel);
            ProfileViewModel = (ProfileViewModel) viewModels.First(vm => vm is ProfileViewModel);
            Profiles = new BindableCollection<Profile>();

            Items.AddRange(viewModels);

            module.ActiveProfileChanged += ModuleOnActiveProfileChanged;
        }

        public ProfileModule Module { get; }
        public IDialogService DialogService { get; }
        public DisplayConditionsViewModel DisplayConditionsViewModel { get; }
        public ElementPropertiesViewModel ElementPropertiesViewModel { get; }
        public LayerElementsViewModel LayerElementsViewModel { get; }
        public ProfileElementsViewModel ProfileElementsViewModel { get; }
        public ProfileViewModel ProfileViewModel { get; }
        public BindableCollection<Profile> Profiles { get; set; }

        public PluginSetting<GridLength> SidePanelsWidth { get; set; }
        public PluginSetting<GridLength> DisplayConditionsHeight { get; set; }
        public PluginSetting<GridLength> BottomPanelsHeight { get; set; }
        public PluginSetting<GridLength> ElementPropertiesWidth { get; set; }

        public Profile SelectedProfile
        {
            get => Module.ActiveProfile;
            set => ChangeSelectedProfile(value);
        }

        public bool CanDeleteActiveProfile => SelectedProfile != null && Profiles.Count > 1;

        private void ChangeSelectedProfile(Profile profile)
        {
            if (profile == Module.ActiveProfile)
                return;

            var oldProfile = Module.ActiveProfile;
            Module.ChangeActiveProfile(profile);

            foreach (var panelViewModel in Items)
            {
                panelViewModel.ProfileEditorViewModel = this;
                panelViewModel.ActiveProfileChanged();
            }

            if (oldProfile != null)
                _profileService.UpdateProfile(oldProfile, false);
            if (profile != null)
                _profileService.UpdateProfile(profile, false);
        }

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
                CreateProfile(name);
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

        private void ModuleOnActiveProfileChanged(object sender, EventArgs e)
        {
            if (SelectedProfile == Module.ActiveProfile)
                return;

            SelectedProfile = Profiles.FirstOrDefault(p => p == Module.ActiveProfile);
        }

        protected override void OnActivate()
        {
            LoadWorkspaceSettings();
            Task.Run(() =>
            {
                LoadProfiles();
                foreach (var panelViewModel in Items)
                {
                    panelViewModel.ProfileEditorViewModel = this;
                    panelViewModel.ActiveProfileChanged();
                }
            });
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            SaveWorkspaceSettings();
            base.OnDeactivate();
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

            Execute.PostToUIThread(() =>
            {
                // Populate the UI collection
                Profiles.Clear();
                Profiles.AddRange(profiles.OrderBy(p => p.Name));

                SelectedProfile = activeProfile;
            });

            if (!activeProfile.IsActivated)
                Module.ChangeActiveProfile(activeProfile);
        }

        public void OnProfileUpdated(ProfileEditorPanelViewModel source = null)
        {
            _profileService.UpdateProfile(SelectedProfile, true);

            foreach (var panelViewModel in Items.Where(p => p != source))
                panelViewModel.ActiveProfileUpdated();
        }

        public void OnProfileElementSelected(ProfileElementViewModel profileElement, ProfileEditorPanelViewModel source = null)
        {
            foreach (var panelViewModel in Items.Where(p => p != source))
                panelViewModel.ProfileElementSelected(profileElement);
        }
    }
}
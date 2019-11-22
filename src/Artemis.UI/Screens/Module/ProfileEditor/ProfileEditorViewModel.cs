using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core.Models.Profile;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Services.Storage.Interfaces;
using Artemis.UI.Screens.Module.ProfileEditor.Dialogs;
using Artemis.UI.Screens.Module.ProfileEditor.DisplayConditions;
using Artemis.UI.Screens.Module.ProfileEditor.ElementProperties;
using Artemis.UI.Screens.Module.ProfileEditor.LayerElements;
using Artemis.UI.Screens.Module.ProfileEditor.Layers;
using Artemis.UI.Screens.Module.ProfileEditor.Visualization;
using Artemis.UI.Services.Interfaces;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor
{
    public class ProfileEditorViewModel : Conductor<ProfileEditorPanelViewModel>.Collection.AllActive
    {
        private readonly IProfileService _profileService;
        private readonly IDialogService _dialogService;

        public ProfileEditorViewModel(ProfileModule module, ICollection<ProfileEditorPanelViewModel> viewModels, IProfileService profileService, IDialogService dialogService)
        {
            _profileService = profileService;
            _dialogService = dialogService;

            DisplayName = "Profile editor";
            Module = module;

            DisplayConditionsViewModel = (DisplayConditionsViewModel) viewModels.First(vm => vm is DisplayConditionsViewModel);
            ElementPropertiesViewModel = (ElementPropertiesViewModel) viewModels.First(vm => vm is ElementPropertiesViewModel);
            LayerElementsViewModel = (LayerElementsViewModel) viewModels.First(vm => vm is LayerElementsViewModel);
            LayersViewModel = (LayersViewModel) viewModels.First(vm => vm is LayersViewModel);
            ProfileViewModel = (ProfileViewModel) viewModels.First(vm => vm is ProfileViewModel);
            Profiles = new BindableCollection<Profile>();

            Items.AddRange(viewModels);

            module.ActiveProfileChanged += ModuleOnActiveProfileChanged;            
        }

        public ProfileModule Module { get; }
        public DisplayConditionsViewModel DisplayConditionsViewModel { get; }
        public ElementPropertiesViewModel ElementPropertiesViewModel { get; }
        public LayerElementsViewModel LayerElementsViewModel { get; }
        public LayersViewModel LayersViewModel { get; }
        public ProfileViewModel ProfileViewModel { get; }

        public BindableCollection<Profile> Profiles { get; set; }
        public Profile SelectedProfile
        {
            get => Module.ActiveProfile;
            set => ChangeSelectedProfile(value);
        }

        private void ChangeSelectedProfile(Profile profile)
        {
            if (profile == Module.ActiveProfile)
                return;

            var oldProfile = Module.ActiveProfile;
            Module.ChangeActiveProfile(profile);

            if (oldProfile != null)
                _profileService.UpdateProfile(oldProfile, false);
            if (profile != null)
                _profileService.UpdateProfile(profile, false);
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
            var result = await _dialogService.ShowDialog<ProfileCreateViewModel>();
            if (result is string name)
                CreateProfile(name);
        }

        public async Task DeleteActiveProfile()
        {
            var result = await _dialogService.ShowConfirmDialog(
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
            SelectedProfile = Profiles.FirstOrDefault(p => p == Module.ActiveProfile);
        }

        protected override void OnActivate()
        {
            Task.Run(() => LoadProfiles());
            base.OnActivate();
        }

        private void LoadProfiles()
        {
            // Get all profiles from the database
            var profiles = _profileService.GetProfiles(Module);
            var activeProfile = _profileService.GetActiveProfile(Module);
            if (activeProfile == null)
            {
                activeProfile = CreateProfile("Default");
                profiles.Add(activeProfile);
            }
            
            // GetActiveProfile can return a duplicate because inactive profiles aren't kept in memory, make sure it's unique in the profiles list
            profiles = profiles.Where(p => p.EntityId != activeProfile.EntityId).ToList();
            profiles.Add(activeProfile);
            
            Execute.OnUIThread(() =>
            {
                // Populate the UI collection
                Profiles.Clear();
                Profiles.AddRange(profiles.OrderBy(p => p.Name));

                SelectedProfile = activeProfile;
            });

            if (!activeProfile.IsActivated)
                Module.ChangeActiveProfile(activeProfile);
        }
    }
}
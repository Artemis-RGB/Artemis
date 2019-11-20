using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core.Models.Profile;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Services.Storage.Interfaces;
using Artemis.UI.Screens.Module.ProfileEditor.DisplayConditions;
using Artemis.UI.Screens.Module.ProfileEditor.ElementProperties;
using Artemis.UI.Screens.Module.ProfileEditor.LayerElements;
using Artemis.UI.Screens.Module.ProfileEditor.Layers;
using Artemis.UI.Screens.Module.ProfileEditor.Visualization;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor
{
    public class ProfileEditorViewModel : Conductor<ProfileEditorPanelViewModel>.Collection.AllActive
    {
        private readonly IProfileService _profileService;

        public ProfileEditorViewModel(ProfileModule module, ICollection<ProfileEditorPanelViewModel> viewModels, IProfileService profileService)
        {
            _profileService = profileService;

            DisplayName = "Profile editor";
            Module = module;

            DisplayConditionsViewModel = (DisplayConditionsViewModel) viewModels.First(vm => vm is DisplayConditionsViewModel);
            ElementPropertiesViewModel = (ElementPropertiesViewModel) viewModels.First(vm => vm is ElementPropertiesViewModel);
            LayerElementsViewModel = (LayerElementsViewModel) viewModels.First(vm => vm is LayerElementsViewModel);
            LayersViewModel = (LayersViewModel) viewModels.First(vm => vm is LayersViewModel);
            ProfileViewModel = (ProfileViewModel) viewModels.First(vm => vm is ProfileViewModel);

            Items.AddRange(viewModels);

            module.ActiveProfileChanged += ModuleOnActiveProfileChanged;
        }

        public Core.Plugins.Abstract.Module Module { get; }
        public DisplayConditionsViewModel DisplayConditionsViewModel { get; }
        public ElementPropertiesViewModel ElementPropertiesViewModel { get; }
        public LayerElementsViewModel LayerElementsViewModel { get; }
        public LayersViewModel LayersViewModel { get; }
        public ProfileViewModel ProfileViewModel { get; }

        public BindableCollection<Profile> Profiles { get; set; }
        public Profile SelectedProfile { get; set; }
        public bool CanDeleteActiveProfile => SelectedProfile != null;

        public async Task AddProfile()
        {
        }

        public async Task DeleteActiveProfile()
        {
        }

        private void ModuleOnActiveProfileChanged(object sender, EventArgs e)
        {
            SelectedProfile = ((ProfileModule) Module).ActiveProfile;
        }

        protected override void OnActivate()
        {
            Task.Run(() => LoadProfiles());
            base.OnActivate();
        }

        private void LoadProfiles()
        {
            var profiles = _profileService.GetProfiles((ProfileModule) Module);
            Profiles.Clear();
            Profiles.AddRange(profiles);

//            if (!profiles.Any())
//            {
//                var profile = new Profile(Module.PluginInfo, "Default");
//            }
        }
    }
}
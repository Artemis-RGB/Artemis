using System.ComponentModel;
using Artemis.Managers;
using Artemis.Modules.Effects.ProfilePreview;
using Artemis.ViewModels.Abstract;
using Artemis.ViewModels.Profiles;
using Ninject;
using Ninject.Parameters;

namespace Artemis.Modules.Effects.WindowsProfile
{
    // TODO: This effect is a hybrid between a regular effect and a game, may want to clean this up
    public sealed class WindowsProfileViewModel : EffectViewModel
    {
        public WindowsProfileViewModel(MainManager main, IKernel kernel, ProfilePreviewModel profilePreviewModel,
            WindowsProfileModel model) : base(main, model)
        {
            DisplayName = "Windows Profile";
            ProfilePreviewModel = profilePreviewModel;
            EffectSettings = ((WindowsProfileModel) EffectModel).Settings;

            IParameter[] args =
            {
                new ConstructorArgument("mainManager", main),
                new ConstructorArgument("gameModel", (WindowsProfileModel) EffectModel),
                new ConstructorArgument("lastProfile", ((WindowsProfileSettings) EffectSettings).LastProfile)
            };
            ProfileEditor = kernel.Get<ProfileEditorViewModel>(args);
            ProfilePreviewModel.Profile = ProfileEditor.SelectedProfile;
            ProfileEditor.PropertyChanged += ProfileUpdater;

            MainManager.EffectManager.EffectModels.Add(EffectModel);
        }

        public ProfileEditorViewModel ProfileEditor { get; set; }
        public ProfilePreviewModel ProfilePreviewModel { get; set; }

        private void ProfileUpdater(object sender, PropertyChangedEventArgs e)
        {
            if ((e.PropertyName != "SelectedProfile") && IsActive)
                return;
            EffectModel.Profile = ProfileEditor.SelectedProfile;
            ProfilePreviewModel.Profile = ProfileEditor.SelectedProfile;

            if ((e.PropertyName != "SelectedProfile") || !ProfileEditor.ProfileViewModel.Activated ||
                (ProfileEditor.ProfileViewModel.SelectedProfile == null))
                return;
            ((WindowsProfileSettings) EffectSettings).LastProfile = ProfileEditor.ProfileViewModel.SelectedProfile.Name;
            EffectSettings.Save();
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            ProfileEditor.Activate();
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
            ProfileEditor.Deactivate();
        }
    }
}
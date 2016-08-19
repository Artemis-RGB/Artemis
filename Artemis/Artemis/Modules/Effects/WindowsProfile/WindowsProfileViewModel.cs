using System.ComponentModel;
using Artemis.InjectionFactories;
using Artemis.Managers;
using Artemis.Modules.Effects.ProfilePreview;
using Artemis.ViewModels.Abstract;
using Artemis.ViewModels.Profiles;

namespace Artemis.Modules.Effects.WindowsProfile
{
    // TODO: This effect is a hybrid between a regular effect and a game, may want to clean this up
    public sealed class WindowsProfileViewModel : EffectViewModel
    {
        public WindowsProfileViewModel(MainManager main, IProfileEditorVmFactory pFactory,
            ProfilePreviewModel profilePreviewModel, WindowsProfileModel model) : base(main, model)
        {
            DisplayName = "Windows Profile";
            PFactory = pFactory;
            ProfilePreviewModel = profilePreviewModel;
            EffectSettings = ((WindowsProfileModel) EffectModel).Settings;
            ProfileEditor = PFactory.CreateProfileEditorVm(main, (WindowsProfileModel) EffectModel,
                ((WindowsProfileSettings) EffectSettings).LastProfile);
            ProfilePreviewModel.Profile = ProfileEditor.SelectedProfile;
            ProfileEditor.PropertyChanged += ProfileUpdater;
            MainManager.EffectManager.EffectModels.Add(EffectModel);
        }

        public ProfileEditorViewModel ProfileEditor { get; set; }

        public IProfileEditorVmFactory PFactory { get; set; }
        public ProfilePreviewModel ProfilePreviewModel { get; set; }

        private void ProfileUpdater(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "SelectedProfile" && IsActive)
                return;
            EffectModel.Profile = ProfileEditor.SelectedProfile;
            ProfilePreviewModel.Profile = ProfileEditor.SelectedProfile;

            if (e.PropertyName != "SelectedProfile" || !ProfileEditor.ProfileViewModel.Activated ||
                ProfileEditor.ProfileViewModel.SelectedProfile == null)
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
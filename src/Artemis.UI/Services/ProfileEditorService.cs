using System;
using Artemis.Core.Models.Profile;
using Artemis.Core.Services.Storage.Interfaces;
using Artemis.UI.Services.Interfaces;

namespace Artemis.UI.Services
{
    public class ProfileEditorService : IProfileEditorService
    {
        private readonly IProfileService _profileService;

        public ProfileEditorService(IProfileService profileService)
        {
            _profileService = profileService;
        }

        public Profile SelectedProfile { get; private set; }
        public ProfileElement SelectedProfileElement { get; private set; }

        public void ChangeSelectedProfile(Profile profile)
        {
            SelectedProfile = profile;
            OnSelectedProfileChanged();
        }

        public void UpdateSelectedProfile()
        {
            _profileService.UpdateProfile(SelectedProfile, false);
            OnSelectedProfileElementUpdated();
        }

        public void ChangeSelectedProfileElement(ProfileElement profileElement)
        {
            SelectedProfileElement = profileElement;
            OnSelectedProfileElementChanged();
        }

        public void UpdateSelectedProfileElement()
        {
            _profileService.UpdateProfile(SelectedProfile, true);
            OnSelectedProfileElementUpdated();
        }

        public event EventHandler SelectedProfileChanged;
        public event EventHandler SelectedProfileUpdated;
        public event EventHandler SelectedProfileElementChanged;
        public event EventHandler SelectedProfileElementUpdated;

        protected virtual void OnSelectedProfileElementUpdated()
        {
            SelectedProfileElementUpdated?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnSelectedProfileElementChanged()
        {
            SelectedProfileElementChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnSelectedProfileUpdated()
        {
            SelectedProfileUpdated?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnSelectedProfileChanged()
        {
            SelectedProfileChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
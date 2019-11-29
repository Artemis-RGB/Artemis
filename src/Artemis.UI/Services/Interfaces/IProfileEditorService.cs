using System;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.Abstract;

namespace Artemis.UI.Services.Interfaces
{
    public interface IProfileEditorService : IArtemisUIService
    {
        Profile SelectedProfile { get; }
        ProfileElement SelectedProfileElement { get; }

        void ChangeSelectedProfile(Profile profile);
        void UpdateSelectedProfile();
        void ChangeSelectedProfileElement(ProfileElement profileElement);
        void UpdateSelectedProfileElement();

        event EventHandler SelectedProfileChanged;
        event EventHandler SelectedProfileUpdated;
        event EventHandler SelectedProfileElementChanged;
        event EventHandler SelectedProfileElementUpdated;
    }
}
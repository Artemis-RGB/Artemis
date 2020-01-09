using System;
using Artemis.Core.Models.Profile;

namespace Artemis.UI.Services.Interfaces
{
    public interface IProfileEditorService : IArtemisUIService
    {
        Profile SelectedProfile { get; }
        ProfileElement SelectedProfileElement { get; }
        TimeSpan CurrentTime { get; set; }

        void ChangeSelectedProfile(Profile profile);
        void UpdateSelectedProfile();
        void ChangeSelectedProfileElement(ProfileElement profileElement);
        void UpdateSelectedProfileElement();

        /// <summary>
        ///     Occurs when a new profile is selected
        /// </summary>
        event EventHandler SelectedProfileChanged;

        /// <summary>
        ///     Occurs then the currently selected profile is updated
        /// </summary>
        event EventHandler SelectedProfileUpdated;

        /// <summary>
        ///     Occurs when a new profile element is selected
        /// </summary>
        event EventHandler SelectedProfileElementChanged;

        /// <summary>
        ///     Occurs when the currently selected profile element is updated
        /// </summary>
        event EventHandler SelectedProfileElementUpdated;

        /// <summary>
        ///     Occurs when the current editor time is changed
        /// </summary>
        event EventHandler CurrentTimeChanged;
    }
}
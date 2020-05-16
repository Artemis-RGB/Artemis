using System;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.LayerProperties;
using Artemis.Core.Models.Profile.LayerProperties.Attributes;
using Artemis.Core.Plugins.Abstract;
using Artemis.UI.Events;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Abstract;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Tree;

namespace Artemis.UI.Services.Interfaces
{
    public interface IProfileEditorService : IArtemisUIService
    {
        Profile SelectedProfile { get; }
        ProfileElement SelectedProfileElement { get; }
        TimeSpan CurrentTime { get; set; }

        LayerPropertyBaseViewModel CreateLayerPropertyViewModel(BaseLayerProperty baseLayerProperty, PropertyDescriptionAttribute propertyDescription);
        void ChangeSelectedProfile(Profile profile);
        void UpdateSelectedProfile();
        void ChangeSelectedProfileElement(ProfileElement profileElement);
        void UpdateSelectedProfileElement();
        void UpdateProfilePreview();
        void UndoUpdateProfile(ProfileModule module);
        void RedoUpdateProfile(ProfileModule module);
        void StopRegularRender();
        void ResumeRegularRender();

        /// <summary>
        ///     Occurs when a new profile is selected
        /// </summary>
        event EventHandler<ProfileElementEventArgs> ProfileSelected;

        /// <summary>
        ///     Occurs then the currently selected profile is updated
        /// </summary>
        event EventHandler<ProfileElementEventArgs> SelectedProfileUpdated;

        /// <summary>
        ///     Occurs when a new profile element is selected
        /// </summary>
        event EventHandler<ProfileElementEventArgs> ProfileElementSelected;

        /// <summary>
        ///     Occurs when the currently selected profile element is updated
        /// </summary>
        event EventHandler<ProfileElementEventArgs> SelectedProfileElementUpdated;

        /// <summary>
        ///     Occurs when the current editor time is changed
        /// </summary>
        event EventHandler CurrentTimeChanged;

        /// <summary>
        ///     Occurs when the profile preview has been updated
        /// </summary>
        event EventHandler ProfilePreviewUpdated;

        TreePropertyViewModel<T> CreateTreePropertyViewModel<T>();
    }
}
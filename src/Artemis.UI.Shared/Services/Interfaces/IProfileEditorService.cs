using System;
using System.Collections.Generic;
using Artemis.Core.Models.Profile;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.Models;
using Artemis.UI.Shared.Events;
using Artemis.UI.Shared.PropertyInput;
using Ninject;

namespace Artemis.UI.Shared.Services.Interfaces
{
    public interface IProfileEditorService : IArtemisSharedUIService
    {
        Profile SelectedProfile { get; }
        ProfileElement SelectedProfileElement { get; }
        TimeSpan CurrentTime { get; set; }
        int PixelsPerSecond { get; set; }
        IReadOnlyList<PropertyInputRegistration> RegisteredPropertyEditors { get; }
        IKernel Kernel { get; }

        void ChangeSelectedProfile(Profile profile);
        void UpdateSelectedProfile();
        void ChangeSelectedProfileElement(ProfileElement profileElement);
        void UpdateSelectedProfileElement();
        void UpdateProfilePreview();
        void UndoUpdateProfile(ProfileModule module);
        void RedoUpdateProfile(ProfileModule module);
        void StopRegularRender();
        void ResumeRegularRender();
        Module GetCurrentModule();

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
        ///     Occurs when the pixels per second (zoom level) is changed
        /// </summary>
        event EventHandler PixelsPerSecondChanged;

        /// <summary>
        ///     Occurs when the profile preview has been updated
        /// </summary>
        event EventHandler ProfilePreviewUpdated;

        /// <summary>
        ///     Registers a new property input view model used in the profile editor for the generic type defined in
        ///     <see cref="PropertyInputViewModel{T}" />
        ///     <para>Note: Registration will remove itself on plugin disable so you don't have to</para>
        /// </summary>
        /// <param name="pluginInfo"></param>
        /// <returns></returns>
        PropertyInputRegistration RegisterPropertyInput<T>(PluginInfo pluginInfo) where T : PropertyInputViewModel;

        void RemovePropertyInput(PropertyInputRegistration registration);
    }
}
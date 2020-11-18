using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Artemis.Core;
using Artemis.Core.Modules;

namespace Artemis.UI.Shared.Services
{
    /// <summary>
    ///     Provides access the the profile editor back-end logic
    /// </summary>
    public interface IProfileEditorService : IArtemisSharedUIService
    {
        /// <summary>
        ///     Gets the currently selected profile
        /// </summary>
        Profile SelectedProfile { get; }

        /// <summary>
        ///     Gets the currently selected profile element
        /// </summary>
        RenderProfileElement SelectedProfileElement { get; }

        /// <summary>
        ///     Gets the currently selected data binding property
        /// </summary>
        ILayerProperty SelectedDataBinding { get; }

        /// <summary>
        ///     Gets or sets the current time
        /// </summary>
        TimeSpan CurrentTime { get; set; }

        /// <summary>
        ///     Gets or sets the pixels per second (zoom level)
        /// </summary>
        int PixelsPerSecond { get; set; }

        /// <summary>
        ///     Gets a read-only collection of all registered property editors
        /// </summary>
        ReadOnlyCollection<PropertyInputRegistration> RegisteredPropertyEditors { get; }

        /// <summary>
        ///     Changes the selected profile
        /// </summary>
        /// <param name="profile">The profile to select</param>
        void ChangeSelectedProfile(Profile profile);

        /// <summary>
        ///     Updates the selected profile and saves it to persistent storage
        /// </summary>
        void UpdateSelectedProfile();

        /// <summary>
        ///     Changes the selected profile element
        /// </summary>
        /// <param name="profileElement">The profile element to select</param>
        void ChangeSelectedProfileElement(RenderProfileElement profileElement);

        /// <summary>
        ///     Updates the selected profile element and saves the profile it is contained in to persistent storage
        /// </summary>
        void UpdateSelectedProfileElement();

        /// <summary>
        ///     Changes the selected data binding property
        /// </summary>
        /// <param name="layerProperty">The data binding property to select</param>
        void ChangeSelectedDataBinding(ILayerProperty layerProperty);

        /// <summary>
        ///     Updates the profile preview, forcing UI-elements to re-render
        /// </summary>
        void UpdateProfilePreview();

        /// <summary>
        ///     Restores the profile to the last <see cref="UpdateSelectedProfile" /> call
        /// </summary>
        /// <returns><see langword="true" /> if undo was successful, otherwise <see langword="false" /></returns>
        bool UndoUpdateProfile();

        /// <summary>
        ///     Restores the profile to the last <see cref="UndoUpdateProfile" /> call
        /// </summary>
        /// <returns><see langword="true" /> if redo was successful, otherwise <see langword="false" /></returns>
        bool RedoUpdateProfile();

        /// <summary>
        ///     Gets the current module the profile editor is initialized for
        /// </summary>
        /// <returns>The current module the profile editor is initialized for</returns>
        ProfileModule GetCurrentModule();

        /// <summary>
        ///     Occurs when a new profile is selected
        /// </summary>
        event EventHandler<ProfileEventArgs> ProfileSelected;

        /// <summary>
        ///     Occurs then the currently selected profile is updated
        /// </summary>
        event EventHandler<ProfileEventArgs> SelectedProfileUpdated;

        /// <summary>
        ///     Occurs when a new profile element is selected
        /// </summary>
        event EventHandler<RenderProfileElementEventArgs> ProfileElementSelected;

        /// <summary>
        ///     Occurs when the currently selected profile element is updated
        /// </summary>
        event EventHandler<RenderProfileElementEventArgs> SelectedProfileElementUpdated;

        /// <summary>
        ///     Occurs when the currently selected data binding layer property is changed
        /// </summary>
        event EventHandler SelectedDataBindingChanged;

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
        /// <param name="plugin"></param>
        /// <returns></returns>
        PropertyInputRegistration RegisterPropertyInput<T>(Plugin plugin) where T : PropertyInputViewModel;

        /// <summary>
        ///     Registers a new property input view model used in the profile editor for the generic type defined in
        ///     <see cref="PropertyInputViewModel{T}" />
        ///     <para>Note: Registration will remove itself on plugin disable so you don't have to</para>
        /// </summary>
        /// <param name="viewModelType"></param>
        /// <param name="plugin"></param>
        /// <returns></returns>
        PropertyInputRegistration RegisterPropertyInput(Type viewModelType, Plugin plugin);

        /// <summary>
        ///     Removes the property input view model
        /// </summary>
        /// <param name="registration"></param>
        void RemovePropertyInput(PropertyInputRegistration registration);

        /// <summary>
        ///     Snaps the given time to the closest relevant element in the timeline, this can be the cursor, a keyframe or a
        ///     segment end.
        /// </summary>
        /// <param name="time"></param>
        /// <param name="tolerance">How close the time must be to snap</param>
        /// <param name="snapToSegments">Enable snapping to timeline segments</param>
        /// <param name="snapToCurrentTime">Enable snapping to the current time of the editor</param>
        /// <param name="snapTimes">An optional extra list of times to snap to</param>
        /// <returns></returns>
        TimeSpan SnapToTimeline(TimeSpan time, TimeSpan tolerance, bool snapToSegments, bool snapToCurrentTime, List<TimeSpan>? snapTimes = null);

        /// <summary>
        ///     If a matching registration is found, creates a new <see cref="PropertyInputViewModel{T}" /> supporting
        ///     <typeparamref name="T" />
        /// </summary>
        PropertyInputViewModel<T> CreatePropertyInputViewModel<T>(LayerProperty<T> layerProperty);
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Artemis.Core;

namespace Artemis.UI.Shared.Services
{
    /// <summary>
    ///     Provides access the the profile editor back-end logic
    /// </summary>
    public interface IProfileEditorService : IArtemisSharedUIService
    {
        /// <summary>
        ///     Gets the currently selected profile configuration
        ///     <para><see langword="null" /> if the editor is closed</para>
        /// </summary>
        ProfileConfiguration? SelectedProfileConfiguration { get; }

        /// <summary>
        ///     Gets the previous selected profile configuration
        /// </summary>
        ProfileConfiguration? PreviousSelectedProfileConfiguration { get; }

        /// <summary>
        ///     Gets the currently selected profile
        ///     <para>
        ///         <see langword="null" /> if the editor is closed, always equal to <see cref="SelectedProfileConfiguration" />.
        ///         <see cref="Profile" />
        ///     </para>
        /// </summary>
        Profile? SelectedProfile { get; }

        /// <summary>
        ///     Gets the currently selected profile element
        /// </summary>
        RenderProfileElement? SelectedProfileElement { get; }

        /// <summary>
        ///     Gets the currently selected data binding property
        /// </summary>
        ILayerProperty? SelectedDataBinding { get; }

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
        ///     Gets or sets a boolean indicating whether the editor is currently playing
        /// </summary>
        bool Playing { get; set; }

        /// <summary>
        ///     Gets or sets a boolean indicating whether editing should be suspended
        /// </summary>
        bool SuspendEditing { get; set; }

        /// <summary>
        ///     Changes the selected profile by its <see cref="ProfileConfiguration" />
        /// </summary>
        /// <param name="profileConfiguration">The profile configuration of the profile to select</param>
        void ChangeSelectedProfileConfiguration(ProfileConfiguration? profileConfiguration);

        /// <summary>
        ///     Saves the <see cref="Profile" /> of the selected <see cref="ProfileConfiguration" /> to persistent storage
        /// </summary>
        void SaveSelectedProfileConfiguration();

        /// <summary>
        ///     Changes the selected profile element
        /// </summary>
        /// <param name="profileElement">The profile element to select</param>
        void ChangeSelectedProfileElement(RenderProfileElement? profileElement);

        /// <summary>
        ///     Saves the currently selected <see cref="ProfileElement" /> to persistent storage
        /// </summary>
        void SaveSelectedProfileElement();

        /// <summary>
        ///     Changes the selected data binding property
        /// </summary>
        /// <param name="layerProperty">The data binding property to select</param>
        void ChangeSelectedDataBinding(ILayerProperty? layerProperty);

        /// <summary>
        ///     Updates the profile preview, forcing UI-elements to re-render
        /// </summary>
        void UpdateProfilePreview();

        /// <summary>
        ///     Restores the profile to the last <see cref="SaveSelectedProfileConfiguration" /> call
        /// </summary>
        /// <returns><see langword="true" /> if undo was successful, otherwise <see langword="false" /></returns>
        bool UndoSaveProfile();

        /// <summary>
        ///     Restores the profile to the last <see cref="UndoSaveProfile" /> call
        /// </summary>
        /// <returns><see langword="true" /> if redo was successful, otherwise <see langword="false" /></returns>
        bool RedoSaveProfile();

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
        ///     Determines if there is a matching registration for the provided layer property
        /// </summary>
        /// <param name="layerProperty">The layer property to try to find a view model for</param>
        bool CanCreatePropertyInputViewModel(ILayerProperty layerProperty);

        /// <summary>
        ///     If a matching registration is found, creates a new <see cref="PropertyInputViewModel{T}" /> supporting
        ///     <typeparamref name="T" />
        /// </summary>
        PropertyInputViewModel<T>? CreatePropertyInputViewModel<T>(LayerProperty<T> layerProperty);

        /// <summary>
        ///     Duplicates the provided profile element placing it in the same folder and one position higher
        /// </summary>
        /// <param name="profileElement">The profile element to duplicate</param>
        /// <returns>The duplicated profile element</returns>
        ProfileElement? DuplicateProfileElement(ProfileElement profileElement);

        /// <summary>
        ///     Copies the provided profile element onto the clipboard
        /// </summary>
        /// <param name="profileElement">The profile element to copy</param>
        void CopyProfileElement(ProfileElement profileElement);

        /// <summary>
        ///     Pastes a render profile element from the clipboard into the target folder
        /// </summary>
        /// <param name="target">The folder to paste the render element in to</param>
        /// <param name="position">The position at which to paste the element</param>
        /// <returns>The pasted render element</returns>
        ProfileElement? PasteProfileElement(Folder target, int position);

        /// <summary>
        ///     Gets a boolean indicating whether a profile element is on the clipboard
        /// </summary>
        bool GetCanPasteProfileElement();

        /// <summary>
        ///     Determines all LEDs on the current active surface contained in the specified rectangle
        /// </summary>
        /// <returns>A list containing all the LEDs that are in the provided rect</returns>
        List<ArtemisLed> GetLedsInRectangle(Rect rect);

        /// <summary>
        ///     Occurs when a new profile is selected
        /// </summary>
        event EventHandler<ProfileConfigurationEventArgs> SelectedProfileChanged;

        /// <summary>
        ///     Occurs then the currently selected profile is updated
        /// </summary>
        event EventHandler<ProfileConfigurationEventArgs> SelectedProfileSaved;

        /// <summary>
        ///     Occurs when a new profile element is selected
        /// </summary>
        event EventHandler<RenderProfileElementEventArgs> SelectedProfileElementChanged;

        /// <summary>
        ///     Occurs when the currently selected profile element is updated
        /// </summary>
        event EventHandler<RenderProfileElementEventArgs> SelectedProfileElementSaved;

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
    }
}
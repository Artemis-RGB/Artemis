using System;
using System.Collections.Generic;
using Artemis.Core;
using Artemis.Core.Modules;
using Ninject;

namespace Artemis.UI.Shared.Services
{
    public interface IProfileEditorService : IArtemisSharedUIService
    {
        Profile SelectedProfile { get; }
        RenderProfileElement SelectedProfileElement { get; }
        BaseLayerProperty SelectedDataBinding { get; }
        TimeSpan CurrentTime { get; set; }
        int PixelsPerSecond { get; set; }
        IReadOnlyList<PropertyInputRegistration> RegisteredPropertyEditors { get; }
        IKernel Kernel { get; }
        void ChangeSelectedProfile(Profile profile);
        void UpdateSelectedProfile();
        void ChangeSelectedProfileElement(RenderProfileElement profileElement);
        void UpdateSelectedProfileElement();
        void ChangeSelectedDataBinding(BaseLayerProperty layerProperty);
        void UpdateProfilePreview();
        bool UndoUpdateProfile();
        bool RedoUpdateProfile();
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
        /// <param name="pluginInfo"></param>
        /// <returns></returns>
        PropertyInputRegistration RegisterPropertyInput<T>(PluginInfo pluginInfo) where T : PropertyInputViewModel;

        void RemovePropertyInput(PropertyInputRegistration registration);

        /// <summary>
        ///     Snaps the given time to the closest relevant element in the timeline, this can be the cursor, a keyframe or a
        ///     segment end.
        /// </summary>
        /// <param name="time"></param>
        /// <param name="tolerance">How close the time must be to snap</param>
        /// <param name="snapToSegments">Enable snapping to timeline segments</param>
        /// <param name="snapToCurrentTime">Enable snapping to the current time of the editor</param>
        /// <param name="snapToKeyframes">Enable snapping to visible keyframes</param>
        /// <param name="excludedKeyframe">A keyframe to exclude during keyframe snapping</param>
        /// <returns></returns>
        TimeSpan SnapToTimeline(TimeSpan time, TimeSpan tolerance, bool snapToSegments, bool snapToCurrentTime, bool snapToKeyframes, BaseLayerPropertyKeyframe excludedKeyframe = null);
    }
}
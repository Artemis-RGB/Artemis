using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Artemis.Core;

namespace Artemis.UI.Shared.Services.ProfileEditor;

/// <summary>
///     Provides access the the profile editor back-end logic.
/// </summary>
public interface IProfileEditorService : IArtemisSharedUIService
{
    /// <summary>
    ///     Gets an observable of the currently selected profile configuration.
    /// </summary>
    IObservable<ProfileConfiguration?> ProfileConfiguration { get; }

    /// <summary>
    ///     Gets an observable of the currently selected profile element.
    /// </summary>
    IObservable<RenderProfileElement?> ProfileElement { get; }

    /// <summary>
    ///     Gets an observable of the currently selected layer property.
    /// </summary>
    IObservable<ILayerProperty?> LayerProperty { get; }

    /// <summary>
    ///     Gets an observable of the current editor history.
    /// </summary>
    IObservable<ProfileEditorHistory?> History { get; }

    /// <summary>
    ///     Gets an observable of the profile preview playback time.
    /// </summary>
    IObservable<TimeSpan> Time { get; }

    /// <summary>
    ///     Gets an observable of the profile preview playing state.
    /// </summary>
    IObservable<bool> Playing { get; }

    /// <summary>
    ///     Gets an observable of the zoom level.
    /// </summary>
    IObservable<int> PixelsPerSecond { get; }

    /// <summary>
    ///     Gets an observable of the suspended state.
    /// </summary>
    IObservable<bool> SuspendedEditing { get; }

    /// <summary>
    ///     Gets an observable of the suspended keybindings state.
    /// </summary>
    IObservable<bool> SuspendedKeybindings { get; }

    /// <summary>
    ///     Gets an observable of the suspended keybindings state.
    /// </summary>
    IObservable<ProfileEditorFocusMode> FocusMode { get; }

    /// <summary>
    ///     Gets an observable read only collection of selected keyframes.
    /// </summary>
    ReadOnlyObservableCollection<ILayerPropertyKeyframe> SelectedKeyframes { get; }

    /// <summary>
    ///     Changes the selected profile by its <see cref="Core.ProfileConfiguration" />.
    /// </summary>
    /// <param name="profileConfiguration">The profile configuration of the profile to select.</param>
    void ChangeCurrentProfileConfiguration(ProfileConfiguration? profileConfiguration);

    /// <summary>
    ///     Changes the selected profile element.
    /// </summary>
    /// <param name="renderProfileElement">The profile element to select.</param>
    void ChangeCurrentProfileElement(RenderProfileElement? renderProfileElement);

    /// <summary>
    ///     Change the selected layer property.
    /// </summary>
    /// <param name="layerProperty">The layer property to select.</param>
    void ChangeCurrentLayerProperty(ILayerProperty? layerProperty);

    /// <summary>
    ///     Changes the current profile preview playback time.
    /// </summary>
    /// <param name="time">The new time.</param>
    void ChangeTime(TimeSpan time);

    /// <summary>
    ///     Changes the current pixels per second
    /// </summary>
    /// <param name="pixelsPerSecond">The new pixels per second.</param>
    void ChangePixelsPerSecond(int pixelsPerSecond);

    /// <summary>
    ///     Changes the current suspended state.
    /// </summary>
    /// <param name="suspend">The new suspended state.</param>
    void ChangeSuspendedEditing(bool suspend);

    /// <summary>
    ///     Changes the current suspended keybindings state.
    /// </summary>
    /// <param name="suspend">The new suspended state.</param>
    void ChangeSuspendedKeybindings(bool suspend);

    /// <summary>
    ///     Changes the current focus mode.
    /// </summary>
    /// <param name="focusMode">The new focus mode.</param>
    void ChangeFocusMode(ProfileEditorFocusMode focusMode);

    /// <summary>
    ///     Selects the provided keyframe.
    /// </summary>
    /// <param name="keyframe">The keyframe to select.</param>
    /// <param name="expand">
    ///     If <see langword="true" /> expands the current selection; otherwise replaces it with only the
    ///     provided <paramref name="keyframe" />.
    /// </param>
    /// <param name="toggle">
    ///     If <see langword="true" /> toggles the selection and only for the provided
    ///     <paramref name="keyframe" />.
    /// </param>
    void SelectKeyframe(ILayerPropertyKeyframe? keyframe, bool expand, bool toggle);

    /// <summary>
    ///     Selects the provided keyframes.
    /// </summary>
    /// <param name="keyframes">The keyframes to select.</param>
    /// <param name="expand">
    ///     If <see langword="true" /> expands the current selection; otherwise replaces it with only the
    ///     provided <paramref name="keyframes" />.
    /// </param>
    void SelectKeyframes(IEnumerable<ILayerPropertyKeyframe> keyframes, bool expand);

    /// <summary>
    ///     Snaps the given time to the closest relevant element in the timeline, this can be the cursor, a keyframe or a
    ///     segment end.
    /// </summary>
    /// <param name="time">The time to snap.</param>
    /// <param name="tolerance">How close the time must be to snap.</param>
    /// <param name="snapToSegments">Enable snapping to timeline segments.</param>
    /// <param name="snapToCurrentTime">Enable snapping to the current time of the editor.</param>
    /// <param name="snapTimes">An optional extra list of times to snap to.</param>
    /// <returns>The snapped time.</returns>
    TimeSpan SnapToTimeline(TimeSpan time, TimeSpan tolerance, bool snapToSegments, bool snapToCurrentTime, List<TimeSpan>? snapTimes = null);

    /// <summary>
    ///     Rounds the given time to something appropriate for the current zoom level.
    /// </summary>
    /// <param name="time">The time to round</param>
    /// <returns>The rounded time.</returns>
    TimeSpan RoundTime(TimeSpan time);

    /// <summary>
    ///     Creates a new folder as a sibling or child of the given target.
    /// </summary>
    /// <param name="target">The target, if this is a layer the new layer will become a sibling, otherwise a child.</param>
    /// <returns>The resulting folder.</returns>
    Folder CreateAndAddFolder(ProfileElement target);

    /// <summary>
    ///     Creates a new layer with the default brush and all current LEDs as a sibling or child of the given target.
    /// </summary>
    /// <param name="target">The target, if this is a layer the new layer will become a sibling, otherwise a child.</param>
    /// <returns>The resulting layer.</returns>
    Layer CreateAndAddLayer(ProfileElement target);

    /// <summary>
    ///     Executes the provided command and adds it to the history.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    void ExecuteCommand(IProfileEditorCommand command);

    /// <summary>
    ///     Creates a new command scope which can be used to group undo/redo actions of multiple commands.
    /// </summary>
    /// <param name="name">The name of the command scope.</param>
    /// <returns>The command scope that will group any commands until disposed.</returns>
    ProfileEditorCommandScope CreateCommandScope(string name);

    /// <summary>
    ///     Saves the current profile.
    /// </summary>
    void SaveProfile();

    /// <summary>
    ///     Asynchronously saves the current profile.
    /// </summary>
    /// <returns>A task representing the save action.</returns>
    Task SaveProfileAsync();

    /// <summary>
    ///     Resumes profile preview playback.
    /// </summary>
    void Play();

    /// <summary>
    ///     Pauses profile preview playback.
    /// </summary>
    void Pause();
}
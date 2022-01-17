using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.UI.Shared.Services.Interfaces;

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
    ///     Executes the provided command and adds it to the history.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    void ExecuteCommand(IProfileEditorCommand command);

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
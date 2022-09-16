namespace Artemis.Core;

/// <summary>
///     Represents a mode for render elements when toggling off the event when using <see cref="EventTriggerMode.Toggle" />
///     .
/// </summary>
public enum EventToggleOffMode
{
    /// <summary>
    ///     When the event toggles the condition off, finish the the current run of the main timeline
    /// </summary>
    Finish,

    /// <summary>
    ///     When the event toggles the condition off, skip to the end segment of the timeline
    /// </summary>
    SkipToEnd
}
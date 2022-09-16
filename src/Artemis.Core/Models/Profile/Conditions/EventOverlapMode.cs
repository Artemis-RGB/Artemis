namespace Artemis.Core;

/// <summary>
///     Represents a mode for render elements to configure the behaviour of events that overlap i.e. trigger again before
///     the timeline finishes.
/// </summary>
public enum EventOverlapMode
{
    /// <summary>
    ///     Stop the current run and restart the timeline
    /// </summary>
    Restart,

    /// <summary>
    ///     Play another copy of the timeline on top of the current run
    /// </summary>
    Copy,

    /// <summary>
    ///     Ignore subsequent event fires until the timeline finishes
    /// </summary>
    Ignore
}
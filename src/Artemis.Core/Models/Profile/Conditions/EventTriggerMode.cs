namespace Artemis.Core;

/// <summary>
///     Represents a mode for render elements to start their timeline when display conditions events are fired.
/// </summary>
public enum EventTriggerMode
{
    /// <summary>
    ///     Play the timeline once.
    /// </summary>
    Play,

    /// <summary>
    ///     Toggle repeating the timeline.
    /// </summary>
    Toggle
}
namespace Artemis.Core.Services;

/// <summary>
///     Represents the status of a keyboards special toggles
/// </summary>
public readonly struct KeyboardToggleStatus
{
    /// <summary>
    ///     Creates a new <see cref="KeyboardToggleStatus" />
    /// </summary>
    /// <param name="numLock">A boolean indicating whether num lock is on</param>
    /// <param name="capsLock">A boolean indicating whether caps lock is on</param>
    /// <param name="scrollLock">A boolean indicating whether scroll lock is on</param>
    public KeyboardToggleStatus(bool numLock, bool capsLock, bool scrollLock)
    {
        NumLock = numLock;
        CapsLock = capsLock;
        ScrollLock = scrollLock;
    }

    /// <summary>
    ///     Gets a boolean indicating whether num lock is on
    /// </summary>
    public bool NumLock { get; }

    /// <summary>
    ///     Gets a boolean indicating whether caps lock is on
    /// </summary>
    public bool CapsLock { get; }

    /// <summary>
    ///     Gets a boolean indicating whether scroll lock is on
    /// </summary>
    public bool ScrollLock { get; }
}
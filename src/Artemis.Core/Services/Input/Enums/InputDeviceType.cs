namespace Artemis.Core.Services;

/// <summary>
///     Represents a device that provides input to the <see cref="IInputService" />
/// </summary>
public enum InputDeviceType
{
    /// <summary>
    ///     None
    /// </summary>
    None,

    /// <summary>
    ///     A keyboard
    /// </summary>
    Keyboard,

    /// <summary>
    ///     A mouse
    /// </summary>
    Mouse
}
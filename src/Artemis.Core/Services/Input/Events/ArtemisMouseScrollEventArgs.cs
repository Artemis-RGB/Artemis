using System;

namespace Artemis.Core.Services;

/// <summary>
///     Contains data for mouse scroll events
/// </summary>
public class ArtemisMouseScrollEventArgs : EventArgs
{
    /// <summary>
    /// </summary>
    /// <param name="device">The device that triggered the event</param>
    /// <param name="direction">The direction in which was scrolled</param>
    /// <param name="delta">The scroll delta (can positive or negative)</param>
    internal ArtemisMouseScrollEventArgs(ArtemisDevice? device, MouseScrollDirection direction, int delta)
    {
        Device = device;
        Direction = direction;
        Delta = delta;
    }

    /// <summary>
    ///     Gets the device that triggered the event
    /// </summary>
    public ArtemisDevice? Device { get; }

    /// <summary>
    ///     Gets the direction in which was scrolled
    /// </summary>
    public MouseScrollDirection Direction { get; }

    /// <summary>
    ///     Gets the scroll delta (can positive or negative)
    /// </summary>
    public int Delta { get; }

    /// <summary>
    ///     Gets a boolean indicating whether the mouse scrolled up
    /// </summary>
    public bool IsScrollingUp => Direction == MouseScrollDirection.Vertical && Delta > 0;

    /// <summary>
    ///     Gets a boolean indicating whether the mouse scrolled down
    /// </summary>
    public bool IsScrollingDown => Direction == MouseScrollDirection.Vertical && Delta < 0;

    /// <summary>
    ///     Gets a boolean indicating whether the mouse scrolled right
    /// </summary>
    public bool IsScrollingRight => Direction == MouseScrollDirection.Horizontal && Delta > 0;

    /// <summary>
    ///     Gets a boolean indicating whether the mouse scrolled left
    /// </summary>
    public bool IsScrollingLeft => Direction == MouseScrollDirection.Horizontal && Delta < 0;
}
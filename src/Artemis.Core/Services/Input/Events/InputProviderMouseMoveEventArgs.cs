using System;

namespace Artemis.Core.Services
{
    /// <summary>
    ///     Contains data for input provider mouse movement events
    /// </summary>
    public class InputProviderMouseMoveEventArgs : EventArgs
    {
        /// <summary>
        /// </summary>
        /// <param name="device">The device that triggered the event</param>
        /// <param name="cursorX">The X position of the mouse cursor (not necessarily tied to the specific device)</param>
        /// <param name="cursorY">The Y position of the mouse cursor (not necessarily tied to the specific device)</param>
        /// <param name="deltaX">The movement delta in the horizontal direction</param>
        /// <param name="deltaY">The movement delta in the vertical direction</param>
        public InputProviderMouseMoveEventArgs(ArtemisDevice? device, int cursorX, int cursorY, int deltaX, int deltaY)
        {
            Device = device;
            CursorX = cursorX;
            CursorY = cursorY;
            DeltaX = deltaX;
            DeltaY = deltaY;
        }

        /// <summary>
        ///     Gets the device that triggered the event
        /// </summary>
        public ArtemisDevice? Device { get; }

        /// <summary>
        /// Gets the X position of the mouse cursor (not necessarily tied to the specific device)
        /// </summary>
        public int CursorX { get; }

        /// <summary>
        /// Gets the Y position of the mouse cursor (not necessarily tied to the specific device)
        /// </summary>
        public int CursorY { get; }

        /// <summary>
        ///     Gets the movement delta in the horizontal direction
        /// </summary>
        public int DeltaX { get; }

        /// <summary>
        ///     Gets the movement delta in the vertical direction
        /// </summary>
        public int DeltaY { get; }
    }
}
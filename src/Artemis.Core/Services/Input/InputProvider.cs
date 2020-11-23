using System;

namespace Artemis.Core.Services
{
    /// <summary>
    ///     Represents an interface for an input provider that provides and implementation for sending and receiving device
    ///     input
    /// </summary>
    public abstract class InputProvider : IDisposable
    {
        /// <summary>
        ///     Occurs when the input provided has received keyboard data
        /// </summary>
        public event EventHandler<InputProviderKeyboardEventArgs>? KeyboardDataReceived;

        /// <summary>
        ///     Occurs when the input provided has received mouse button data
        /// </summary>
        public event EventHandler<InputProviderMouseButtonEventArgs>? MouseButtonDataReceived;

        /// <summary>
        ///     Occurs when the input provided has received mouse scroll data
        /// </summary>
        public event EventHandler<InputProviderMouseScrollEventArgs>? MouseScrollDataReceived;

        /// <summary>
        ///     Occurs when the input provided has received mouse move data
        /// </summary>
        public event EventHandler<InputProviderMouseMoveEventArgs>? MouseMoveDataReceived;

        /// <summary>
        ///     Occurs when the input provided received a device identifier
        /// </summary>
        public event EventHandler<InputProviderIdentifierEventArgs>? IdentifierReceived;

        /// <summary>
        ///     Invokes the <see cref="KeyboardDataReceived" /> event which the <see cref="IInputService" /> listens to as long as
        ///     this provider is registered
        /// </summary>
        /// <param name="device">The device that triggered the event</param>
        /// <param name="key">The key that triggered the event</param>
        /// <param name="isDown">Whether the key is pressed down</param>
        protected virtual void OnKeyboardDataReceived(ArtemisDevice? device, KeyboardKey key, bool isDown)
        {
            KeyboardDataReceived?.Invoke(this, new InputProviderKeyboardEventArgs(device, key, isDown));
        }

        /// <summary>
        ///     Invokes the <see cref="MouseButtonDataReceived" /> event which the <see cref="IInputService" /> listens to as long
        ///     as this provider is registered
        /// </summary>
        /// <param name="device">The device that triggered the event</param>
        /// <param name="button">The button that triggered the event</param>
        /// <param name="isDown">Whether the button is pressed down</param>
        protected virtual void OnMouseButtonDataReceived(ArtemisDevice? device, MouseButton button, bool isDown)
        {
            MouseButtonDataReceived?.Invoke(this, new InputProviderMouseButtonEventArgs(device, button, isDown));
        }

        /// <summary>
        ///     Invokes the <see cref="MouseScrollDataReceived" /> event which the <see cref="IInputService" /> listens to as long
        ///     as this provider is registered
        /// </summary>
        /// <param name="device">The device that triggered the event</param>
        /// <param name="direction">The direction in which was scrolled</param>
        /// <param name="delta">The scroll delta (can positive or negative)</param>
        protected virtual void OnMouseScrollDataReceived(ArtemisDevice? device, MouseScrollDirection direction, int delta)
        {
            MouseScrollDataReceived?.Invoke(this, new InputProviderMouseScrollEventArgs(device, direction, delta));
        }

        /// <summary>
        ///     Invokes the <see cref="MouseMoveDataReceived" /> event which the <see cref="IInputService" /> listens to as long as
        ///     this provider is registered
        /// </summary>
        /// <param name="device">The device that triggered the event</param>
        /// <param name="cursorX">The X position of the mouse cursor (not necessarily tied to the specific device)</param>
        /// <param name="cursorY">The Y position of the mouse cursor (not necessarily tied to the specific device)</param>
        /// <param name="deltaX">The movement delta in the horizontal direction</param>
        /// <param name="deltaY">The movement delta in the vertical direction</param>
        protected virtual void OnMouseMoveDataReceived(ArtemisDevice? device, int cursorX, int cursorY, int deltaX, int deltaY)
        {
            MouseMoveDataReceived?.Invoke(this, new InputProviderMouseMoveEventArgs(device, cursorX, cursorY, deltaX, deltaY));
        }

        /// <summary>
        ///     Invokes the <see cref="IdentifierReceived" /> event which the <see cref="IInputService" /> listens to as long as
        ///     this provider is registered
        ///     <para>Except for on mouse movement, call this whenever you have an identifier ready for the core to process</para>
        /// </summary>
        /// <param name="identifier">A value that can be used to identify this device</param>
        /// <param name="deviceType">The type of device this identifier belongs to</param>
        protected virtual void OnIdentifierReceived(object identifier, InputDeviceType deviceType)
        {
            IdentifierReceived?.Invoke(this, new InputProviderIdentifierEventArgs(identifier, deviceType));
        }

        #region IDisposable

        /// <summary>
        ///     Releases the unmanaged resources used by the object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        ///     <see langword="true" /> to release both managed and unmanaged resources;
        ///     <see langword="false" /> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
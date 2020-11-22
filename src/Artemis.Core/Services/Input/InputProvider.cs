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
        public event EventHandler<object>? IdentifierReceived;

        /// <summary>
        ///     Invokes the <see cref="KeyboardDataReceived" /> event which the <see cref="IInputService" /> listens to as long as
        ///     this provider is registered
        /// </summary>
        protected virtual void OnKeyboardDataReceived(InputProviderKeyboardEventArgs e)
        {
            KeyboardDataReceived?.Invoke(this, e);
        }

        /// <summary>
        ///     Invokes the <see cref="MouseButtonDataReceived" /> event which the <see cref="IInputService" /> listens to as long
        ///     as this provider is registered
        /// </summary>
        protected virtual void OnMouseButtonDataReceived(InputProviderMouseButtonEventArgs e)
        {
            MouseButtonDataReceived?.Invoke(this, e);
        }

        /// <summary>
        ///     Invokes the <see cref="MouseScrollDataReceived" /> event which the <see cref="IInputService" /> listens to as long
        ///     as this provider is registered
        /// </summary>
        protected virtual void OnMouseScrollDataReceived(InputProviderMouseScrollEventArgs e)
        {
            MouseScrollDataReceived?.Invoke(this, e);
        }

        /// <summary>
        ///     Invokes the <see cref="MouseMoveDataReceived" /> event which the <see cref="IInputService" /> listens to as long as
        ///     this provider is registered
        /// </summary>
        protected virtual void OnMouseMoveDataReceived(InputProviderMouseMoveEventArgs e)
        {
            MouseMoveDataReceived?.Invoke(this, e);
        }

        /// <summary>
        ///     Invokes the <see cref="IdentifierReceived" /> event which the <see cref="IInputService" /> listens to as long as
        ///     this provider is registered
        /// </summary>
        protected virtual void OnIdentifierReceived(object identifier)
        {
            IdentifierReceived?.Invoke(this, identifier);
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
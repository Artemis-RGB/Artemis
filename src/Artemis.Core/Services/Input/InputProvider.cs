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
        ///     Invokes the <see cref="KeyboardDataReceived" /> event
        /// </summary>
        protected virtual void OnKeyboardDataReceived(InputProviderKeyboardEventArgs e)
        {
            KeyboardDataReceived?.Invoke(this, e);
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
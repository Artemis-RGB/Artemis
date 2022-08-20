using System;
using System.Threading.Tasks;
using Artemis.Core.LayerBrushes;

namespace Artemis.UI.Shared.LayerBrushes
{
    /// <summary>
    ///     Represents a view model for a brush configuration window
    /// </summary>
    public abstract class BrushConfigurationViewModel : ValidatableViewModelBase
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="BrushConfigurationViewModel" /> class
        /// </summary>
        /// <param name="layerBrush"></param>
        protected BrushConfigurationViewModel(BaseLayerBrush layerBrush)
        {
            LayerBrush = layerBrush;
        }

        /// <summary>
        ///     Gets the layer brush this view model is associated with
        /// </summary>
        public BaseLayerBrush LayerBrush { get; }

        /// <summary>
        ///     Closes the dialog
        /// </summary>
        public void RequestClose()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///     Called when the window wants to close, returning <see langword="false" /> will cause the window to stay open.
        /// </summary>
        /// <returns><see langword="true" /> if the window may close; otherwise <see langword="false" />.</returns>
        public virtual bool CanClose()
        {
            return true;
        }

        /// <summary>
        ///     Called when the window wants to close, returning <see langword="false" /> will cause the window to stay open.
        /// </summary>
        /// <returns>A task <see langword="true" /> if the window may close; otherwise <see langword="false" />.</returns>
        public virtual Task<bool> CanCloseAsync()
        {
            return Task.FromResult(true);
        }

        /// <summary>
        ///     Occurs when a close was requested
        /// </summary>
        public event EventHandler? CloseRequested;
    }
}
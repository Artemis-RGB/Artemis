using System;

namespace Artemis.Core
{
    /// <summary>
    ///     Provides data for layer property events.
    /// </summary>
    public class LayerPropertyEventArgs : EventArgs
    {
        internal LayerPropertyEventArgs(ILayerProperty layerProperty)
        {
            LayerProperty = layerProperty;
        }

        /// <summary>
        ///     Gets the layer property this event is related to
        /// </summary>
        public ILayerProperty LayerProperty { get; }
    }
}
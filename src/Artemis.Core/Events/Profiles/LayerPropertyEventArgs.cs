using System;

namespace Artemis.Core
{
    /// <summary>
    ///     Provides strongly typed data for layer property events of type <typeparamref name="T" />.
    /// </summary>
    public class LayerPropertyEventArgs<T> : EventArgs
    {
        internal LayerPropertyEventArgs(LayerProperty<T> layerProperty)
        {
            LayerProperty = layerProperty;
        }

        /// <summary>
        ///     Gets the layer property this event is related to
        /// </summary>
        public LayerProperty<T> LayerProperty { get; }
    }

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
using System;

namespace Artemis.Core
{
    public class LayerPropertyEventArgs : EventArgs
    {
        public LayerPropertyEventArgs(BaseLayerProperty layerProperty)
        {
            LayerProperty = layerProperty;
        }

        public BaseLayerProperty LayerProperty { get; }
    }
}
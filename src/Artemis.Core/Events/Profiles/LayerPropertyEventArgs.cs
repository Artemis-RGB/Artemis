using System;

namespace Artemis.Core
{
    public class LayerPropertyEventArgs<T> : EventArgs
    {
        public LayerPropertyEventArgs(LayerProperty<T> layerProperty)
        {
            LayerProperty = layerProperty;
        }

        public LayerProperty<T> LayerProperty { get; }
    }  
    
    public class LayerPropertyEventArgs : EventArgs
    {
        public LayerPropertyEventArgs(ILayerProperty layerProperty)
        {
            LayerProperty = layerProperty;
        }

        public ILayerProperty LayerProperty { get; }
    }
}
using System;
using Artemis.Core.Models.Profile.LayerProperties;

namespace Artemis.Core.Events
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
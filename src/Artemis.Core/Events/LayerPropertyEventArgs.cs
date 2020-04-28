using System;
using Artemis.Core.Models.Profile.LayerProperties;

namespace Artemis.Core.Events
{
    public class LayerPropertyEventArgs : EventArgs
    {
        public LayerPropertyEventArgs(LayerProperty layerProperty)
        {
            LayerProperty = layerProperty;
        }

        public LayerProperty LayerProperty { get; }
    }
}
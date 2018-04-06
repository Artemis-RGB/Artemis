using System;
using System.Collections.Generic;
using Artemis.Core.Plugins.Interfaces;
using Artemis.Core.ProfileElements.Interfaces;
using Artemis.Core.Services.Interfaces;
using Artemis.Storage.Entities;
using RGB.NET.Core;

namespace Artemis.Core.ProfileElements
{
    public class Layer : IProfileElement
    {
        public Layer()
        {
            Children = new List<IProfileElement>();
        }

        public ILayerType LayerType { get; private set; }

        public List<IProfileElement> Children { get; set; }
        public int Order { get; set; }
        public string Name { get; set; }

        public void Update()
        {
            if (LayerType == null)
                return;

            lock (LayerType)
            {
                LayerType.Update(this);
            }
        }

        public void Render(IRGBDevice rgbDevice)
        {
            if (LayerType == null)
                return;

            lock (LayerType)
            {
                LayerType.Render(this, rgbDevice);
            }
        }

        public static Layer FromLayerEntity(LayerEntity layerEntity, IPluginService pluginService)
        {
            var layer = new Layer
            {
                Name = layerEntity.Name,
                Order = layerEntity.Order,
                LayerType = pluginService.GetLayerTypeByGuid(Guid.Parse(layerEntity.Guid))
            };
            
            return layer;
        }

        public void UpdateLayerType(ILayerType layerType)
        {
            if (LayerType != null)
            {
                lock (LayerType)
                {
                    LayerType.Dispose();
                }
            }

            LayerType = layerType;
        }
    }
}
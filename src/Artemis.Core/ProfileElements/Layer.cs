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
        public Layer(Profile profile)
        {
            Profile = profile;
            Children = new List<IProfileElement>();
        }

        public Profile Profile { get; }
        public ILayerType LayerType { get; private set; }
        public ILayerTypeConfiguration LayerTypeConfiguration { get; set; }
        public List<IProfileElement> Children { get; set; }
        public int Order { get; set; }
        public string Name { get; set; }

        public void Update(double deltaTime)
        {
            if (LayerType == null)
                return;

            lock (LayerType)
            {
                LayerType.Update(this);
            }
        }

        public void Render(double deltaTime, RGBSurface surface)
        {
            if (LayerType == null)
                return;

            lock (LayerType)
            {
                LayerType.Render(this, surface);
            }
        }

        public static Layer FromLayerEntity(Profile profile, LayerEntity layerEntity, IPluginService pluginService)
        {
            var layer = new Layer(profile)
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
                lock (LayerType)
                {
                    LayerType.Dispose();
                }

            LayerType = layerType;
        }

        public override string ToString()
        {
            return $"{nameof(Profile)}: {Profile}, {nameof(Order)}: {Order}, {nameof(Name)}: {Name}";
        }
    }
}
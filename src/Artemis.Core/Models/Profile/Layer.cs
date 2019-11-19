using System;
using System.Collections.Generic;
using System.Drawing;
using Artemis.Core.Models.Profile.Abstract;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.Interfaces;
using Artemis.Core.Services.Interfaces;
using Artemis.Storage.Entities;

namespace Artemis.Core.Models.Profile
{
    public class Layer : ProfileElement
    {
        internal Layer(Profile profile, Folder folder, string name)
        {
            LayerEntity = new LayerEntity();
            Guid = System.Guid.NewGuid().ToString();

            Profile = profile;
            ParentFolder = folder;
            Name = name;
        }

        internal Layer(Profile profile, Folder folder, LayerEntity layerEntity, IPluginService pluginService)
        {
            LayerEntity = layerEntity;
            Guid = layerEntity.Guid;

            Profile = profile;
            ParentFolder = folder;
            LayerType = pluginService.GetLayerTypeByGuid(System.Guid.Parse(layerEntity.LayerTypeGuid));
        }

        internal LayerEntity LayerEntity { get; set; }
        internal string Guid { get; set; }

        public Profile Profile { get; }
        public Folder ParentFolder { get; }

        public LayerType LayerType { get; private set; }
        public ILayerTypeConfiguration LayerTypeConfiguration { get; set; }

        public override void Update(double deltaTime)
        {
            if (LayerType == null)
                return;

            lock (LayerType)
            {
                LayerType.Update(this);
            }
        }

        public override void Render(double deltaTime, Surface.Surface surface, Graphics graphics)
        {
            if (LayerType == null)
                return;

            lock (LayerType)
            {
                LayerType.Render(this, surface, graphics);
            }
        }

        internal override void ApplyToEntity()
        {
            LayerEntity.Guid = Guid;
            LayerEntity.Order = Order;
            LayerEntity.Name = Name;
            LayerEntity.LayerTypeGuid = LayerType?.PluginInfo.Guid.ToString();

            // TODO: Settings
        }

        public void UpdateLayerType(LayerType layerType)
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

        public override string ToString()
        {
            return $"{nameof(Profile)}: {Profile}, {nameof(Order)}: {Order}, {nameof(Name)}: {Name}";
        }
    }
}
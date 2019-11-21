using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using Artemis.Core.Extensions;
using Artemis.Core.Models.Profile.Abstract;
using Artemis.Core.Models.Surface;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.Interfaces;
using Artemis.Core.Services.Interfaces;
using Artemis.Storage.Entities.Profile;
using Device = Artemis.Core.Models.Surface.Device;

namespace Artemis.Core.Models.Profile
{
    public sealed class Layer : ProfileElement
    {
        internal Layer(Profile profile, Folder folder, string name)
        {
            LayerEntity = new LayerEntity();
            EntityId = Guid.NewGuid();

            Profile = profile;
            ParentFolder = folder;
            Name = name;
            Leds = new List<DeviceLed>();
        }

        internal Layer(Profile profile, Folder folder, LayerEntity layerEntity, IPluginService pluginService)
        {
            LayerEntity = layerEntity;
            EntityId = layerEntity.Id;

            Profile = profile;
            ParentFolder = folder;
            LayerType = pluginService.GetLayerTypeByGuid(layerEntity.LayerTypeGuid);
            Leds = new List<DeviceLed>();
        }

        internal LayerEntity LayerEntity { get; set; }
        internal Guid EntityId { get; set; }

        public Profile Profile { get; }
        public Folder ParentFolder { get; }

        public List<DeviceLed> Leds { get; private set; }
        public LayerType LayerType { get; private set; }
        public ILayerTypeConfiguration LayerTypeConfiguration { get; set; }

        public Rectangle RenderRectangle { get; set; }
        public GraphicsPath RenderPath { get; set; }

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
            LayerEntity.Id = EntityId;
            LayerEntity.ParentId = ParentFolder?.EntityId ?? new Guid();
            LayerEntity.LayerTypeGuid = LayerType?.PluginInfo.Guid ?? new Guid();

            LayerEntity.Order = Order;
            LayerEntity.Name = Name;

            LayerEntity.ProfileId = Profile.EntityId;
            // TODO: LEDs, conditions, elements
        }

        public void ApplySurface(Surface.Surface surface)
        {
            var leds = new List<DeviceLed>();
            foreach (var surfaceDevice in surface.Devices)
            {
                var deviceHash = surfaceDevice.RgbDevice.GetDeviceHashCode();
                leds.AddRange(surfaceDevice.Leds.Where(dl => LayerEntity.Leds.Any(l => l.DeviceHash == deviceHash && l.LedName == dl.RgbLed.ToString())));
            }

            Leds = leds;
            CalculateRenderProperties();
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

        internal void CalculateRenderProperties()
        {
            if (!Leds.Any())
            {
                // TODO: Create an empty rectangle and path
                return;
            }

            // Determine to top-left and bottom-right
            var minX = Leds.Min(l => l.AbsoluteRenderRectangle.X);
            var minY = Leds.Min(l => l.AbsoluteRenderRectangle.Y);
            var maxX = Leds.Max(l => l.AbsoluteRenderRectangle.X);
            var maxY = Leds.Max(l => l.AbsoluteRenderRectangle.Y);

            RenderRectangle = new Rectangle(minX, minY, maxX - minX, maxY - minY);

            var path = new GraphicsPath();
            path.AddRectangles(Leds.Select(l => l.AbsoluteRenderRectangle).ToArray());
            RenderPath = path;
        }
        
        public override string ToString()
        {
            return $"{nameof(Profile)}: {Profile}, {nameof(Order)}: {Order}, {nameof(Name)}: {Name}";
        }
    }
}
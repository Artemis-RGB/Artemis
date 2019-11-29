using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace Artemis.Core.Models.Profile
{
    public sealed class Layer : ProfileElement
    {
        private List<ArtemisLed> _leds;

        public Layer(Profile profile, ProfileElement parent, string name)
        {
            LayerEntity = new LayerEntity();
            EntityId = Guid.NewGuid();

            Profile = profile;
            Parent = parent;
            Name = name;
            _leds = new List<ArtemisLed>();
        }

        internal Layer(Profile profile, ProfileElement parent, LayerEntity layerEntity, IPluginService pluginService)
        {
            LayerEntity = layerEntity;
            EntityId = layerEntity.Id;

            Profile = profile;
            Parent = parent;
            Name = layerEntity.Name;
            Order = layerEntity.Order;

            LayerType = pluginService.GetLayerTypeByGuid(layerEntity.LayerTypeGuid);
            _leds = new List<ArtemisLed>();
        }

        internal LayerEntity LayerEntity { get; set; }

        public ReadOnlyCollection<ArtemisLed> Leds => _leds.AsReadOnly();
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

        public override void Render(double deltaTime, ArtemisSurface surface, Graphics graphics)
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
            LayerEntity.ParentId = Parent?.EntityId ?? new Guid();
            LayerEntity.LayerTypeGuid = LayerType?.PluginInfo.Guid ?? new Guid();

            LayerEntity.Order = Order;
            LayerEntity.Name = Name;

            LayerEntity.ProfileId = Profile.EntityId;

            LayerEntity.Leds.Clear();
            foreach (var artemisLed in Leds)
            {
                var ledEntity = new LedEntity
                {
                    DeviceHash = artemisLed.Device.RgbDevice.GetDeviceHashCode(),
                    LedName = artemisLed.RgbLed.Id.ToString()
                };
                LayerEntity.Leds.Add(ledEntity);
            }

            LayerEntity.Condition.Clear();
            
            LayerEntity.Elements.Clear();
        }

        public void ApplySurface(ArtemisSurface surface)
        {
            var leds = new List<ArtemisLed>();

            // Get the surface LEDs for this layer
            var availableLeds = surface.Devices.SelectMany(d => d.Leds).ToList();
            foreach (var ledEntity in LayerEntity.Leds)
            {
                var match = availableLeds.FirstOrDefault(a => a.Device.RgbDevice.GetDeviceHashCode() == ledEntity.DeviceHash &&
                                                              a.RgbLed.Id.ToString() == ledEntity.LedName);
                if (match != null)
                    leds.Add(match);
            }

            _leds = leds;
            CalculateRenderProperties();
        }

        public void AddLed(ArtemisLed led)
        {
            _leds.Add(led);
            CalculateRenderProperties();
        }

        public void AddLeds(IEnumerable<ArtemisLed> leds)
        {
            _leds.AddRange(leds);
            CalculateRenderProperties();
        }

        public void RemoveLed(ArtemisLed led)
        {
            _leds.Remove(led);
            CalculateRenderProperties();
        }

        public void ClearLeds()
        {
            _leds.Clear();
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
            return $"Layer - {nameof(Name)}: {Name}, {nameof(Order)}: {Order}";
        }
    }
}
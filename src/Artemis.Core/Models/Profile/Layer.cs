using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core.Extensions;
using Artemis.Core.Models.Profile.Abstract;
using Artemis.Core.Models.Surface;
using Artemis.Core.Plugins.LayerElement;
using Artemis.Storage.Entities.Profile;
using Newtonsoft.Json;
using SkiaSharp;

namespace Artemis.Core.Models.Profile
{
    public sealed class Layer : ProfileElement
    {
        private readonly List<LayerElement> _layerElements;
        private List<ArtemisLed> _leds;

        public Layer(Profile profile, ProfileElement parent, string name)
        {
            LayerEntity = new LayerEntity();
            EntityId = Guid.NewGuid();

            Profile = profile;
            Parent = parent;
            Name = name;

            _leds = new List<ArtemisLed>();
            _layerElements = new List<LayerElement>();
        }

        internal Layer(Profile profile, ProfileElement parent, LayerEntity layerEntity)
        {
            LayerEntity = layerEntity;
            EntityId = layerEntity.Id;

            Profile = profile;
            Parent = parent;
            Name = layerEntity.Name;
            Order = layerEntity.Order;

            _leds = new List<ArtemisLed>();
            _layerElements = new List<LayerElement>();
        }

        internal LayerEntity LayerEntity { get; set; }

        public ReadOnlyCollection<ArtemisLed> Leds => _leds.AsReadOnly();
        public ReadOnlyCollection<LayerElement> LayerElements => _layerElements.AsReadOnly();

        public SKRect RenderRectangle { get; set; }
        public SKRect AbsoluteRenderRectangle { get; set; }
        public SKPath RenderPath { get; set; }

        public override void Update(double deltaTime)
        {
            foreach (var layerElement in LayerElements)
                layerElement.Update(deltaTime);
        }

        public override void Render(double deltaTime, ArtemisSurface surface, SKCanvas canvas)
        {
            if (RenderRectangle == null || AbsoluteRenderRectangle == null || RenderPath == null)
                return;

            canvas.Save();

            foreach (var layerElement in LayerElements)
                layerElement.RenderPreProcess(surface, canvas);

            using (var bitmap = new SKBitmap(new SKImageInfo((int) RenderRectangle.Width, (int) RenderRectangle.Height)))
            using (var layerCanvas = new SKCanvas(bitmap))
            {
                layerCanvas.Clear();

                foreach (var layerElement in LayerElements)
                    layerElement.Render(surface, layerCanvas);

                var baseShader = SKShader.CreateBitmap(bitmap, SKShaderTileMode.Repeat, SKShaderTileMode.Repeat, SKMatrix.MakeTranslation(RenderRectangle.Left, RenderRectangle.Top));
                foreach (var layerElement in LayerElements)
                {
                    var newBaseShader = layerElement.RenderPostProcess(surface, bitmap, baseShader);
                    if (newBaseShader == null)
                        continue;

                    // Dispose the old base shader if the layer element provided a new one
                    if (!ReferenceEquals(baseShader, newBaseShader))
                        baseShader.Dispose();

                    baseShader = newBaseShader;
                }

                //canvas.ClipPath(RenderPath);
                canvas.DrawRect(RenderRectangle, new SKPaint {Shader = baseShader, FilterQuality = SKFilterQuality.Low});
                baseShader.Dispose();
            }

            canvas.Restore();
        }

        internal override void ApplyToEntity()
        {
            LayerEntity.Id = EntityId;
            LayerEntity.ParentId = Parent?.EntityId ?? new Guid();

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
            foreach (var layerElement in LayerElements)
            {
                var layerElementEntity = new LayerElementEntity
                {
                    PluginGuid = layerElement.Descriptor.LayerElementProvider.PluginInfo.Guid,
                    LayerElementType = layerElement.GetType().Name,
                    Configuration = JsonConvert.SerializeObject(layerElement.Settings)
                };
                LayerEntity.Elements.Add(layerElementEntity);
            }
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

        internal void AddLayerElement(LayerElement layerElement)
        {
            _layerElements.Add(layerElement);
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

        internal void CalculateRenderProperties()
        {
            if (!Leds.Any())
            {
                // TODO: Create an empty rectangle and path
                return;
            }

            // Determine to top-left and bottom-right
            var minX = Leds.Min(l => l.AbsoluteRenderRectangle.Left);
            var minY = Leds.Min(l => l.AbsoluteRenderRectangle.Top);
            var maxX = Leds.Max(l => l.AbsoluteRenderRectangle.Right);
            var maxY = Leds.Max(l => l.AbsoluteRenderRectangle.Bottom);

            RenderRectangle = SKRect.Create(minX, minY, maxX - minX, maxY - minY);
            AbsoluteRenderRectangle = SKRect.Create(0, 0, maxX - minX, maxY - minY);

            var path = new SKPath {FillType = SKPathFillType.Winding};
            foreach (var artemisLed in Leds)
                path.AddRect(artemisLed.AbsoluteRenderRectangle);

            RenderPath = path;
            OnRenderPropertiesUpdated();
        }

        public override string ToString()
        {
            return $"Layer - {nameof(Name)}: {Name}, {nameof(Order)}: {Order}";
        }

        #region Events

        public event EventHandler RenderPropertiesUpdated;

        private void OnRenderPropertiesUpdated()
        {
            RenderPropertiesUpdated?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}
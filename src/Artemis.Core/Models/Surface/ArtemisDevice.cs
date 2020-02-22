using System;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core.Extensions;
using Artemis.Core.Plugins.Abstract;
using Artemis.Storage.Entities.Surface;
using RGB.NET.Core;
using SkiaSharp;
using Stylet;

namespace Artemis.Core.Models.Surface
{
    public class ArtemisDevice : PropertyChangedBase
    {
        internal ArtemisDevice(IRGBDevice rgbDevice, Plugin plugin, ArtemisSurface surface)
        {
            RgbDevice = rgbDevice;
            Plugin = plugin;
            Surface = surface;
            DeviceEntity = new DeviceEntity();
            Leds = rgbDevice.Select(l => new ArtemisLed(l, this)).ToList().AsReadOnly();

            Rotation = 0;
            Scale = 1;
            ZIndex = 1;

            ApplyToEntity();
            CalculateRenderProperties();
        }

        internal ArtemisDevice(IRGBDevice rgbDevice, Plugin plugin, ArtemisSurface surface, DeviceEntity deviceEntity)
        {
            RgbDevice = rgbDevice;
            Plugin = plugin;
            Surface = surface;
            DeviceEntity = deviceEntity;
            Leds = rgbDevice.Select(l => new ArtemisLed(l, this)).ToList().AsReadOnly();
        }

        public SKRect RenderRectangle { get; private set; }
        public SKPath RenderPath { get; private set; }

        public IRGBDevice RgbDevice { get; }
        public Plugin Plugin { get; }
        public ArtemisSurface Surface { get; }
        public DeviceEntity DeviceEntity { get; }
        public ReadOnlyCollection<ArtemisLed> Leds { get; set; }

        public double X
        {
            get => DeviceEntity.X;
            set => DeviceEntity.X = value;
        }

        public double Y
        {
            get => DeviceEntity.Y;
            set => DeviceEntity.Y = value;
        }

        public double Rotation
        {
            get => DeviceEntity.Rotation;
            set => DeviceEntity.Rotation = value;
        }

        public double Scale
        {
            get => DeviceEntity.Scale;
            set => DeviceEntity.Scale = value;
        }

        public int ZIndex
        {
            get => DeviceEntity.ZIndex;
            set => DeviceEntity.ZIndex = value;
        }

        public override string ToString()
        {
            return $"[{RgbDevice.DeviceInfo.DeviceType}] {RgbDevice.DeviceInfo.DeviceName} - {X}.{Y}.{ZIndex}";
        }

        public event EventHandler DeviceUpdated;

        protected virtual void OnDeviceUpdated()
        {
            DeviceUpdated?.Invoke(this, EventArgs.Empty);
        }

        internal void ApplyToEntity()
        {
            // Other properties are computed
            DeviceEntity.DeviceIdentifier = RgbDevice.GetDeviceIdentifier();
        }

        internal void ApplyToRgbDevice()
        {
            RgbDevice.Location = new Point(DeviceEntity.X, DeviceEntity.Y);
            RgbDevice.Rotation = DeviceEntity.Rotation;
            RgbDevice.Scale = DeviceEntity.Scale;

            CalculateRenderProperties();
            OnDeviceUpdated();
        }

        internal void CalculateRenderProperties()
        {
            RenderRectangle = SKRect.Create(
                (RgbDevice.Location.X * Surface.Scale).RoundToInt(),
                (RgbDevice.Location.Y * Surface.Scale).RoundToInt(),
                (RgbDevice.DeviceRectangle.Size.Width * Surface.Scale).RoundToInt(),
                (RgbDevice.DeviceRectangle.Size.Height * Surface.Scale).RoundToInt()
            );

            if (!Leds.Any())
                return;

            foreach (var led in Leds)
                led.CalculateRenderRectangle();

            var path = new SKPath {FillType = SKPathFillType.Winding};
            foreach (var artemisLed in Leds)
                path.AddRect(artemisLed.AbsoluteRenderRectangle);

            RenderPath = path;
        }
    }
}
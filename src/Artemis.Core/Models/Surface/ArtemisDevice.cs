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
        private SKRect _renderRectangle;
        private SKPath _renderPath;
        private ReadOnlyCollection<ArtemisLed> _leds;

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

        public SKRect RenderRectangle
        {
            get => _renderRectangle;
            private set => SetAndNotify(ref _renderRectangle, value);
        }

        public SKPath RenderPath
        {
            get => _renderPath;
            private set => SetAndNotify(ref _renderPath, value);
        }

        public IRGBDevice RgbDevice { get; }
        public Plugin Plugin { get; }
        public ArtemisSurface Surface { get; }
        public DeviceEntity DeviceEntity { get; }

        public ReadOnlyCollection<ArtemisLed> Leds
        {
            get => _leds;
            set => SetAndNotify(ref _leds, value);
        }

        public double X
        {
            get => DeviceEntity.X;
            set
            {
                DeviceEntity.X = value;
                NotifyOfPropertyChange(nameof(X));
            }
        }

        public double Y
        {
            get => DeviceEntity.Y;
            set
            {
                DeviceEntity.Y = value;
                NotifyOfPropertyChange(nameof(Y));
            }
        }

        public double Rotation
        {
            get => DeviceEntity.Rotation;
            set
            {
                DeviceEntity.Rotation = value;
                NotifyOfPropertyChange(nameof(Rotation));
            }
        }

        public double Scale
        {
            get => DeviceEntity.Scale;
            set
            {
                DeviceEntity.Scale = value;
                NotifyOfPropertyChange(nameof(Scale));
            }
        }

        public int ZIndex
        {
            get => DeviceEntity.ZIndex;
            set
            {
                DeviceEntity.ZIndex = value;
                NotifyOfPropertyChange(nameof(ZIndex));
            }
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
            RgbDevice.Rotation = DeviceEntity.Rotation;
            RgbDevice.Scale = DeviceEntity.Scale;

            // Workaround for device rotation not applying
            if (DeviceEntity.X == 0 && DeviceEntity.Y == 0)
                RgbDevice.Location = new Point(1, 1);
            RgbDevice.Location = new Point(DeviceEntity.X, DeviceEntity.Y);

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
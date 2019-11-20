using System;
using System.Collections.ObjectModel;
using System.Drawing.Drawing2D;
using System.Linq;
using Artemis.Core.Extensions;
using Artemis.Core.Plugins.Abstract;
using Artemis.Storage.Entities.Surface;
using RGB.NET.Core;
using Rectangle = System.Drawing.Rectangle;

namespace Artemis.Core.Models.Surface
{
    public class Device
    {
        internal Device(IRGBDevice rgbDevice, Plugin plugin, Surface surface)
        {
            RgbDevice = rgbDevice;
            Plugin = plugin;
            Surface = surface;
            DeviceEntity = new DeviceEntity();
            Leds = rgbDevice.Select(l => new DeviceLed(l, this)).ToList().AsReadOnly();

            Rotation = 0;
            ZIndex = 1;

            ApplyToEntity();
            CalculateRenderRectangle();
        }

        internal Device(IRGBDevice rgbDevice, Plugin plugin, Surface surface, DeviceEntity deviceEntity)
        {
            RgbDevice = rgbDevice;
            Plugin = plugin;
            Surface = surface;
            DeviceEntity = deviceEntity;
            Leds = rgbDevice.Select(l => new DeviceLed(l, this)).ToList().AsReadOnly();
        }

        public Rectangle RenderRectangle { get; private set; }
        public GraphicsPath RenderPath { get; private set; }

        public IRGBDevice RgbDevice { get; private set; }
        public Plugin Plugin { get; }
        public Surface Surface { get; private set; }
        public DeviceEntity DeviceEntity { get; private set; }
        public ReadOnlyCollection<DeviceLed> Leds { get; set; }

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

        public int ZIndex
        {
            get => DeviceEntity.ZIndex;
            set => DeviceEntity.ZIndex = value;
        }

        internal void ApplyToEntity()
        {
            // Other properties are computed
            DeviceEntity.DeviceHashCode = RgbDevice.GetDeviceHashCode();
        }

        internal void ApplyToRgbDevice()
        {
            RgbDevice.Location = new Point(DeviceEntity.X, DeviceEntity.Y);
            CalculateRenderRectangle();
        }

        internal void CalculateRenderRectangle()
        {
            RenderRectangle = new Rectangle(
                (int) Math.Round(RgbDevice.Location.X * Surface.Scale, MidpointRounding.AwayFromZero),
                (int) Math.Round(RgbDevice.Location.Y * Surface.Scale, MidpointRounding.AwayFromZero),
                (int) Math.Round(RgbDevice.Size.Width * Surface.Scale, MidpointRounding.AwayFromZero),
                (int) Math.Round(RgbDevice.Size.Height * Surface.Scale, MidpointRounding.AwayFromZero)
            );

            if (!Leds.Any())
                return;

            foreach (var led in Leds)
                led.CalculateRenderRectangle();

            var path = new GraphicsPath();
            path.AddRectangles(Leds.Select(l => l.AbsoluteRenderRectangle).ToArray());
            RenderPath = path;
        }

        public override string ToString()
        {
            return $"[{RgbDevice.DeviceInfo.DeviceType}] {RgbDevice.DeviceInfo.DeviceName} - {X}.{Y}.{ZIndex}";
        }
    }
}
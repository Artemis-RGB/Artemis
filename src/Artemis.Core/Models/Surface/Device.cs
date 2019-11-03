using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using Artemis.Core.Extensions;
using Artemis.Core.Plugins.Abstract;
using Artemis.Storage.Entities;
using RGB.NET.Core;
using Point = RGB.NET.Core.Point;
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
            Configuration = new DeviceEntity();
            Leds = rgbDevice.Select(l => new DeviceLed(l, this)).ToList().AsReadOnly();

            Rotation = 0;
            ZIndex = 1;

            ApplyToConfiguration();
            CalculateRenderRectangle();
        }

        internal Device(IRGBDevice rgbDevice, Plugin plugin, Surface surface, DeviceEntity configuration)
        {
            RgbDevice = rgbDevice;
            Plugin = plugin;
            Surface = surface;
            Configuration = configuration;
            Leds = rgbDevice.Select(l => new DeviceLed(l, this)).ToList().AsReadOnly();

            Rotation = configuration.Rotation;
            ZIndex = configuration.ZIndex;
        }

        public Rectangle RenderRectangle { get; private set; }
        public GraphicsPath RenderPath { get; private set; }

        public IRGBDevice RgbDevice { get; private set; }
        public Plugin Plugin { get; }
        public Surface Surface { get; private set; }
        public DeviceEntity Configuration { get; private set; }
        public ReadOnlyCollection<DeviceLed> Leds { get; set; }

        public double X
        {
            get => Configuration.X;
            set => Configuration.X = value;
        }

        public double Y
        {
            get => Configuration.Y;
            set => Configuration.Y = value;
        }

        public double Rotation
        {
            get => Configuration.Rotation;
            set => Configuration.Rotation = value;
        }

        public int ZIndex
        {
            get => Configuration.ZIndex;
            set => Configuration.ZIndex = value;
        }

        internal void ApplyToConfiguration()
        {
            Configuration.SurfaceId = Surface.Guid;
            Configuration.DeviceHashCode = RgbDevice.GetDeviceHashCode();

            // Ensure the position configuration is in the surface configuration's' collection of positions
            if (Surface.SurfaceEntity.DeviceEntities.All(p => p.Guid != Configuration.Guid))
                Surface.SurfaceEntity.DeviceEntities.Add(Configuration);
        }

        internal void ApplyToRgbDevice()
        {
            RgbDevice.Location = new Point(Configuration.X, Configuration.Y);
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

            foreach (var led in Leds)
                led.CalculateRenderRectangle();

            var path = new GraphicsPath();
            path.AddRectangles(Leds.Select(l => l.AbsoluteRenderRectangle).ToArray());
            RenderPath = path;
        }
        
        internal void Destroy()
        {
            Configuration = null;
            RgbDevice = null;
            Surface = null;
        }
    }
}
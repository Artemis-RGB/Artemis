using System;
using System.Collections.Generic;
using System.Linq;
using RGB.NET.Core;

namespace Artemis.Core.Services.Models
{
    internal class SurfaceArrangementType
    {
        public SurfaceArrangementType(SurfaceArrangement surfaceArrangement, RGBDeviceType deviceType, int zIndex)
        {
            SurfaceArrangement = surfaceArrangement;
            DeviceType = deviceType;
            ZIndex = zIndex;
            Configurations = new List<SurfaceArrangementConfiguration>();
        }

        public SurfaceArrangement SurfaceArrangement { get; }
        public RGBDeviceType DeviceType { get; }
        public int ZIndex { get; }
        public List<SurfaceArrangementConfiguration> Configurations { get; }
        public SurfaceArrangementConfiguration? AppliedConfiguration { get; set; }

        public bool HasDevices(ArtemisSurface surface)
        {
            return surface.Devices.Any(d => d.RgbDevice.DeviceInfo.DeviceType == DeviceType);
        }

        public void Arrange(ArtemisSurface surface)
        {
            List<ArtemisDevice> devices = surface.Devices.Where(d => d.RgbDevice.DeviceInfo.DeviceType == DeviceType).ToList();
            if (!devices.Any())
                return;

            AppliedConfiguration = null;
            foreach (SurfaceArrangementConfiguration configuration in Configurations)
            {
                bool applied = configuration.Apply(devices, surface);
                if (applied)
                {
                    AppliedConfiguration = configuration;
                    foreach (ArtemisDevice artemisDevice in devices) 
                        artemisDevice.ZIndex = ZIndex;
                    return;
                }
            }

            // If nothing applied fall back to just basing on whatever is the furthers to the right
            SurfaceArrangementConfiguration fallback = new(
                null,
                HorizontalArrangementPosition.Right,
                VerticalArrangementPosition.Equal,
                10
            ) {SurfaceArrangement = SurfaceArrangement};
            fallback.Apply(devices, surface);
            AppliedConfiguration = fallback;
        }

        public Point GetEdge(HorizontalArrangementPosition horizontalPosition, VerticalArrangementPosition verticalPosition, ArtemisSurface surface)
        {
            List<ArtemisDevice> devices = SurfaceArrangement.ArrangedDevices.Where(d => d.RgbDevice.DeviceInfo.DeviceType == DeviceType || DeviceType == RGBDeviceType.All).ToList();
            if (!devices.Any())
                return new Point();

            double x = horizontalPosition switch
            {
                HorizontalArrangementPosition.Left => devices.Min(d => d.RgbDevice.Location.X) - (AppliedConfiguration?.MarginLeft ?? 0.0),
                HorizontalArrangementPosition.Right => devices.Max(d => d.RgbDevice.Location.X + d.RgbDevice.Size.Width) + (AppliedConfiguration?.MarginRight ?? 0.0),
                HorizontalArrangementPosition.Center => devices.First().RgbDevice.Boundary.Center.X,
                HorizontalArrangementPosition.Equal => devices.First().RgbDevice.Location.X,
                _ => throw new ArgumentOutOfRangeException(nameof(horizontalPosition), horizontalPosition, null)
            };
            double y = verticalPosition switch
            {
                VerticalArrangementPosition.Top => devices.Min(d => d.RgbDevice.Location.Y) - (AppliedConfiguration?.MarginTop ?? 0.0),
                VerticalArrangementPosition.Bottom => devices.Max(d => d.RgbDevice.Location.Y + d.RgbDevice.Size.Height) + (AppliedConfiguration?.MarginBottom ?? 0.0),
                VerticalArrangementPosition.Center => devices.First().RgbDevice.Boundary.Center.Y,
                VerticalArrangementPosition.Equal => devices.First().RgbDevice.Location.Y,
                _ => throw new ArgumentOutOfRangeException(nameof(verticalPosition), verticalPosition, null)
            };

            return new Point(x, y);
        }

        public void AddConfiguration(SurfaceArrangementConfiguration surfaceArrangementConfiguration)
        {
            surfaceArrangementConfiguration.SurfaceArrangement = SurfaceArrangement;
            Configurations.Add(surfaceArrangementConfiguration);
        }
    }

    internal enum HorizontalArrangementPosition
    {
        Left,
        Right,
        Equal,
        Center
    }

    internal enum VerticalArrangementPosition
    {
        Top,
        Bottom,
        Equal,
        Center
    }
}
using Artemis.Storage.Entities;
using RGB.NET.Core;

namespace Artemis.Core.Models.Surface
{
    public class SurfaceDeviceConfiguration
    {
        internal SurfaceDeviceConfiguration(int deviceId, IRGBDeviceInfo deviceInfo, SurfaceConfiguration surface)
        {
            DeviceId = deviceId;
            DeviceName = deviceInfo.DeviceName;
            DeviceModel = deviceInfo.Model;
            DeviceManufacturer = deviceInfo.Manufacturer;

            Surface = surface;
        }

        internal SurfaceDeviceConfiguration(SurfacePositionEntity position, SurfaceConfiguration surfaceConfiguration)
        {
            Guid = position.Guid;

            DeviceId = position.DeviceId;
            DeviceName = position.DeviceName;
            DeviceModel = position.DeviceModel;
            DeviceManufacturer = position.DeviceManufacturer;

            X = position.X;
            Y = position.Y;
            Rotation = position.Rotation;
            ZIndex = position.ZIndex;

            Surface = surfaceConfiguration;
        }
        
        internal string Guid { get; set; }

        public int DeviceId { get; set; }
        public string DeviceName { get; set; }
        public string DeviceModel { get; set; }
        public string DeviceManufacturer { get; set; }

        public double X { get; set; }
        public double Y { get; set; }
        public double Rotation { get; set; }
        public int ZIndex { get; set; }

        public SurfaceConfiguration Surface { get; internal set; }
    }
}
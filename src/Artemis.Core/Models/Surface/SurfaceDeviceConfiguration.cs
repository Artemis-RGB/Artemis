using System.Linq;
using Artemis.Storage.Entities;
using RGB.NET.Core;

namespace Artemis.Core.Models.Surface
{
    public class SurfaceDeviceConfiguration
    {
        internal SurfaceDeviceConfiguration(IRGBDevice device, int deviceId, SurfaceConfiguration surface)
        {
            PositionEntity = new SurfacePositionEntity();
            Guid = System.Guid.NewGuid().ToString();

            Device = device;
            DeviceId = deviceId;
            DeviceName = device.DeviceInfo.DeviceName;
            DeviceModel = device.DeviceInfo.Model;
            DeviceManufacturer = device.DeviceInfo.Manufacturer;

            X = device.Location.X;
            Y = device.Location.Y;
            Rotation = 0;
            ZIndex = 1;

            Surface = surface;

            ApplyToEntity();
        }

        internal SurfaceDeviceConfiguration(SurfacePositionEntity positionEntity, SurfaceConfiguration surfaceConfiguration)
        {
            PositionEntity = positionEntity;
            Guid = positionEntity.Guid;

            DeviceId = positionEntity.DeviceId;
            DeviceName = positionEntity.DeviceName;
            DeviceModel = positionEntity.DeviceModel;
            DeviceManufacturer = positionEntity.DeviceManufacturer;

            X = positionEntity.X;
            Y = positionEntity.Y;
            Rotation = positionEntity.Rotation;
            ZIndex = positionEntity.ZIndex;

            Surface = surfaceConfiguration;
        }

        internal SurfacePositionEntity PositionEntity { get; set; }
        internal string Guid { get; }

        public IRGBDevice Device { get; internal set; }
        public int DeviceId { get; }
        public string DeviceName { get; }
        public string DeviceModel { get; }
        public string DeviceManufacturer { get; }

        public double X { get; set; }
        public double Y { get; set; }
        public double Rotation { get; set; }
        public int ZIndex { get; set; }

        public SurfaceConfiguration Surface { get; private set; }

        /// <summary>
        ///     Applies the configuration to the device
        /// </summary>
        public void ApplyToDevice()
        {
            if (Device != null)
            {
                Device.Location = new Point(X, Y);
            }
        }

        /// <summary>
        ///     Must be called when saving to the database
        /// </summary>
        internal void ApplyToEntity()
        {
            PositionEntity.Guid = Guid;
            PositionEntity.SurfaceId = Surface.Guid;

            PositionEntity.DeviceId = DeviceId;
            PositionEntity.DeviceName = DeviceName;
            PositionEntity.DeviceModel = DeviceModel;
            PositionEntity.DeviceManufacturer = DeviceManufacturer;

            PositionEntity.X = X;
            PositionEntity.Y = Y;
            PositionEntity.Rotation = Rotation;
            PositionEntity.ZIndex = ZIndex;

            // Ensure the position entity is in the surface entity's' collection of positions
            if (Surface.SurfaceEntity.SurfacePositions.All(p => p.Guid != Guid))
                Surface.SurfaceEntity.SurfacePositions.Add(PositionEntity);
        }

        public void Destroy()
        {
            PositionEntity = null;
            Device = null;
            Surface = null;
        }
    }
}
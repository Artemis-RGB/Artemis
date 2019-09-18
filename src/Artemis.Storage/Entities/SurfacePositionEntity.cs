using System.ComponentModel.DataAnnotations;

namespace Artemis.Storage.Entities
{
    public class SurfacePositionEntity
    {
        [Key]
        public string Guid { get; set; }

        public int DeviceId { get; set; }
        public string DeviceName { get; set; }
        public string DeviceModel { get; set; }
        public string DeviceManufacturer { get; set; }

        public double X { get; set; }
        public double Y { get; set; }
        public double Rotation { get; set; }

        public string SurfaceId { get; set; }
        public virtual SurfaceEntity Surface { get; set; }
    }
}
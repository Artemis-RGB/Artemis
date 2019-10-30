using System.ComponentModel.DataAnnotations;

namespace Artemis.Storage.Entities
{
    public class DeviceEntity
    {
        public DeviceEntity()
        {
            Guid = System.Guid.NewGuid().ToString();
        }

        [Key]
        public string Guid { get; set; }

        public int DeviceHashCode { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Rotation { get; set; }
        public int ZIndex { get; set; }

        public string SurfaceId { get; set; }
        public virtual SurfaceEntity Surface { get; set; }
    }
}
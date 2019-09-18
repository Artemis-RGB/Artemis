using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Artemis.Storage.Entities
{
    public class SurfaceEntity
    {
        [Key]
        public string Guid { get; set; }

        public string Name { get; set; }
        public bool IsActive { get; set; }

        public virtual ICollection<SurfacePositionEntity> SurfacePositions { get; set; }
    }
}
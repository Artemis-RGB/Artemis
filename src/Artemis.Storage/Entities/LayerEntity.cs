using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Artemis.Storage.Entities
{
    [Table("Layers")]
    public class LayerEntity
    {
        [Key]
        public string Guid { get; set; }

        public virtual ICollection<LedEntity> Leds { get; set; }
        public virtual ICollection<LayerSettingEntity> Settings { get; set; }

        public int Order { get; set; }
        public string Name { get; set; }
        public string LayerTypeGuid { get; set; }
    }
}
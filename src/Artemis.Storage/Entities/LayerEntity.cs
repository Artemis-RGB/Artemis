using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Artemis.Storage.Entities
{
    [Table("Layers")]
    internal class LayerEntity
    {
        [Key]
        public int Id { get; set; }

        public string Type { get; set; }

        public virtual ICollection<LedEntity> Leds { get; set; }
        public virtual ICollection<LayerSettingEntity> Settings { get; set; }

        public int Order { get; set; }
        public string Name { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Artemis.Storage.Entities
{
    [Table("Leds")]
    public class LedEntity
    {
        [Key]
        public string Guid { get; set; }

        public string LedName { get; set; }
        public string LimitedToDevice { get; set; }

        public string LayerId { get; set; }
        public virtual LayerEntity Layer { get; set; }
    }
}
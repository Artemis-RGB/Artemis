using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Artemis.Storage.Entities
{
    [Table("Leds")]
    internal class LedEntity
    {
        [Key]
        public int Id { get; set; }

        public string LedName { get; set; }
        public string LimitedToDevice { get; set; }

        public int LayerId { get; set; }
        public virtual LayerEntity Layer { get; set; }
    }
}
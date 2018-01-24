using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Artemis.Storage.Entities
{
    [Table("LayerSettings")]
    internal class LayerSettingEntity
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
        public string Value { get; set; }

        public ICollection<KeypointEntity> Keypoints { get; set; }
    }
}
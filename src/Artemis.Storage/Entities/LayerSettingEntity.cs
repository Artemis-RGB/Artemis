using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Artemis.Storage.Entities
{
    [Table("LayerSettings")]
    public class LayerSettingEntity
    {
        [Key]
        public string Guid { get; set; }

        public string Name { get; set; }
        public string Value { get; set; }

        public ICollection<KeypointEntity> Keypoints { get; set; }
    }
}
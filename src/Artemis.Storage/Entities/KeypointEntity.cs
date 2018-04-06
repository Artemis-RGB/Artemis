using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Artemis.Storage.Entities
{
    [Table("Keypoints")]
    public class KeypointEntity
    {
        [Key]
        public string Guid { get; set; }

        public int Time { get; set; }
        public string Value { get; set; }
    }
}
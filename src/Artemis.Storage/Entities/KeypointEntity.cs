using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Artemis.Storage.Entities
{
    [Table("Keypoints")]
    internal class KeypointEntity
    {
        [Key]
        public int Id { get; set; }

        public int Time { get; set; }
        public string Value { get; set; }
    }
}
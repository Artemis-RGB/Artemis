using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Artemis.Storage.Entities
{
    [Table("Folders")]
    internal class FolderEntity
    {
        [Key]
        public int Id { get; set; }

        public int Order { get; set; }
        public string Name { get; set; }

        public virtual ICollection<FolderEntity> Folders { get; set; }
        public virtual ICollection<LayerEntity> Layers { get; set; }
    }
}
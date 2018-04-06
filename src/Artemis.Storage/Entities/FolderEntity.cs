using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Artemis.Storage.Entities
{
    [Table("Folders")]
    public class FolderEntity
    {
        [Key]
        public string Guid { get; set; }

        public int Order { get; set; }
        public string Name { get; set; }

        public virtual ICollection<FolderEntity> Folders { get; set; }
        public virtual ICollection<LayerEntity> Layers { get; set; }
    }
}
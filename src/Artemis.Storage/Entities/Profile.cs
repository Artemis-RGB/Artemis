using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Artemis.Storage.Entities
{
    internal class Profile
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
        public virtual ICollection<Layer> Layers { get; set; }
    }
}
using System.Collections.Generic;

namespace Artemis.Storage.Entities
{
    internal class Layer
    {
        public int ProfileId { get; set; }
        public string Name { get; set; }
        public LayerType LayerType { get; set; }

        public virtual Profile Profile { get; set; }
        public virtual ICollection<Layer> Layers { get; set; }
    }

    internal enum LayerType
    {
    }
}

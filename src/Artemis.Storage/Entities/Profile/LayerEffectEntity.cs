using System;

namespace Artemis.Storage.Entities.Profile
{
    public class LayerEffectEntity
    {
        public Guid Id { get; set; }
        public string ProviderId { get; set; }
        public string EffectType { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public bool HasBeenRenamed { get; set; }
        public int Order { get; set; }
    }
}
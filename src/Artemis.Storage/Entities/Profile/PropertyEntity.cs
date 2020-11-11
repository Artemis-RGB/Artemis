using System.Collections.Generic;
using Artemis.Storage.Entities.Profile.DataBindings;

namespace Artemis.Storage.Entities.Profile
{
    public class PropertyEntity
    {
        public PropertyEntity()
        {
            KeyframeEntities = new List<KeyframeEntity>();
            DataBindingEntities = new List<DataBindingEntity>();
        }

        public string FeatureId { get; set; }
        public string Path { get; set; }

        public string Value { get; set; }
        public bool KeyframesEnabled { get; set; }

        public List<KeyframeEntity> KeyframeEntities { get; set; }
        public List<DataBindingEntity> DataBindingEntities { get; set; }
    }
}
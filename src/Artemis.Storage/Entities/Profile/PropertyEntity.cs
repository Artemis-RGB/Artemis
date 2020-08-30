using System;
using System.Collections.Generic;
using Artemis.Storage.Entities.Profile.DataBindings;

namespace Artemis.Storage.Entities.Profile
{
    public class PropertyEntity
    {
        public PropertyEntity()
        {
            KeyframeEntities = new List<KeyframeEntity>();
        }

        public Guid PluginGuid { get; set; }
        public string Path { get; set; }

        public string Value { get; set; }
        public bool KeyframesEnabled { get; set; }

        public List<KeyframeEntity> KeyframeEntities { get; set; }
        public DataBindingEntity DataBindingEntity { get; set; }
    }
}
using System;
using System.Collections.Generic;

namespace Artemis.Storage.Entities.Profile
{
    public class PropertyEntity
    {
        public PropertyEntity()
        {
            KeyframeEntities = new List<KeyframeEntity>();
        }

        public string Id { get; set; }
        public string ValueType { get; set; }
        public string Value { get; set; }

        public List<KeyframeEntity> KeyframeEntities { get; set; }
    }

    public class KeyframeEntity
    {
        public TimeSpan Position { get; set; }
        public string Value { get; set; }
    }
}
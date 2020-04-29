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

        public Guid PluginGuid { get; set; }
        public string Path { get; set; }

        public string Value { get; set; }
        public bool IsUsingKeyframes { get; set; }

        public List<KeyframeEntity> KeyframeEntities { get; set; }
    }

    public class KeyframeEntity
    {
        public TimeSpan Position { get; set; }
        public string Value { get; set; }
        public int EasingFunction { get; set; }
    }
}
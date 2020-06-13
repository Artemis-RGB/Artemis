using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using LiteDB;

namespace Artemis.Storage.Entities.Profile
{
    public class LayerEntity
    {
        public LayerEntity()
        {
            Leds = new List<LedEntity>();
            PropertyEntities = new List<PropertyEntity>();
            Condition = new List<ProfileConditionEntity>();
            LayerEffects = new List<LayerEffectEntity>();
            ExpandedPropertyGroups = new List<string>();
        }

        public Guid Id { get; set; }
        public Guid ParentId { get; set; }

        public int Order { get; set; }
        public string Name { get; set; }

        public List<LedEntity> Leds { get; set; }
        public List<PropertyEntity> PropertyEntities { get; set; }
        public List<ProfileConditionEntity> Condition { get; set; }
        public List<LayerEffectEntity> LayerEffects { get; set; }
        public List<string> ExpandedPropertyGroups { get; set; }

        [BsonRef("ProfileEntity")]
        public ProfileEntity Profile { get; set; }

        public Guid ProfileId { get; set; }
    }


}
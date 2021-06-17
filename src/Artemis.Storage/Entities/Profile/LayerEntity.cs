using System;
using System.Collections.Generic;
using Artemis.Storage.Entities.General;
using Artemis.Storage.Entities.Profile.Abstract;
using Artemis.Storage.Entities.Profile.AdaptionHints;
using LiteDB;

namespace Artemis.Storage.Entities.Profile
{
    public class LayerEntity : RenderElementEntity
    {
        public LayerEntity()
        {
            Leds = new List<LedEntity>();
            AdaptionHints = new List<IAdaptionHintEntity>();
            ScriptConfiguration = new List<ScriptConfigurationEntity>();
            PropertyEntities = new List<PropertyEntity>();
            LayerEffects = new List<LayerEffectEntity>();
            ExpandedPropertyGroups = new List<string>();
        }

        public int Order { get; set; }
        public string Name { get; set; }
        public bool Suspended { get; set; }

        public List<LedEntity> Leds { get; set; }
        public List<IAdaptionHintEntity> AdaptionHints { get; set; }
        public List<ScriptConfigurationEntity> ScriptConfiguration { get; set; }

        [BsonRef("ProfileEntity")]
        public ProfileEntity Profile { get; set; }

        public Guid ProfileId { get; set; }
    }
}
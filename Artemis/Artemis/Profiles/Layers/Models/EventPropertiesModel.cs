using System;
using NClone.MetadataProviders;
using Newtonsoft.Json;

namespace Artemis.Profiles.Layers.Models
{
    public abstract class EventPropertiesModel
    {
        public ExpirationType ExpirationType { get; set; }
        public TimeSpan Length { get; set; }
        public TimeSpan TriggerDelay { get; set; }

        [JsonIgnore]
        [CustomReplicationBehavior(ReplicationBehavior.Ignore)]
        public bool MustTrigger { get; set; }

        [JsonIgnore]
        [CustomReplicationBehavior(ReplicationBehavior.Ignore)]
        public DateTime AnimationStart { get; set; }

        [JsonIgnore]
        [CustomReplicationBehavior(ReplicationBehavior.Ignore)]
        public bool MustDraw { get; set; }

        /// <summary>
        ///     Resets the event's properties and triggers it
        /// </summary>
        public abstract void TriggerEvent(LayerModel layer);

        /// <summary>
        ///     Gets whether the event should stop
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public abstract bool MustStop(LayerModel layer);

        // Called every frame, if parent conditions met.
        public void Update(LayerModel layerModel, bool conditionsMet)
        {
            if (MustStop(layerModel))
                MustDraw = false;

            if (!conditionsMet)
                MustTrigger = true;
        }
    }

    public enum ExpirationType
    {
        Time,
        Animation
    }
}
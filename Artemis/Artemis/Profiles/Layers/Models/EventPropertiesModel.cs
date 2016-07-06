using System;
using Newtonsoft.Json;

namespace Artemis.Profiles.Layers.Models
{
    public abstract class EventPropertiesModel
    {
        public ExpirationType ExpirationType { get; set; }
        public TimeSpan Length { get; set; }
        public TimeSpan TriggerDelay { get; set; }

        [JsonIgnore]
        public bool MustTrigger { get; set; }

        [JsonIgnore]
        public DateTime AnimationStart { get; set; }

        [JsonIgnore]
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
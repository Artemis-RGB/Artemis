using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Artemis.Profiles.Layers.Models
{
    public abstract class EventPropertiesModel
    {
        public ExpirationType ExpirationType { get; set; }
        public TimeSpan Length { get; set; }
        public TimeSpan TriggerDelay { get; set; }

        [JsonIgnore]
        public bool CanTrigger { get; set; }

        [JsonIgnore]
        public DateTime EventTriggerTime { get; set; }

        [JsonIgnore]
        public bool MustDraw { get; set; }

        /// <summary>
        ///     Resets the event's properties and triggers it
        /// </summary>
        public virtual void TriggerEvent(LayerModel layer)
        {
            if (!CanTrigger)
                return;

            if (TriggerDelay > TimeSpan.Zero)
            {
                if (EventCanTriggerTime == DateTime.MinValue)
                    EventCanTriggerTime = DateTime.Now;

                if (DateTime.Now - EventCanTriggerTime < TriggerDelay)
                    return;

                EventCanTriggerTime = DateTime.MinValue;
            }

            CanTrigger = false;
            MustDraw = true;
            EventTriggerTime = DateTime.Now;
            layer.Properties.AnimationProgress = 0.0;
        }

        public DateTime EventCanTriggerTime { get; set; }


        /// <summary>
        ///     Gets whether the event should stop
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public virtual bool MustStop(LayerModel layer)
        {
            if (ExpirationType == ExpirationType.Time)
            {
                if (EventTriggerTime == DateTime.MinValue)
                    return false;
                return DateTime.Now - EventTriggerTime > Length;
            }
            if (ExpirationType == ExpirationType.Animation)
                return (layer.LayerAnimation == null) || layer.LayerAnimation.MustExpire(layer);

            return true;
        }

        // Called every frame, if parent conditions met.
        public void Update(LayerModel layerModel, bool conditionsMet)
        {
            if (EventCanTriggerTime > DateTime.MinValue && (DateTime.Now - EventCanTriggerTime > TriggerDelay))
            {
                CanTrigger = true;
                TriggerEvent(layerModel);
                return;
            }

            if (MustDraw && MustStop(layerModel))
                MustDraw = false;

            if (!conditionsMet)
                CanTrigger = true;
        }

        protected bool DelayExpired()
        {
            return EventCanTriggerTime > DateTime.MinValue && DateTime.Now - EventCanTriggerTime >= TriggerDelay;
        }
    }

    public enum ExpirationType
    {
        Time,
        Animation
    }
}
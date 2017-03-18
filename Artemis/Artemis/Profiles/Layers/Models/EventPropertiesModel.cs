using System;
using System.Timers;
using Newtonsoft.Json;

namespace Artemis.Profiles.Layers.Models
{
    public abstract class EventPropertiesModel
    {
        public ExpirationType ExpirationType { get; set; }
        public TimeSpan Length { get; set; }
        public TimeSpan TriggerDelay { get; set; }

        [JsonIgnore]
        public bool CanTrigger { get; set; } = true;

        [JsonIgnore]
        public DateTime EventTriggerTime { get; set; }

        [JsonIgnore]
        public bool MustDraw { get; set; }

        /// <summary>
        ///     If possible, triggers the event
        /// </summary>
        public virtual void TriggerEvent(LayerModel layer)
        {
            if (!CanTrigger)
                return;

            // Don't allow any more triggering regardless of what happens next
            CanTrigger = false;

            // If there is a trigger delay, stop here and await that delay
            if (TriggerDelay > TimeSpan.Zero)
            {
                var timer = new Timer(TriggerDelay.TotalMilliseconds) {AutoReset = false};
                timer.Elapsed += (sender, args) =>
                {
                    HardTrigger(layer);
                    timer.Dispose();
                };
                timer.Start();

                return;
            }

            // Trigger the event
            HardTrigger(layer);
        }

        /// <summary>
        ///     Instantly trigger the event regardless of current state
        /// </summary>
        /// <param name="layer"></param>
        public void HardTrigger(LayerModel layer)
        {
            MustDraw = true;
            CanTrigger = false;
            EventTriggerTime = DateTime.Now;

            // Reset the animation in case it didn't finish before
            layer.AnimationProgress = 0.0;
        }


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
                return layer.LayerAnimation == null || layer.LayerAnimation.MustExpire(layer);

            return true;
        }

        // Called every frame, if parent conditions met.
        public void Update(LayerModel layerModel, bool conditionsMet)
        {
            // If the event isn't finished yet just keep going
            if (MustDraw && !MustStop(layerModel))
                return;

            // Otherwise make sure MustDraw is false
            MustDraw = false;

            // If the conditions aren't met and the event has finished it can be triggered again
            if (!conditionsMet)
                CanTrigger = true;
        }
    }

    public enum ExpirationType
    {
        Time,
        Animation
    }
}

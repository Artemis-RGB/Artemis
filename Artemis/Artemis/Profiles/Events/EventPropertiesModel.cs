using System;
using System.Xml.Serialization;
using Artemis.Profiles.Layers.Models;

namespace Artemis.Profiles.Events
{
    [XmlInclude(typeof(KeyboardEventPropertiesModel))]
    public abstract class EventPropertiesModel
    {
        public ExpirationType ExpirationType { get; set; }

        // Pretend property for serialization
        [XmlElement("Length")]
        public long LengthTicks
        {
            get { return Length.Ticks; }
            set { Length = new TimeSpan(value); }
        }

        // Pretend property for serialization
        [XmlElement("TriggerDelay")]
        public long TriggerDelayTicks
        {
            get { return TriggerDelay.Ticks; }
            set { TriggerDelay = new TimeSpan(value); }
        }

        [XmlIgnore]
        public TimeSpan Length { get; set; }

        [XmlIgnore]
        public TimeSpan TriggerDelay { get; set; }

        [XmlIgnore]
        public bool MustTrigger { get; set; }

        [XmlIgnore]
        public DateTime AnimationStart { get; set; }

        [XmlIgnore]
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
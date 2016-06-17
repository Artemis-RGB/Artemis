using System;
using System.Xml.Serialization;
using Artemis.Models.Profiles.Layers;

namespace Artemis.Models.Profiles.Events
{
    [XmlInclude(typeof(KeyboardEventPropertiesModel))]
    public abstract class EventPropertiesModel
    {
        public ExpirationType ExpirationType { get; set; }
        public TimeSpan Length { get; set; }
        public TimeSpan TriggerDelay { get; set; }

        [XmlIgnore]
        public bool MustTrigger { get; set; }

        [XmlIgnore]
        public DateTime AnimationStart { get; set; }

        /// <summary>
        ///     Resets the event's properties and triggers it
        /// </summary>
        public abstract void TriggerEvent(LayerModel layer);

        /// <summary>
        /// Gets whether the event should stop
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public abstract bool MustStop(LayerModel layer);
    }

    public enum ExpirationType
    {
        Time,
        Animation
    }
}
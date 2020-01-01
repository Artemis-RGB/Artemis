using System;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Timeline
{
    public class TimelineKeyframeViewModel : PropertyChangedBase
    {
        private const double BaseSize = 60.0;

        public TimeSpan Position { get; set; }
        
        public double X { get; set; }
        public string Timestamp { get; set; }

        public void Update(int pixelsPerSecond)
        {
//            var timelinePartWidth = pixelsPerSecond % BaseSize + BaseSize;
//            var millisecondsPerPart = extraParts > 0 ? BaseSize * 1000 / extraParts : BaseSize * 1000;
            X = pixelsPerSecond * Position.TotalSeconds;
            Timestamp = $"{Math.Floor(Position.TotalSeconds):00}.{Position.Milliseconds:000}";
        }
    }
}
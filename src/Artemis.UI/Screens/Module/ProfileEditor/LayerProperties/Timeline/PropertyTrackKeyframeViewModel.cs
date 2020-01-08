using System;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.LayerProperties;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Timeline
{
    public class PropertyTrackKeyframeViewModel : PropertyChangedBase
    {
        public PropertyTrackKeyframeViewModel(BaseKeyframe keyframe)
        {
            Keyframe = keyframe;
        }

        public BaseKeyframe Keyframe { get; }

        public double X { get; set; }
        public string Timestamp { get; set; }

        public void Update(int pixelsPerSecond)
        {
            X = pixelsPerSecond * Keyframe.Position.TotalSeconds;
            Timestamp = $"{Math.Floor(Keyframe.Position.TotalSeconds):00}.{Keyframe.Position.Milliseconds:000}";
        }
    }
}
using Artemis.Core.Models.Profile.LayerProperties.Abstract;
using SkiaSharp;

namespace Artemis.Core.Models.Profile.LayerProperties
{
    public class SKPointLayerProperty : LayerProperty<SKPoint>
    {
        protected override void UpdateCurrentValue(float keyframeProgress, float keyframeProgressEased)
        {
            var xDiff = NextKeyframe.Value.X - CurrentKeyframe.Value.X;
            var yDiff = NextKeyframe.Value.Y - CurrentKeyframe.Value.Y;
            CurrentValue = new SKPoint(CurrentKeyframe.Value.X + xDiff * keyframeProgressEased, CurrentKeyframe.Value.Y + yDiff * keyframeProgressEased);
        }
    }
}
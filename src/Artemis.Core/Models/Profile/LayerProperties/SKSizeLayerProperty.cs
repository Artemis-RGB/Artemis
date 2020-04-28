using Artemis.Core.Models.Profile.LayerProperties.Abstract;
using SkiaSharp;

namespace Artemis.Core.Models.Profile.LayerProperties
{
    public class SKSizeLayerProperty : LayerProperty<SKSize>
    {
        protected override void UpdateCurrentValue(float keyframeProgress, float keyframeProgressEased)
        {
            var widthDiff = NextKeyframe.Value.Width - CurrentKeyframe.Value.Width;
            var heightDiff = NextKeyframe.Value.Height - CurrentKeyframe.Value.Height;
            CurrentValue = new SKSize(CurrentKeyframe.Value.Width + widthDiff * keyframeProgressEased, CurrentKeyframe.Value.Height + heightDiff * keyframeProgressEased);
        }
    }
}
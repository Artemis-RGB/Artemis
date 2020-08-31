using SkiaSharp;

namespace Artemis.Core
{
    /// <inheritdoc />
    public class SKPointLayerProperty : LayerProperty<SKPoint>
    {
        internal SKPointLayerProperty()
        {
        }

        public static implicit operator SKPoint(SKPointLayerProperty p)
        {
            return p.CurrentValue;
        }

        protected override void UpdateCurrentValue(float keyframeProgress, float keyframeProgressEased)
        {
            var xDiff = NextKeyframe.Value.X - CurrentKeyframe.Value.X;
            var yDiff = NextKeyframe.Value.Y - CurrentKeyframe.Value.Y;
            CurrentValue = new SKPoint(CurrentKeyframe.Value.X + xDiff * keyframeProgressEased, CurrentKeyframe.Value.Y + yDiff * keyframeProgressEased);
        }
    }
}
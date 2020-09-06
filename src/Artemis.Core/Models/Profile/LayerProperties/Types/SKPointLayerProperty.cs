using SkiaSharp;

namespace Artemis.Core
{
    /// <inheritdoc />
    public class SKPointLayerProperty : LayerProperty<SKPoint>
    {
        internal SKPointLayerProperty()
        {
            RegisterDataBindingProperty(point => point.X, new FloatDataBindingConverter());
            RegisterDataBindingProperty(point => point.Y, new FloatDataBindingConverter());
        }

        /// <summary>
        ///     Implicitly converts an <see cref="SKPointLayerProperty" /> to an <see cref="SKPoint" />
        /// </summary>
        public static implicit operator SKPoint(SKPointLayerProperty p)
        {
            return p.CurrentValue;
        }

        /// <inheritdoc />
        protected override void UpdateCurrentValue(float keyframeProgress, float keyframeProgressEased)
        {
            var xDiff = NextKeyframe.Value.X - CurrentKeyframe.Value.X;
            var yDiff = NextKeyframe.Value.Y - CurrentKeyframe.Value.Y;
            CurrentValue = new SKPoint(CurrentKeyframe.Value.X + xDiff * keyframeProgressEased, CurrentKeyframe.Value.Y + yDiff * keyframeProgressEased);
        }
    }
}
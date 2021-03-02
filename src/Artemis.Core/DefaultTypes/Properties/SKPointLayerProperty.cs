using SkiaSharp;

namespace Artemis.Core
{
    /// <inheritdoc />
    public class SKPointLayerProperty : LayerProperty<SKPoint>
    {
        internal SKPointLayerProperty()
        {
            RegisterDataBindingProperty(() => CurrentValue.X, value => CurrentValue = new SKPoint(value, CurrentValue.Y), new FloatDataBindingConverter<SKPoint>(), "X");
            RegisterDataBindingProperty(() => CurrentValue.Y, value => CurrentValue = new SKPoint(CurrentValue.X, value), new FloatDataBindingConverter<SKPoint>(), "Y");
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
            float xDiff = NextKeyframe!.Value.X - CurrentKeyframe!.Value.X;
            float yDiff = NextKeyframe!.Value.Y - CurrentKeyframe!.Value.Y;
            CurrentValue = new SKPoint(CurrentKeyframe!.Value.X + xDiff * keyframeProgressEased, CurrentKeyframe!.Value.Y + yDiff * keyframeProgressEased);
        }
    }
}
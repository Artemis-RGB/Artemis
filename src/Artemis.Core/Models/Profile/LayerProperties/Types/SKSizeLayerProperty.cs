using SkiaSharp;

namespace Artemis.Core
{
    /// <inheritdoc />
    public class SKSizeLayerProperty : LayerProperty<SKSize>
    {
        internal SKSizeLayerProperty()
        {
            RegisterDataBindingProperty(size => size.Width, new FloatDataBindingConverter());
            RegisterDataBindingProperty(size => size.Height, new FloatDataBindingConverter());
        }

        /// <summary>
        ///     Implicitly converts an <see cref="SKSizeLayerProperty" /> to an <see cref="SKSize" />
        /// </summary>
        public static implicit operator SKSize(SKSizeLayerProperty p)
        {
            return p.CurrentValue;
        }

        /// <inheritdoc />
        protected override void UpdateCurrentValue(float keyframeProgress, float keyframeProgressEased)
        {
            var widthDiff = NextKeyframe.Value.Width - CurrentKeyframe.Value.Width;
            var heightDiff = NextKeyframe.Value.Height - CurrentKeyframe.Value.Height;
            CurrentValue = new SKSize(CurrentKeyframe.Value.Width + widthDiff * keyframeProgressEased, CurrentKeyframe.Value.Height + heightDiff * keyframeProgressEased);
        }
    }
}
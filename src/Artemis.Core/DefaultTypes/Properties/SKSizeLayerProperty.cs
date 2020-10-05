using SkiaSharp;

namespace Artemis.Core.DefaultTypes
{
    /// <inheritdoc />
    public class SKSizeLayerProperty : LayerProperty<SKSize>
    {
        internal SKSizeLayerProperty()
        {
            RegisterDataBindingProperty(size => size.Height, new FloatDataBindingConverter<SKSize>());
            RegisterDataBindingProperty(size => size.Width, new FloatDataBindingConverter<SKSize>());
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
            float widthDiff = NextKeyframe.Value.Width - CurrentKeyframe.Value.Width;
            float heightDiff = NextKeyframe.Value.Height - CurrentKeyframe.Value.Height;
            CurrentValue = new SKSize(CurrentKeyframe.Value.Width + widthDiff * keyframeProgressEased, CurrentKeyframe.Value.Height + heightDiff * keyframeProgressEased);
        }
    }
}
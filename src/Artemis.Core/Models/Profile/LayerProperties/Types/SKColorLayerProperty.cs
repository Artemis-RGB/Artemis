using SkiaSharp;

namespace Artemis.Core
{
    /// <inheritdoc />
    public class SKColorLayerProperty : LayerProperty<SKColor>
    {
        internal SKColorLayerProperty()
        {
            RegisterDataBindingProperty(value => value, new SKColorDataBindingConverter());
        }

        /// <summary>
        ///     Implicitly converts an <see cref="SKColorLayerProperty" /> to an <see cref="SKColor" />¶
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static implicit operator SKColor(SKColorLayerProperty p)
        {
            return p.CurrentValue;
        }

        /// <inheritdoc />
        protected override void UpdateCurrentValue(float keyframeProgress, float keyframeProgressEased)
        {
            CurrentValue = CurrentKeyframe.Value.Interpolate(NextKeyframe.Value, keyframeProgressEased);
        }
    }
}
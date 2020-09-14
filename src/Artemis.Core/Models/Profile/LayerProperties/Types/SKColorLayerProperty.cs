using SkiaSharp;

namespace Artemis.Core
{
    /// <inheritdoc />
    public class SKColorLayerProperty : LayerProperty<SKColor>
    {
        internal SKColorLayerProperty()
        {
            RegisterDataBindingProperty(color => color.Alpha, new SKColorArgbDataBindingConverter(SKColorArgbDataBindingConverter.Channel.Alpha));
            RegisterDataBindingProperty(color => color.Red, new SKColorArgbDataBindingConverter(SKColorArgbDataBindingConverter.Channel.Red));
            RegisterDataBindingProperty(color => color.Green, new SKColorArgbDataBindingConverter(SKColorArgbDataBindingConverter.Channel.Green));
            RegisterDataBindingProperty(color => color.Blue, new SKColorArgbDataBindingConverter(SKColorArgbDataBindingConverter.Channel.Blue));
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
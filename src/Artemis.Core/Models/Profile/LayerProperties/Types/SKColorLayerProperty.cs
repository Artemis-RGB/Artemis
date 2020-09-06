using SkiaSharp;

namespace Artemis.Core
{
    /// <inheritdoc />
    public class SKColorLayerProperty : LayerProperty<SKColor>
    {
        internal SKColorLayerProperty()
        {
            RegisterDataBindingProperty(color => color.Alpha, new SKColorPartDataBindingConverter(SKColorPartDataBindingConverter.Channel.Alpha));
            RegisterDataBindingProperty(color => color.Red, new SKColorPartDataBindingConverter(SKColorPartDataBindingConverter.Channel.Red));
            RegisterDataBindingProperty(color => color.Green, new SKColorPartDataBindingConverter(SKColorPartDataBindingConverter.Channel.Green));
            RegisterDataBindingProperty(color => color.Blue, new SKColorPartDataBindingConverter(SKColorPartDataBindingConverter.Channel.Blue));
            RegisterDataBindingProperty(color => color.Hue, new SKColorPartDataBindingConverter(SKColorPartDataBindingConverter.Channel.Hue));
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
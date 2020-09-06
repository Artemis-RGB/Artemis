using System;
using SkiaSharp;

namespace Artemis.Core
{
    /// <inheritdoc />
    public class SKColorLayerProperty : LayerProperty<SKColor>
    {
        internal SKColorLayerProperty()
        {
            RegisterDataBindingProperty(color => color.Alpha, new SKColorDataBindingConverter(SKColorDataBindingConverter.Channel.Alpha));
            RegisterDataBindingProperty(color => color.Red, new SKColorDataBindingConverter(SKColorDataBindingConverter.Channel.Red));
            RegisterDataBindingProperty(color => color.Green, new SKColorDataBindingConverter(SKColorDataBindingConverter.Channel.Green));
            RegisterDataBindingProperty(color => color.Blue, new SKColorDataBindingConverter(SKColorDataBindingConverter.Channel.Blue));
            RegisterDataBindingProperty(color => color.Hue, new SKColorDataBindingConverter(SKColorDataBindingConverter.Channel.Hue));
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

    internal class SKColorDataBindingConverter : IDataBindingConverter
    {
        private readonly Channel _channel;

        public SKColorDataBindingConverter(Channel channel)
        {
            _channel = channel;
        }

        public BaseLayerProperty BaseLayerProperty { get; set; }

        public object Sum(object a, object b)
        {
            return (float) a + (float) b;
        }

        public object Interpolate(object a, object b, float progress)
        {
            var diff = (float) b - (float) a;
            return diff * progress;
        }

        public void ApplyValue(object value)
        {
            var property = (SKColorLayerProperty) BaseLayerProperty;
            switch (_channel)
            {
                case Channel.Alpha:
                    property.CurrentValue = property.CurrentValue.WithAlpha((byte) value);
                    break;
                case Channel.Red:
                    property.CurrentValue = property.CurrentValue.WithRed((byte) value);
                    break;
                case Channel.Green:
                    property.CurrentValue = property.CurrentValue.WithGreen((byte) value);
                    break;
                case Channel.Blue:
                    property.CurrentValue = property.CurrentValue.WithBlue((byte) value);
                    break;
                case Channel.Hue:
                    property.CurrentValue.ToHsv(out var h, out var s, out var v);
                    property.CurrentValue = SKColor.FromHsv((float) value, s, v);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public object GetValue()
        {
            var property = (SKColorLayerProperty) BaseLayerProperty;
            switch (_channel)
            {
                case Channel.Alpha:
                    return property.CurrentValue.Alpha;
                case Channel.Red:
                    return property.CurrentValue.Red;
                case Channel.Green:
                    return property.CurrentValue.Green;
                case Channel.Blue:
                    return property.CurrentValue.Blue;
                case Channel.Hue:
                    property.CurrentValue.ToHsv(out var h, out _, out _);
                    return h;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public enum Channel
        {
            Alpha,
            Red,
            Green,
            Blue,
            Hue
        }
    }
}
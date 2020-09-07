using System;
using SkiaSharp;

namespace Artemis.Core
{
    // This is internal because it's mainly a proof-of-concept
    internal class SKColorPartDataBindingConverter : DataBindingConverter
    {
        private readonly Channel _channel;

        public SKColorPartDataBindingConverter(Channel channel)
        {
            _channel = channel;

            SupportsSum = true;
            SupportsInterpolate = true;
            SupportedType = _channel switch
            {
                Channel.Alpha => typeof(byte),
                Channel.Red => typeof(byte),
                Channel.Green => typeof(byte),
                Channel.Blue => typeof(byte),
                Channel.Hue => typeof(float),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public override object Sum(object a, object b)
        {
            return (float) a + (float) b;
        }

        public override object Interpolate(object a, object b, double progress)
        {
            var diff = (float) b - (float) a;
            return diff * progress;
        }

        public override void ApplyValue(object value)
        {
            var property = (SKColorLayerProperty) DataBinding.LayerProperty;
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

        public override object GetValue()
        {
            var property = (SKColorLayerProperty) DataBinding.LayerProperty;
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
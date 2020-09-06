using System;
using SkiaSharp;

namespace Artemis.Core
{
    // This is internal because it's mainly a proof-of-concept
    internal class SKColorPartDataBindingConverter : IDataBindingConverter
    {
        private readonly Channel _channel;

        public SKColorPartDataBindingConverter(Channel channel)
        {
            _channel = channel;
        }

        // This depends on what channel was passed
        public Type SupportedType
        {
            get
            {
                switch (_channel)
                {
                    case Channel.Alpha:
                    case Channel.Red:
                    case Channel.Green:
                    case Channel.Blue:
                        return typeof(byte);
                    case Channel.Hue:
                        return typeof(float);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public bool SupportsSum => true;
        public bool SupportsInterpolate => true;

        public object Sum(BaseLayerProperty layerProperty, object a, object b)
        {
            return (float) a + (float) b;
        }

        public object Interpolate(BaseLayerProperty layerProperty, object a, object b, float progress)
        {
            var diff = (float) b - (float) a;
            return diff * progress;
        }

        public void ApplyValue(BaseLayerProperty layerProperty, object value)
        {
            var property = (SKColorLayerProperty) layerProperty;
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

        public object GetValue(BaseLayerProperty layerProperty)
        {
            var property = (SKColorLayerProperty) layerProperty;
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
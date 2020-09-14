using System;
using SkiaSharp;

namespace Artemis.Core
{
    // This is internal because it's mainly a proof-of-concept
    internal class SKColorArgbDataBindingConverter : DataBindingConverter<SKColor, byte>
    {
        private readonly Channel _channel;

        public SKColorArgbDataBindingConverter(Channel channel)
        {
            _channel = channel;

            SupportsSum = true;
            SupportsInterpolate = true;
        }

        public override byte Sum(byte a, byte b)
        {
            return ClampToByte(a + b);
        }

        public override byte Interpolate(byte a, byte b, double progress)
        {
            var diff = b - a;
            return ClampToByte(a + diff * progress);
        }

        public override void ApplyValue(byte value)
        {
            switch (_channel)
            {
                case Channel.Alpha:
                    DataBinding.LayerProperty.CurrentValue = DataBinding.LayerProperty.CurrentValue.WithAlpha(value);
                    break;
                case Channel.Red:
                    DataBinding.LayerProperty.CurrentValue = DataBinding.LayerProperty.CurrentValue.WithRed(value);
                    break;
                case Channel.Green:
                    DataBinding.LayerProperty.CurrentValue = DataBinding.LayerProperty.CurrentValue.WithGreen(value);
                    break;
                case Channel.Blue:
                    DataBinding.LayerProperty.CurrentValue = DataBinding.LayerProperty.CurrentValue.WithBlue(value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override byte ConvertFromObject(object source)
        {
            var saveValue = Convert.ToDouble(source);
            return ClampToByte(saveValue);
        }

        private static byte ClampToByte(double value)
        {
            return (byte) Math.Clamp(value, 0, 255);
        }

        public enum Channel
        {
            Alpha,
            Red,
            Green,
            Blue
        }
    }
}
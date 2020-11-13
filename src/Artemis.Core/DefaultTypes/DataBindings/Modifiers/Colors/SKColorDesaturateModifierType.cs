using System;
using SkiaSharp;

namespace Artemis.Core
{
    internal class SKColorDesaturateModifierType : DataBindingModifierType<SKColor, float>
    {
        public override string Name => "Desaturate";
        public override string Icon => "ImageMinus";
        public override string Description => "Desaturates the color by the amount in percent";

        public override SKColor Apply(SKColor currentValue, float parameterValue)
        {
            currentValue.ToHsl(out float h, out float s, out float l);
            s -= parameterValue;
            s = Math.Clamp(s, 0, 100);

            return SKColor.FromHsl(h, s, l, currentValue.Alpha);
        }
    }
}
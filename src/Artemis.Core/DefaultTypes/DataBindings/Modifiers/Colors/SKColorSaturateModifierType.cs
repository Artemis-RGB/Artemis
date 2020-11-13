using System;
using SkiaSharp;

namespace Artemis.Core
{
    internal class SKColorSaturateModifierType : DataBindingModifierType<SKColor, float>
    {
        public override string Name => "Saturate";
        public override string Icon => "ImagePlus";
        public override string Description => "Saturates the color by the amount in percent";

        public override SKColor Apply(SKColor currentValue, float parameterValue)
        {
            currentValue.ToHsv(out float h, out float s, out float v);
            s += parameterValue;
            s = Math.Clamp(s, 0, 100);
            
            return SKColor.FromHsv(h, s, v, currentValue.Alpha);
        }
    }
}
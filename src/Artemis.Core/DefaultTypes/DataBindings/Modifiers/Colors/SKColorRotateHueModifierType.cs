using System;
using System.Collections.Generic;
using SkiaSharp;

namespace Artemis.Core.DefaultTypes
{
    internal class SKColorRotateHueModifierType : DataBindingModifierType
    {
        public override IReadOnlyCollection<Type> CompatibleTypes => new List<Type> {typeof(SKColor)};
        public override Type ParameterType => typeof(float);

        public override string Name => "Rotate Hue by";
        public override string Icon => "RotateRight";
        public override string Description => "Rotates the hue of the color by the amount in degrees";

        public override object Apply(object currentValue, object parameterValue)
        {
            ((SKColor) currentValue).ToHsl(out float h, out float s, out float l);

            h += (float)parameterValue;

            return SKColor.FromHsl(h % 360, s, l);
        }
    }
}
using System;
using System.Collections.Generic;
using SkiaSharp;

namespace Artemis.Core.DefaultTypes
{
    internal class SKColorDarkenModifierType : DataBindingModifierType
    {
        public override IReadOnlyCollection<Type> CompatibleTypes => new List<Type> {typeof(SKColor)};
        public override Type ParameterType => typeof(float);

        public override string Name => "Darken by";
        public override string Icon => "CarLightDimmed";
        public override string Description => "Darkens the color by the amount in percent";

        public override object Apply(object currentValue, object parameterValue)
        {
            ((SKColor) currentValue).ToHsl(out float h, out float s, out float l);
            l *= (Convert.ToSingle(parameterValue) * -1 + 100f) / 100f;
            return SKColor.FromHsl(h, s, l);
        }
    }
}
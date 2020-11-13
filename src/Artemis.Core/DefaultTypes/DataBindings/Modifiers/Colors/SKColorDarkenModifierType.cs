using SkiaSharp;

namespace Artemis.Core
{
    internal class SKColorDarkenModifierType : DataBindingModifierType<SKColor, float>
    {
        public override string Name => "Darken by";
        public override string Icon => "CarLightDimmed";
        public override string Description => "Darkens the color by the amount in percent";

        public override SKColor Apply(SKColor currentValue, float parameterValue)
        {
            currentValue.ToHsl(out float h, out float s, out float l);
            l *= (parameterValue * -1 + 100f) / 100f;
            return SKColor.FromHsl(h, s, l);
        }
    }
}
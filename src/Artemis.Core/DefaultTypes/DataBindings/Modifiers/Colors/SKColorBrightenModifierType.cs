using SkiaSharp;

namespace Artemis.Core.DefaultTypes
{
    internal class SKColorBrightenModifierType : DataBindingModifierType<SKColor, float>
    {
        public override string Name => "Brighten by";
        public override string Icon => "CarLightHigh";
        public override string Description => "Brightens the color by the amount in percent";

        public override SKColor Apply(SKColor currentValue, float parameterValue)
        {
            currentValue.ToHsl(out float h, out float s, out float l);
            l *= (parameterValue + 100f) / 100f;
            return SKColor.FromHsl(h, s, l);
        }
    }
}
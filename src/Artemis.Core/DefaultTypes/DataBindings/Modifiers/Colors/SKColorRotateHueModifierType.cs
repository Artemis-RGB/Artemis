using SkiaSharp;

namespace Artemis.Core.DefaultTypes
{
    internal class SKColorRotateHueModifierType : DataBindingModifierType<SKColor, float>
    {
        public override string Name => "Rotate Hue by";
        public override string Icon => "RotateRight";
        public override string Description => "Rotates the hue of the color by the amount in degrees";

        public override SKColor Apply(SKColor currentValue, float parameterValue)
        {
            currentValue.ToHsl(out float h, out float s, out float l);
            h += parameterValue;
            return SKColor.FromHsl(h % 360, s, l);
        }
    }
}
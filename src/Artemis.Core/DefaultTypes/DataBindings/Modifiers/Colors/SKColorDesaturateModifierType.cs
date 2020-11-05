using SkiaSharp;

namespace Artemis.Core.DefaultTypes
{
    internal class SKColorDesaturateModifierType : DataBindingModifierType<SKColor, float>
    {
        public override string Name => "Desaturate";
        public override string Icon => "ImageMinus";
        public override string Description => "Desaturates the color by the amount in percent";

        public override SKColor Apply(SKColor currentValue, float parameterValue)
        {
            // TODO: Not so straightforward ^^
            currentValue.ToHsl(out float h, out float s, out float l);
            s *= (parameterValue * -1 + 100f) / 100f;
            return SKColor.FromHsl(h, s, l);
        }
    }
}
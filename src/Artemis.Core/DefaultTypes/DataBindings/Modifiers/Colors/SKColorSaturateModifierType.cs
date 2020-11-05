using SkiaSharp;

namespace Artemis.Core.DefaultTypes
{
    internal class SKColorSaturateModifierType : DataBindingModifierType<SKColor, float>
    {
        public override string Name => "Saturate";
        public override string Icon => "ImagePlus";
        public override string Description => "Saturates the color by the amount in percent";

        public override SKColor Apply(SKColor currentValue, float parameterValue)
        {
            // TODO: Not so straightforward ^^
            currentValue.ToHsl(out float h, out float s, out float l);
            return SKColor.FromHsl(h, parameterValue, l);
        }
    }
}
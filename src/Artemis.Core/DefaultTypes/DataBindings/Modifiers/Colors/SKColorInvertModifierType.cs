using SkiaSharp;

namespace Artemis.Core.DefaultTypes
{
    internal class SKColorInvertModifierType : DataBindingModifierType<SKColor>
    {
        public override string Name => "Invert color";
        public override string Icon => "InvertColors";
        public override string Description => "Inverts the color by rotating its hue by a 180°";

        public override SKColor Apply(SKColor currentValue)
        {
            currentValue.ToHsl(out float h, out float s, out float l);
            h += 180;
            return SKColor.FromHsl(h % 360, s, l);
        }
    }
}
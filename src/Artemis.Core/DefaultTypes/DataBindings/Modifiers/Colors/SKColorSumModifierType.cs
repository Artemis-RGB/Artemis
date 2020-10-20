using SkiaSharp;

namespace Artemis.Core.DefaultTypes
{
    internal class SKColorSumModifierType : DataBindingModifierType<SKColor, SKColor>
    {
        public override string Name => "Combine with";
        public override string Icon => "FormatColorFill";
        public override string Description => "Adds the two colors together";

        public override SKColor Apply(SKColor currentValue, SKColor parameterValue)
        {
            return currentValue.Sum(parameterValue);
        }
    }
}
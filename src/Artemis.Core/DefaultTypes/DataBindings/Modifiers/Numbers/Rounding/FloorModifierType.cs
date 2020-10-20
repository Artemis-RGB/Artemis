using System;

namespace Artemis.Core.DefaultTypes
{
    internal class FloorModifierType : DataBindingModifierType<double>
    {
        public override string Name => "Round down";
        public override string Icon => "ArrowDown";
        public override string Category => "Rounding";
        public override string Description => "Floors the input, rounding it down to the nearest whole number";

        public override double Apply(double currentValue)
        {
            return Math.Floor(currentValue);
        }
    }
}
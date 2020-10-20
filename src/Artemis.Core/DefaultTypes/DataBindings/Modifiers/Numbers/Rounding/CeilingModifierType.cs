using System;

namespace Artemis.Core.DefaultTypes
{
    internal class CeilingModifierType : DataBindingModifierType<double>
    {
        public override string Name => "Round up";
        public override string Icon => "ArrowUp";
        public override string Category => "Rounding";
        public override string Description => "Ceils the input, rounding it up to the nearest whole number";

        public override double Apply(double currentValue)
        {
            return Math.Ceiling(currentValue);
        }
    }
}
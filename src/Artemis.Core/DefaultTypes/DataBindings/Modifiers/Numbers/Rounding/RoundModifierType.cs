using System;

namespace Artemis.Core
{
    internal class RoundModifierType : DataBindingModifierType<double>
    {
        public override string Name => "Round";
        public override string Icon => "ArrowCollapse";
        public override string Category => "Rounding";
        public override string Description => "Rounds the input to the nearest whole number";

        public override double Apply(double currentValue)
        {
            return Math.Round(currentValue, MidpointRounding.AwayFromZero);
        }
    }
}
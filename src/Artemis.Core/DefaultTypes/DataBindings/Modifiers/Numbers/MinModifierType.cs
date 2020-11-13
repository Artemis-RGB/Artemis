using System;

namespace Artemis.Core
{
    internal class MinModifierType : DataBindingModifierType<double, double>
    {
        public override string Name => "Min";
        public override string Icon => "ChevronDownBoxOutline";
        public override string Category => "Advanced";
        public override string Description => "Keeps only the smallest of input value and parameter";

        public override double Apply(double currentValue, double parameterValue)
        {
            return Math.Min(currentValue, parameterValue);
        }
    }
}
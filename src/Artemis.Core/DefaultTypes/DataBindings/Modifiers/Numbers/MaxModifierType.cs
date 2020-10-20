using System;

namespace Artemis.Core.DefaultTypes
{
    internal class MaxModifierType : DataBindingModifierType<double, double>
    {
        public override string Name => "Max";
        public override string Icon => "ChevronUpBoxOutline";
        public override string Category => "Advanced";
        public override string Description => "Keeps only the largest of input value and parameter";

        public override double Apply(double currentValue, double parameterValue)
        {
            return Math.Max(currentValue, parameterValue);
        }
    }
}
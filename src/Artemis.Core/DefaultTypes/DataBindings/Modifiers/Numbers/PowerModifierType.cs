using System;

namespace Artemis.Core.DefaultTypes
{
    internal class PowerModifierType : DataBindingModifierType<double, double>
    {
        public override string Name => "Power";
        public override string Icon => "Exponent";
        public override string Category => "Advanced";
        public override string Description => "Raises the input value to the power of the parameter value";

        public override double Apply(double currentValue, double parameterValue)
        {
            return Math.Pow(currentValue, parameterValue);
        }
    }
}
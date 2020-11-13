using System;

namespace Artemis.Core
{
    internal class SecantModifierType : DataBindingModifierType<double>
    {
        public override string Name => "Secant";
        public override string Icon => null;
        public override string Category => "Trigonometry";
        public override string Description => "Treats the input as an angle and calculates the secant";

        public override double Apply(double currentValue)
        {
            return 1d / Math.Cos(currentValue);
        }
    }
}
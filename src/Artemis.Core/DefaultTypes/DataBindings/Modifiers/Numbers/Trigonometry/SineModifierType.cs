using System;

namespace Artemis.Core
{
    internal class SineModifierType : DataBindingModifierType<double>
    {
        public override string Name => "Sine";
        public override string Icon => "MathSin";
        public override string Category => "Trigonometry";
        public override string Description => "Treats the input as an angle and calculates the sine";

        public override double Apply(double currentValue)
        {
            return Math.Sin(currentValue);
        }
    }
}
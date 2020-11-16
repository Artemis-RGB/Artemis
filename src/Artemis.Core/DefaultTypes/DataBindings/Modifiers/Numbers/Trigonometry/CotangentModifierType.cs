using System;

namespace Artemis.Core
{
    internal class CotangentModifierType : DataBindingModifierType<double>
    {
        public override string Name => "Cotangent";
        public override string? Icon => null;
        public override string Category => "Trigonometry";
        public override string Description => "Treats the input as an angle and calculates the cotangent";

        public override double Apply(double currentValue)
        {
            return 1d / Math.Tan(currentValue);
        }
    }
}
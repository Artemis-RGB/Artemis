using System;

namespace Artemis.Core
{
    internal class SquareRootModifierType : DataBindingModifierType<double>
    {
        public override string Name => "Square root";
        public override string Icon => "SquareRoot";
        public override string Category => "Advanced";
        public override string Description => "Calculates square root of the input value";

        public override double Apply(double currentValue)
        {
            return Math.Sqrt(currentValue);
        }
    }
}
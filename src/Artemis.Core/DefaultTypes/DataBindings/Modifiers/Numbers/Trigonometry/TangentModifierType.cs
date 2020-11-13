using System;

namespace Artemis.Core
{
    internal class TangentModifierType : DataBindingModifierType<double>
    {
        public override string Name => "Tangent";
        public override string Icon => "MathTan";
        public override string Category => "Trigonometry";
        public override string Description => "Treats the input as an angle and calculates the tangent";

        public override double Apply(double currentValue)
        {
            return Math.Tan(currentValue);
        }
    }
}
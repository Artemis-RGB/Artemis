using System;

namespace Artemis.Core.DefaultTypes
{
    internal class CosineModifierType : DataBindingModifierType<double>
    {
        public override string Name => "Cosine";
        public override string Icon => "MathCos";
        public override string Category => "Trigonometry";
        public override string Description => "Treats the input as an angle and calculates the cosine";

        public override double Apply(double currentValue)
        {
            return Math.Cos(currentValue);
        }
    }
}
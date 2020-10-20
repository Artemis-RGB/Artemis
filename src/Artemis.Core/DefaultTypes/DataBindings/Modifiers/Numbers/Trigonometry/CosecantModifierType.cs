using System;

namespace Artemis.Core.DefaultTypes
{
    internal class CosecantModifierType : DataBindingModifierType<double>
    {
        public override string Name => "Cosecant";
        public override string Icon => null;
        public override string Category => "Trigonometry";
        public override string Description => "Treats the input as an angle and calculates the cosecant";

        public override double Apply(double currentValue)
        {
            return 1d / Math.Sin(currentValue);
        }
    }
}
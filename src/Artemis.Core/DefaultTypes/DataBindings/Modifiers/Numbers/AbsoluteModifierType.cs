using System;

namespace Artemis.Core.DefaultTypes
{
    internal class AbsoluteModifierType : DataBindingModifierType<double>
    {
        public override string Name => "Absolute";
        public override string Icon => "NumericPositive1";
        public override string Category => "Advanced";
        public override string Description => "Converts the input value to an absolute value";

        public override double Apply(double currentValue)
        {
            return Math.Abs(currentValue);
        }
    }
}
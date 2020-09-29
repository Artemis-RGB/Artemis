using System;
using System.Collections.Generic;

namespace Artemis.Core.DefaultTypes
{
    internal class SineModifierType : DataBindingModifierType
    {
        public override IReadOnlyCollection<Type> CompatibleTypes => Constants.NumberTypes;
        public override bool SupportsParameter => false;

        public override string Name => "Sine";
        public override string Icon => "MathSin";
        public override string Category => "Trigonometry";
        public override string Description => "Treats the input as an angle and calculates the sine";

        public override object Apply(object currentValue, object parameterValue)
        {
            return Math.Sin(Convert.ToSingle(currentValue));
        }
    }
}
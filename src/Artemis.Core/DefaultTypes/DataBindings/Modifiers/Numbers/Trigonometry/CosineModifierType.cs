using System;
using System.Collections.Generic;

namespace Artemis.Core.DefaultTypes
{
    internal class CosineModifierType : DataBindingModifierType
    {
        public override IReadOnlyCollection<Type> CompatibleTypes => Constants.NumberTypes;
        public override bool SupportsParameter => false;

        public override string Name => "Cosine";
        public override string Icon => "MathCos";
        public override string Category => "Trigonometry";
        public override string Description => "Treats the input as an angle and calculates the cosine";

        public override object Apply(object currentValue, object parameterValue)
        {
            return Math.Cos(Convert.ToSingle(currentValue));
        }
    }
}
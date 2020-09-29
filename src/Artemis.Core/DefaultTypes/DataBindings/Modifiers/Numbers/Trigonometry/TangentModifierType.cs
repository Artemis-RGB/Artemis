using System;
using System.Collections.Generic;

namespace Artemis.Core.DefaultTypes
{
    internal class TangentModifierType : DataBindingModifierType
    {
        public override IReadOnlyCollection<Type> CompatibleTypes => Constants.NumberTypes;
        public override bool SupportsParameter => false;

        public override string Name => "Tangent";
        public override string Icon => "MathTan";
        public override string Category => "Trigonometry";
        public override string Description => "Treats the input as an angle and calculates the tangent";

        public override object Apply(object currentValue, object parameterValue)
        {
            return Math.Tan(Convert.ToSingle(currentValue));
        }
    }
}
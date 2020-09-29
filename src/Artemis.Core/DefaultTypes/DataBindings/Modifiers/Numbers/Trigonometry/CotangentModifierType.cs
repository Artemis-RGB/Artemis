using System;
using System.Collections.Generic;

namespace Artemis.Core.DefaultTypes
{
    internal class CotangentModifierType : DataBindingModifierType
    {
        public override IReadOnlyCollection<Type> CompatibleTypes => Constants.NumberTypes;
        public override bool SupportsParameter => false;

        public override string Name => "Cotangent";
        public override string Icon => null;
        public override string Category => "Trigonometry";
        public override string Description => "Treats the input as an angle and calculates the cotangent";

        public override object Apply(object currentValue, object parameterValue)
        {
            return 1f / Math.Tan(Convert.ToSingle(currentValue));
        }
    }
}
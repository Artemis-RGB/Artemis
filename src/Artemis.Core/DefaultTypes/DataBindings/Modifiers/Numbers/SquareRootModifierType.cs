using System;
using System.Collections.Generic;

namespace Artemis.Core.DefaultTypes
{
    internal class SquareRootModifierType : DataBindingModifierType
    {
        public override IReadOnlyCollection<Type> CompatibleTypes => Constants.NumberTypes;
        public override bool SupportsParameter => false;

        public override string Name => "Square root";
        public override string Icon => "SquareRoot";
        public override string Category => "Advanced";
        public override string Description => "Calculates square root of the input value";

        public override object Apply(object currentValue, object parameterValue)
        {
            return Math.Sqrt(Convert.ToSingle(currentValue));
        }
    }
}
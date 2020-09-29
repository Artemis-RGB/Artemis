using System;
using System.Collections.Generic;

namespace Artemis.Core.DefaultTypes
{
    internal class AbsoluteModifierType : DataBindingModifierType
    {
        public override IReadOnlyCollection<Type> CompatibleTypes => Constants.NumberTypes;
        public override bool SupportsParameter => false;

        public override string Name => "Absolute";
        public override string Icon => "NumericPositive1";
        public override string Category => "Advanced";
        public override string Description => "Converts the input value to an absolute value";

        public override object Apply(object currentValue, object parameterValue)
        {
            return Math.Abs(Convert.ToSingle(currentValue));
        }
    }
}
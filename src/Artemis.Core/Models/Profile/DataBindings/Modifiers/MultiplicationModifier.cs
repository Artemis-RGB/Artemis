using System;
using System.Collections.Generic;

namespace Artemis.Core
{
    internal class MultiplicationModifierType : DataBindingModifierType
    {
        public MultiplicationModifierType()
        {
            PreferredParameterType = typeof(float);
        }

        public override IReadOnlyCollection<Type> CompatibleTypes => Constants.NumberTypes;

        public override string Description => "Multiply by";
        public override string Icon => "Close";

        public override object Apply(object currentValue, object parameterValue)
        {
            return Convert.ToSingle(currentValue) * Convert.ToSingle(parameterValue);
        }
    }
}
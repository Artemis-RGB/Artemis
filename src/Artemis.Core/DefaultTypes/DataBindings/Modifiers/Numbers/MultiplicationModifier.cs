using System;
using System.Collections.Generic;

namespace Artemis.Core.DefaultTypes
{
    internal class MultiplicationModifierType : DataBindingModifierType
    {
        public override IReadOnlyCollection<Type> CompatibleTypes => Constants.NumberTypes;
        public override Type ParameterType => typeof(float);

        public override string Name => "Multiply by";
        public override string Icon => "Close";

        public override object Apply(object currentValue, object parameterValue)
        {
            return Convert.ToSingle(currentValue) * Convert.ToSingle(parameterValue);
        }
    }
}
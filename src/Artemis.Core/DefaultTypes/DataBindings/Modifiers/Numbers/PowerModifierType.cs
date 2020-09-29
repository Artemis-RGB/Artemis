using System;
using System.Collections.Generic;

namespace Artemis.Core.DefaultTypes
{
    internal class PowerModifierType : DataBindingModifierType
    {
        public override IReadOnlyCollection<Type> CompatibleTypes => Constants.NumberTypes;

        public override string Name => "Power";
        public override string Icon => "Exponent";
        public override string Category => "Advanced";
        public override string Description => "Raises the input value to the power of the parameter value";

        public override object Apply(object currentValue, object parameterValue)
        {
            return Math.Pow(Convert.ToSingle(currentValue), Convert.ToSingle(parameterValue));
        }
    }
}
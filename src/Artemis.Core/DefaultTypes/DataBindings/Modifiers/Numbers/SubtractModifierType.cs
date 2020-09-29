using System;
using System.Collections.Generic;

namespace Artemis.Core.DefaultTypes
{
    internal class SubtractModifierType : DataBindingModifierType
    {
        public override IReadOnlyCollection<Type> CompatibleTypes => Constants.NumberTypes;

        public override string Name => "Subtract";
        public override string Icon => "Minus";

        public override object Apply(object currentValue, object parameterValue)
        {
            return Convert.ToSingle(currentValue) - Convert.ToSingle(parameterValue);
        }
    }
}
using System;
using System.Collections.Generic;

namespace Artemis.Core.DefaultTypes
{
    internal class SumModifierType : DataBindingModifierType
    {
        public override IReadOnlyCollection<Type> CompatibleTypes => Constants.NumberTypes;

        public override string Name => "Sum";
        public override string Icon => "Plus";

        public override object Apply(object currentValue, object parameterValue)
        {
            return Convert.ToSingle(currentValue) + Convert.ToSingle(parameterValue);
        }
    }
}
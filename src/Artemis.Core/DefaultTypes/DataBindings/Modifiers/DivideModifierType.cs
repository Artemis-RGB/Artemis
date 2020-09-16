using System;
using System.Collections.Generic;

namespace Artemis.Core.DefaultTypes
{
    internal class DivideModifierType : DataBindingModifierType
    {
        public DivideModifierType()
        {
            PreferredParameterType = typeof(float);
        }

        public override IReadOnlyCollection<Type> CompatibleTypes => Constants.NumberTypes;

        public override string Description => "Divide by";
        public override string Icon => "Divide";

        public override object Apply(object currentValue, object parameterValue)
        {
            var parameter = Convert.ToSingle(parameterValue);
            // Ye ye none of that
            if (parameter == 0)
                return 0;

            return Convert.ToSingle(currentValue) / parameter;
        }
    }
}
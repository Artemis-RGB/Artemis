using System;
using System.Collections.Generic;

namespace Artemis.Core.DefaultTypes
{
    internal class DivideModifierType : DataBindingModifierType
    {
        public override IReadOnlyCollection<Type> CompatibleTypes => Constants.NumberTypes;
        public override Type ParameterType => typeof(float);

        public override string Name => "Divide by";
        public override string Icon => "Divide";

        public override object Apply(object currentValue, object parameterValue)
        {
            float parameter = Convert.ToSingle(parameterValue);
            // Ye ye none of that
            if (parameter == 0)
                return 0;

            return Convert.ToSingle(currentValue) / parameter;
        }
    }
}
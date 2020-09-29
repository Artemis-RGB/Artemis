using System;
using System.Collections.Generic;

namespace Artemis.Core.DefaultTypes
{
    internal class PercentageOfModifierType : DataBindingModifierType
    {
        public override IReadOnlyCollection<Type> CompatibleTypes => Constants.NumberTypes;
        public override Type ParameterType => typeof(float);

        public override string Name => "Percentage of";
        public override string Icon => "Percent";
        public override string Description => "Calculates how much percent the parameter value is of the current value";

        public override object Apply(object currentValue, object parameterValue)
        {
            var parameter = Convert.ToSingle(parameterValue);
            // Ye ye none of that
            if (parameter == 0f)
                return 100f;

            return 100f / Convert.ToSingle(parameterValue) * Convert.ToSingle(currentValue);
        }
    }
}
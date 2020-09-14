using System;
using System.Collections.Generic;

namespace Artemis.Core
{
    internal class PercentageOfModifierType : DataBindingModifierType
    {
        public PercentageOfModifierType()
        {
            PreferredParameterType = typeof(float);
        }

        public override IReadOnlyCollection<Type> CompatibleTypes => Constants.NumberTypes;

        public override string Description => "Percentage of";
        public override string Icon => "Percent";

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
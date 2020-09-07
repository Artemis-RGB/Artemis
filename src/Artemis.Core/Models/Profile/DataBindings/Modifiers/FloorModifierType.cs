using System;
using System.Collections.Generic;

namespace Artemis.Core
{
    internal class FloorModifierType : DataBindingModifierType
    {
        public FloorModifierType()
        {
            SupportsParameter = false;
        }

        public override IReadOnlyCollection<Type> CompatibleTypes => Constants.NumberTypes;
        public override string Description => "Floor";
        public override string Icon => "ArrowDownDropCircleOutline";


        public override object Apply(object currentValue, object parameterValue)
        {
            var floatValue = Convert.ToSingle(currentValue);
            return Math.Floor(floatValue);
        }
    }
}
using System;
using System.Collections.Generic;

namespace Artemis.Core.DefaultTypes
{
    internal class LessThanConditionOperator : ConditionOperator
    {
        public override IReadOnlyCollection<Type> CompatibleTypes => Constants.NumberTypes;

        public override string Description => "Is less than";
        public override string Icon => "LessThan";

        public override bool Evaluate(object a, object b)
        {
            return Convert.ToSingle(a) < Convert.ToSingle(b);
        }
    }
}
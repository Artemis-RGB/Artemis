using System;
using System.Collections.Generic;

namespace Artemis.Core.DefaultTypes
{
    internal class GreaterThanConditionOperator : ConditionOperator
    {
        public override IReadOnlyCollection<Type> CompatibleTypes => Constants.NumberTypes;

        public override string Description => "Is greater than";
        public override string Icon => "GreaterThan";

        public override bool Evaluate(object a, object b)
        {
            return Convert.ToSingle(a) > Convert.ToSingle(b);
        }
    }
}
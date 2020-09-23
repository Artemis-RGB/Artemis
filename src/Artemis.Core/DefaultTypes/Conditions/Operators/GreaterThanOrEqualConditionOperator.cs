using System;
using System.Collections.Generic;

namespace Artemis.Core.DefaultTypes
{
    internal class GreaterThanOrEqualConditionOperator : ConditionOperator
    {
        public override IReadOnlyCollection<Type> CompatibleTypes => Constants.NumberTypes;

        public override string Description => "Is greater than or equal to";
        public override string Icon => "GreaterThanOrEqual";

        public override bool Evaluate(object a, object b)
        {
            return Convert.ToSingle(a) >= Convert.ToSingle(b);
        }
    }
}
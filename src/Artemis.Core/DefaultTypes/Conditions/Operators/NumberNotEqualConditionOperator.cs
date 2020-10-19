using System;

namespace Artemis.Core.DefaultTypes
{
    internal class NumberNotEqualConditionOperator : ConditionOperator<double, double>
    {
        public override string Description => "Does not equal";
        public override string Icon => "NotEqualVariant";

        public override bool Evaluate(double a, double b)
        {
            // Numbers can be tricky, an epsilon like this is close enough
            return Math.Abs(a - b) > 0.000001;
        }
    }
}
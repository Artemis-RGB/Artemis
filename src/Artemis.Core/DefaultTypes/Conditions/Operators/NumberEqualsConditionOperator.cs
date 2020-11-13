using System;

namespace Artemis.Core
{
    internal class NumberEqualsConditionOperator : ConditionOperator<double, double>
    {
        public override string Description => "Equals";
        public override string Icon => "Equal";

        public override bool Evaluate(double a, double b)
        {
            // Numbers can be tricky, an epsilon like this is close enough
            return Math.Abs(a - b) < 0.000001;
        }
    }
}
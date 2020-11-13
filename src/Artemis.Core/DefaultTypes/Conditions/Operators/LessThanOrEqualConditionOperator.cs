namespace Artemis.Core
{
    internal class LessThanOrEqualConditionOperator : ConditionOperator<double, double>
    {
        public override string Description => "Is less than or equal to";
        public override string Icon => "LessThanOrEqual";

        public override bool Evaluate(double a, double b)
        {
            return a <= b;
        }
    }
}
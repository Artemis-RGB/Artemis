namespace Artemis.Core.DefaultTypes
{
    internal class GreaterThanOrEqualConditionOperator : ConditionOperator<double, double>
    {
        public override string Description => "Is greater than or equal to";
        public override string Icon => "GreaterThanOrEqual";

        public override bool Evaluate(double a, double b)
        {
            return a >= b;
        }
    }
}
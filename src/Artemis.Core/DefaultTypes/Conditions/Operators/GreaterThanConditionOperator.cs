namespace Artemis.Core
{
    internal class GreaterThanConditionOperator : ConditionOperator<double, double>
    {
        public override string Description => "Is greater than";
        public override string Icon => "GreaterThan";

        public override bool Evaluate(double a, double b)
        {
            return a > b;
        }
    }
}
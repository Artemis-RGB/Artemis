namespace Artemis.Core
{
    internal class LessThanConditionOperator : ConditionOperator<double, double>
    {
        public override string Description => "Is less than";
        public override string Icon => "LessThan";

        public override bool Evaluate(double a, double b)
        {
            return a < b;
        }
    }
}
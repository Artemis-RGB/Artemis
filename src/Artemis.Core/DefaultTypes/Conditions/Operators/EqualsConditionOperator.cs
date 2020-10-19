namespace Artemis.Core.DefaultTypes
{
    internal class EqualsConditionOperator : ConditionOperator<object, object>
    {
        public override string Description => "Equals";
        public override string Icon => "Equal";

        public override bool Evaluate(object a, object b)
        {
            return Equals(a, b);
        }
    }
}
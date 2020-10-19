namespace Artemis.Core.DefaultTypes
{
    internal class NotEqualConditionOperator : ConditionOperator<object, object>
    {
        public override string Description => "Does not equal";
        public override string Icon => "NotEqualVariant";

        public override bool Evaluate(object a, object b)
        {
            return !Equals(a, b);
        }
    }
}
namespace Artemis.Core.DefaultTypes
{
    internal class NullConditionOperator : ConditionOperator<object>
    {
        public override string Description => "Is null";
        public override string Icon => "Null";

        public override bool Evaluate(object a)
        {
            return a == null;
        }
    }
}
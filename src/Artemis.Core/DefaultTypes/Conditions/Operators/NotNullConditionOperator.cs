namespace Artemis.Core.DefaultTypes
{
    internal class NotNullConditionOperator : ConditionOperator<object>
    {
        public override string Description => "Is not null";
        public override string Icon => "CheckboxMarkedCircleOutline";

        public override bool Evaluate(object a)
        {
            return a != null;
        }
    }
}
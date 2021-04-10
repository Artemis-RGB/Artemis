namespace Artemis.Core
{
    internal class StringNotNullConditionOperator : ConditionOperator<string>
    {
        public override string Description => "Is not null";
        public override string Icon => "CheckboxMarkedCircleOutline";

        public override bool Evaluate(string a)
        {
            return !string.IsNullOrWhiteSpace(a);
        }
    }
}
namespace Artemis.Core
{
    internal class StringNullConditionOperator : ConditionOperator<string>
    {
        public override string Description => "Is null";
        public override string Icon => "Null";

        public override bool Evaluate(string a)
        {
            return string.IsNullOrWhiteSpace(a);
        }
    }
}
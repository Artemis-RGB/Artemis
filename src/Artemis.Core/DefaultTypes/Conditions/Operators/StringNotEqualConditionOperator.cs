using System;

namespace Artemis.Core.DefaultTypes
{
    internal class StringNotEqualConditionOperator : ConditionOperator<string, string>
    {
        public override string Description => "Does not equal";
        public override string Icon => "NotEqualVariant";

        public override bool Evaluate(string a, string b)
        {
            return !string.Equals(a, b, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
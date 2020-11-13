using System;

namespace Artemis.Core
{
    internal class StringEqualsConditionOperator : ConditionOperator<string, string>
    {
        public override string Description => "Equals";
        public override string Icon => "Equal";

        public override bool Evaluate(string a, string b)
        {
            return string.Equals(a, b, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
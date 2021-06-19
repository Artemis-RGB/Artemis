using System;

namespace Artemis.Core
{
    internal class StringNotContainsConditionOperator : ConditionOperator<string, string>
    {
        public override string Description => "Does not contain";
        public override string Icon => "FormatStrikethrough";

        public override bool Evaluate(string a, string b)
        {
            return a != null && (b == null || !a.Contains(b, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
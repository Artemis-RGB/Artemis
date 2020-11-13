using System;

namespace Artemis.Core
{
    internal class StringStartsWithConditionOperator : ConditionOperator<string, string>
    {
        public override string Description => "Starts with";
        public override string Icon => "ContainStart";

        public override bool Evaluate(string a, string b)
        {
            return a != null && b != null && a.StartsWith(b, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
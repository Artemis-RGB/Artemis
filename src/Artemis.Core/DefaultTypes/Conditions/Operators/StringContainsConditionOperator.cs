using System;

namespace Artemis.Core.DefaultTypes
{
    internal class StringContainsConditionOperator : ConditionOperator<string, string>
    {
        public override string Description => "Contains";
        public override string Icon => "Contain";

        public override bool Evaluate(string a, string b)
        {
            return a != null && b != null && a.Contains(b, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
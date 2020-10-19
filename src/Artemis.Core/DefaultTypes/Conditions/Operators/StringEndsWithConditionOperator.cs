using System;

namespace Artemis.Core.DefaultTypes
{
    internal class StringEndsWithConditionOperator : ConditionOperator<string, string>
    {
        public override string Description => "Ends with";
        public override string Icon => "ContainEnd";

        public override bool Evaluate(string a, string b)
        {
            return a != null && b != null && a.EndsWith(b, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
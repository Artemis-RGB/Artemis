using System;

namespace Artemis.Core
{
    internal class EnumNotContainsConditionOperator : ConditionOperator<Enum, Enum>
    {
        public override string Description => "Does not contain";
        public override string Icon => "FormatStrikethrough";

        public override bool Evaluate(Enum a, Enum b)
        {
            return a != null && (b == null || !a.HasFlag(b));
        }
    }
}
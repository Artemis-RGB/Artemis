using System;

namespace Artemis.Core
{
    internal class EnumContainsConditionOperator : ConditionOperator<Enum, Enum>
    {
        public override string Description => "Contains";
        public override string Icon => "Contain";

        public override bool Evaluate(Enum a, Enum b)
        {
            return a != null && b != null && a.HasFlag(b);
        }
    }
}
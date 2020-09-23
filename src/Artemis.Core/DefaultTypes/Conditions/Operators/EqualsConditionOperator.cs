using System;
using System.Collections.Generic;

namespace Artemis.Core.DefaultTypes
{
    internal class EqualsConditionOperator : ConditionOperator
    {
        public override IReadOnlyCollection<Type> CompatibleTypes => new List<Type> {typeof(object)};

        public override string Description => "Equals";
        public override string Icon => "Equal";

        public override bool Evaluate(object a, object b)
        {
            return Equals(a, b);
        }
    }
}
using System;
using System.Collections.Generic;

namespace Artemis.Core.DefaultTypes
{
    internal class NullConditionOperator : ConditionOperator
    {
        public NullConditionOperator()
        {
            SupportsRightSide = false;
        }

        public override IReadOnlyCollection<Type> CompatibleTypes => new List<Type> {typeof(object)};

        public override string Description => "Is null";
        public override string Icon => "Null";

        public override bool Evaluate(object a, object b)
        {
            return a == null;
        }
    }
}
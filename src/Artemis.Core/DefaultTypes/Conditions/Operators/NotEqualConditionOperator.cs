using System;
using System.Collections.Generic;

namespace Artemis.Core.DefaultTypes
{
    internal class NotEqualConditionOperator : ConditionOperator
    {
        public override IReadOnlyCollection<Type> CompatibleTypes => new List<Type> {typeof(object)};

        public override string Description => "Does not equal";
        public override string Icon => "NotEqualVariant";

        public override bool Evaluate(object a, object b)
        {
            return !Equals(a, b);
        }
    }
}
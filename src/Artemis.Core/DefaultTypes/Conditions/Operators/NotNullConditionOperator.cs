using System;
using System.Collections.Generic;

namespace Artemis.Core.DefaultTypes
{
    internal class NotNullConditionOperator : ConditionOperator
    {
        public NotNullConditionOperator()
        {
            SupportsRightSide = false;
        }

        public override IReadOnlyCollection<Type> CompatibleTypes => new List<Type> {typeof(object)};

        public override string Description => "Is not null";
        public override string Icon => "CheckboxMarkedCircleOutline";

        public override bool Evaluate(object a, object b)
        {
            return a != null;
        }
    }
}
using System;
using System.Collections.Generic;

namespace Artemis.Core.DefaultTypes
{
    internal class StringEndsWithConditionOperator : ConditionOperator
    {
        public override IReadOnlyCollection<Type> CompatibleTypes => new List<Type> {typeof(string)};

        public override string Description => "Ends with";
        public override string Icon => "ContainEnd";

        public override bool Evaluate(object a, object b)
        {
            string aString = (string) a;
            string bString = (string) b;

            return bString != null && aString != null && aString.EndsWith(bString, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
using System;
using System.Collections.Generic;

namespace Artemis.Core.DefaultTypes
{
    internal class StringNotEqualConditionOperator : ConditionOperator
    {
        public override IReadOnlyCollection<Type> CompatibleTypes => new List<Type> {typeof(string)};

        public override string Description => "Does not equal";
        public override string Icon => "NotEqualVariant";

        public override bool Evaluate(object a, object b)
        {
            string aString = (string) a;
            string bString = (string) b;

            return !string.Equals(aString, bString, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
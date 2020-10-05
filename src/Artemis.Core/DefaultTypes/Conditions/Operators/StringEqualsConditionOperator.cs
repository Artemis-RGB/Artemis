using System;
using System.Collections.Generic;

namespace Artemis.Core.DefaultTypes
{
    internal class StringEqualsConditionOperator : ConditionOperator
    {
        public override IReadOnlyCollection<Type> CompatibleTypes => new List<Type> {typeof(string)};

        public override string Description => "Equals";
        public override string Icon => "Equal";

        public override bool Evaluate(object a, object b)
        {
            string aString = (string) a;
            string bString = (string) b;

            return string.Equals(aString, bString, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
using System;
using System.Collections.Generic;

namespace Artemis.Core.DefaultTypes
{
    internal class StringNotContainsConditionOperator : ConditionOperator
    {
        public override IReadOnlyCollection<Type> CompatibleTypes => new List<Type> {typeof(string)};

        public override string Description => "Does not contain";
        public override string Icon => "FormatStrikethrough";

        public override bool Evaluate(object a, object b)
        {
            var aString = (string) a;
            var bString = (string) b;

            return bString != null && aString != null && !aString.Contains(bString, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
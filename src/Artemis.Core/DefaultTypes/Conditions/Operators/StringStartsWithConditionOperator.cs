using System;
using System.Collections.Generic;

namespace Artemis.Core.DefaultTypes
{
    internal class StringStartsWithConditionOperator : ConditionOperator
    {
        public override IReadOnlyCollection<Type> CompatibleTypes => new List<Type> {typeof(string)};

        public override string Description => "Starts with";
        public override string Icon => "ContainStart";

        public override bool Evaluate(object a, object b)
        {
            var aString = (string) a;
            var bString = (string) b;

            return bString != null && aString != null && aString.StartsWith(bString, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
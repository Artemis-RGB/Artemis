using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Artemis.Core.DefaultTypes {
    internal class StringMatchesConditionOperator : ConditionOperator {
        public override IReadOnlyCollection<Type> CompatibleTypes => new List<Type> { typeof(string) };

        public override string Description => "Matches Regex";
        public override string Icon => "Regex";

        public override bool Evaluate(object a, object b) {
            string aString = (string)a;
            string bString = (string)b;

            return Regex.IsMatch(aString, bString);
        }
    }
}

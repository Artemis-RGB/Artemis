using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Artemis.Core.DefaultTypes {
    internal class StringMatchesConditionOperator : ConditionOperator {
        public override IReadOnlyCollection<Type> CompatibleTypes => new List<Type> { typeof(string) };

        public override string Description => "Matches Regex";
        public override string Icon => "Regex";

        public override bool Evaluate(object a, object b) {
            string text = a?.ToString() ?? string.Empty;
            string regex = b?.ToString() ?? string.Empty;

            // Ensures full match
            if (!regex.StartsWith("^")) regex = "^" + regex;
            if (!regex.EndsWith("$")) regex += "$";

            return Regex.IsMatch(text, regex);
        }
    }
}

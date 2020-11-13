using System.Text.RegularExpressions;

namespace Artemis.Core {
    internal class StringMatchesRegexConditionOperator : ConditionOperator<string, string>
    {
        public override string Description => "Matches Regex";
        public override string Icon => "Regex";

        public override bool Evaluate(string text, string regex)
        {
            // Ensures full match
            if (!regex.StartsWith("^"))
                regex = "^" + regex;
            if (!regex.EndsWith("$"))
                regex += "$";

            return Regex.IsMatch(text, regex);
        }
    }
}

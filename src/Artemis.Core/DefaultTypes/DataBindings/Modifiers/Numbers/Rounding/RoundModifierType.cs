using System;
using System.Collections.Generic;

namespace Artemis.Core.DefaultTypes
{
    internal class RoundModifierType : DataBindingModifierType
    {
        public override IReadOnlyCollection<Type> CompatibleTypes => Constants.NumberTypes;
        public override bool SupportsParameter => false;

        public override string Name => "Round";
        public override string Icon => "ArrowCollapse";
        public override string Category => "Rounding";
        public override string Description => "Rounds the input to the nearest whole number";

        public override object Apply(object currentValue, object parameterValue)
        {
            var floatValue = Convert.ToSingle(currentValue);
            return Math.Round(floatValue, MidpointRounding.AwayFromZero);
        }
    }
}
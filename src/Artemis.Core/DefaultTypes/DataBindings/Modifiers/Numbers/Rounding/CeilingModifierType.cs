using System;
using System.Collections.Generic;

namespace Artemis.Core.DefaultTypes
{
    internal class CeilingModifierType : DataBindingModifierType
    {
        public override IReadOnlyCollection<Type> CompatibleTypes => Constants.NumberTypes;
        public override bool SupportsParameter => false;

        public override string Name => "Round up";
        public override string Icon => "ArrowUp";
        public override string Category => "Rounding";
        public override string Description => "Ceils the input, rounding it up to the nearest whole number";

        public override object Apply(object currentValue, object parameterValue)
        {
            float floatValue = Convert.ToSingle(currentValue);
            return Math.Ceiling(floatValue);
        }
    }
}
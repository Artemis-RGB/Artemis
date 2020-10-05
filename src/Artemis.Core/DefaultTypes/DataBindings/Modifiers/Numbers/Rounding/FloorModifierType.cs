using System;
using System.Collections.Generic;

namespace Artemis.Core.DefaultTypes
{
    internal class FloorModifierType : DataBindingModifierType
    {
        public override IReadOnlyCollection<Type> CompatibleTypes => Constants.NumberTypes;
        public override bool SupportsParameter => false;

        public override string Name => "Round down";
        public override string Icon => "ArrowDown";
        public override string Category => "Rounding";
        public override string Description => "Floors the input, rounding it down to the nearest whole number";

        public override object Apply(object currentValue, object parameterValue)
        {
            float floatValue = Convert.ToSingle(currentValue);
            return Math.Floor(floatValue);
        }
    }
}
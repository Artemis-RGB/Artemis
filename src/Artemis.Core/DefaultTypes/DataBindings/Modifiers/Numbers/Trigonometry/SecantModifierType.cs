using System;
using System.Collections.Generic;

namespace Artemis.Core.DefaultTypes
{
    internal class SecantModifierType : DataBindingModifierType
    {
        public override IReadOnlyCollection<Type> CompatibleTypes => Constants.NumberTypes;
        public override bool SupportsParameter => false;

        public override string Name => "Secant";
        public override string Icon => null;
        public override string Category => "Trigonometry";
        public override string Description => "Treats the input as an angle and calculates the secant";

        public override object Apply(object currentValue, object parameterValue)
        {
            return 1f / Math.Cos(Convert.ToSingle(currentValue));
        }
    }
}
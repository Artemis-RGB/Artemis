using System;
using System.Collections.Generic;

namespace Artemis.Core.DefaultTypes
{
    internal class CosecantModifierType : DataBindingModifierType
    {
        public override IReadOnlyCollection<Type> CompatibleTypes => Constants.NumberTypes;
        public override bool SupportsParameter => false;

        public override string Name => "Cosecant";
        public override string Icon => null;
        public override string Category => "Trigonometry";
        public override string Description => "Treats the input as an angle and calculates the cosecant";

        public override object Apply(object currentValue, object parameterValue)
        {
            return 1f / Math.Sin(Convert.ToSingle(currentValue));
        }
    }
}
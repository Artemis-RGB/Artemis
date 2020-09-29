using System;
using System.Collections.Generic;

namespace Artemis.Core.DefaultTypes
{
    internal class MinModifierType : DataBindingModifierType
    {
        public override IReadOnlyCollection<Type> CompatibleTypes => Constants.NumberTypes;

        public override string Name => "Min";
        public override string Icon => "ChevronDownBoxOutline";
        public override string Category => "Advanced";
        public override string Description => "Keeps only the smallest of input value and parameter";

        public override object Apply(object currentValue, object parameterValue)
        {
            return Math.Min(Convert.ToSingle(currentValue), Convert.ToSingle(parameterValue));
        }
    }
}
using System;
using System.Collections.Generic;

namespace Artemis.Core.DefaultTypes
{
    internal class MaxModifierType : DataBindingModifierType
    {
        public override IReadOnlyCollection<Type> CompatibleTypes => Constants.NumberTypes;

        public override string Name => "Max";
        public override string Icon => "ChevronUpBoxOutline";
        public override string Category => "Advanced";
        public override string Description => "Keeps only the largest of input value and parameter";

        public override object Apply(object currentValue, object parameterValue)
        {
            return Math.Max(Convert.ToSingle(currentValue), Convert.ToSingle(parameterValue));
        }
    }
}
using System;
using System.Collections.Generic;

namespace Artemis.Core.DefaultTypes
{
    internal class ModuloModifierType : DataBindingModifierType
    {
        public override IReadOnlyCollection<Type> CompatibleTypes => Constants.NumberTypes;

        public override string Name => "Modulo";
        public override string Icon => "Stairs";
        public override string Category => "Advanced";
        public override string Description => "Calculates the remained of the division between the input value and the parameter";

        public override object Apply(object currentValue, object parameterValue)
        {
            return Convert.ToSingle(currentValue) % Convert.ToSingle(parameterValue);
        }
    }
}
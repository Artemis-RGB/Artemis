using System;
using System.Collections.Generic;
using SkiaSharp;

namespace Artemis.Core.DefaultTypes
{
    internal class SKColorSumModifierType : DataBindingModifierType
    {
        public override IReadOnlyCollection<Type> CompatibleTypes => new List<Type> {typeof(SKColor)};
        public override string Description => "Combine with";
        public override string Icon => "FormatColorFill";

        public override object Apply(object currentValue, object parameterValue)
        {
            return ((SKColor) currentValue).Sum((SKColor) parameterValue);
        }
    }
}
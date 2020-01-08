using System;
using System.Collections.Generic;
using Artemis.Core.Models.Profile.LayerProperties;

namespace Artemis.Core.Models.Profile.KeyframeEngines
{
    /// <inheritdoc />
    public class IntKeyframeEngine : KeyframeEngine
    {
        public sealed override List<Type> CompatibleTypes { get; } = new List<Type> {typeof(int)};

        public override object GetCurrentValue()
        {
            // Nothing fancy for now, just return the base value
            return ((LayerProperty<int>) LayerProperty).Value;
        }
    }
}
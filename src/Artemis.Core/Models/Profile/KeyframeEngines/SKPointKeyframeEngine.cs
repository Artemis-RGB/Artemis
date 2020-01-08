using System;
using System.Collections.Generic;
using Artemis.Core.Models.Profile.LayerProperties;
using SkiaSharp;

namespace Artemis.Core.Models.Profile.KeyframeEngines
{
    /// <inheritdoc />
    public class SKPointKeyframeEngine : KeyframeEngine
    {
        public sealed override List<Type> CompatibleTypes { get; } = new List<Type> {typeof(SKPoint)};

        public override object GetCurrentValue()
        {
            // Nothing fancy for now, just return the base value
            return ((LayerProperty<SKPoint>) LayerProperty).Value;
        }
    }
}
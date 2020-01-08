using System;
using System.Collections.Generic;
using Artemis.Core.Models.Profile.LayerProperties;
using SkiaSharp;

namespace Artemis.Core.Models.Profile.KeyframeEngines
{
    /// <inheritdoc />
    public class SKSizeKeyframeEngine : KeyframeEngine
    {
        public sealed override List<Type> CompatibleTypes { get; } = new List<Type> {typeof(SKSize)};

        public override object GetCurrentValue()
        {
            // Nothing fancy for now, just return the base value
            return ((LayerProperty<SKSize>) LayerProperty).Value;
        }
    }
}
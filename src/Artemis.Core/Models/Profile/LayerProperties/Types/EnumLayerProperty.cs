using System;
using Artemis.Core.Exceptions;

namespace Artemis.Core.Models.Profile.LayerProperties.Types
{
    /// <inheritdoc />
    public class EnumLayerProperty<T> : LayerProperty<T> where T : Enum
    {
        public EnumLayerProperty()
        {
            KeyframesSupported = false;
        }

        protected override void UpdateCurrentValue(float keyframeProgress, float keyframeProgressEased)
        {
            throw new ArtemisCoreException("Enum properties do not support keyframes.");
        }

        public static implicit operator T(EnumLayerProperty<T> p) => p.CurrentValue;
        public static implicit operator int(EnumLayerProperty<T> p) => Convert.ToInt32(p.CurrentValue);
    }
}
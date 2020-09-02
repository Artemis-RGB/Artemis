using System;

namespace Artemis.Core
{
    /// <inheritdoc />
    public class EnumLayerProperty<T> : LayerProperty<T> where T : Enum
    {
        public EnumLayerProperty()
        {
            KeyframesSupported = false;
            DataBindingsSupported = false;
        }

        public static implicit operator T(EnumLayerProperty<T> p)
        {
            return p.CurrentValue;
        }

        public static implicit operator int(EnumLayerProperty<T> p)
        {
            return Convert.ToInt32(p.CurrentValue);
        }

        protected override void UpdateCurrentValue(float keyframeProgress, float keyframeProgressEased)
        {
            throw new ArtemisCoreException("Enum properties do not support keyframes.");
        }
    }
}
using System;

namespace Artemis.Core
{
    /// <inheritdoc />
    public class EnumLayerProperty<T> : LayerProperty<T> where T : Enum
    {
        internal EnumLayerProperty()
        {
            KeyframesSupported = false;
            DataBindingsSupported = false;
        }

        /// <summary>
        ///     Implicitly converts an <see cref="EnumLayerProperty{T}" /> to a <typeparamref name="T"/>
        /// </summary>
        public static implicit operator T(EnumLayerProperty<T> p)
        {
            return p.CurrentValue;
        }

        /// <summary>
        ///     Implicitly converts an <see cref="EnumLayerProperty{T}" /> to an <see cref="int" />
        /// </summary>
        public static implicit operator int(EnumLayerProperty<T> p)
        {
            return Convert.ToInt32(p.CurrentValue);
        }

        /// <inheritdoc />
        protected override void UpdateCurrentValue(float keyframeProgress, float keyframeProgressEased)
        {
            throw new ArtemisCoreException("Enum properties do not support keyframes.");
        }
    }
}
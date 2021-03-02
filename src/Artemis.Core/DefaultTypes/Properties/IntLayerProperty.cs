using System;

namespace Artemis.Core
{
    /// <inheritdoc />
    public class IntLayerProperty : LayerProperty<int>
    {
        internal IntLayerProperty()
        {
            RegisterDataBindingProperty(value => value, (_, newValue) => CurrentValue = newValue, new IntDataBindingConverter(), "Value");
        }

        /// <summary>
        ///     Implicitly converts an <see cref="IntLayerProperty" /> to an <see cref="int" />
        /// </summary>
        public static implicit operator int(IntLayerProperty p)
        {
            return p.CurrentValue;
        }

        /// <summary>
        ///     Implicitly converts an <see cref="IntLayerProperty" /> to a <see cref="float" />
        /// </summary>
        public static implicit operator float(IntLayerProperty p)
        {
            return p.CurrentValue;
        }

        /// <summary>
        ///     Implicitly converts an <see cref="IntLayerProperty" /> to a <see cref="double" />
        /// </summary>
        public static implicit operator double(IntLayerProperty p)
        {
            return p.CurrentValue;
        }

        /// <inheritdoc />
        protected override void UpdateCurrentValue(float keyframeProgress, float keyframeProgressEased)
        {
            int diff = NextKeyframe!.Value - CurrentKeyframe!.Value;
            CurrentValue = (int) Math.Round(CurrentKeyframe!.Value + diff * keyframeProgressEased, MidpointRounding.AwayFromZero);
        }
    }
}
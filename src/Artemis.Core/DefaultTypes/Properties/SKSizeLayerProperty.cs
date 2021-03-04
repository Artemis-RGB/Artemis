﻿using SkiaSharp;

namespace Artemis.Core
{
    /// <inheritdoc />
    public class SKSizeLayerProperty : LayerProperty<SKSize>
    {
        internal SKSizeLayerProperty()
        {
            RegisterDataBindingProperty(() => CurrentValue.Width, (value) => CurrentValue = new SKSize(value, CurrentValue.Height), new FloatDataBindingConverter<SKSize>(), "Width");
            RegisterDataBindingProperty(() => CurrentValue.Height, (value) => CurrentValue = new SKSize(CurrentValue.Width, value), new FloatDataBindingConverter<SKSize>(), "Height");
        }

        /// <summary>
        ///     Implicitly converts an <see cref="SKSizeLayerProperty" /> to an <see cref="SKSize" />
        /// </summary>
        public static implicit operator SKSize(SKSizeLayerProperty p)
        {
            return p.CurrentValue;
        }

        /// <inheritdoc />
        protected override void UpdateCurrentValue(float keyframeProgress, float keyframeProgressEased)
        {
            float widthDiff = NextKeyframe!.Value.Width - CurrentKeyframe!.Value.Width;
            float heightDiff = NextKeyframe!.Value.Height - CurrentKeyframe!.Value.Height;
            CurrentValue = new SKSize(CurrentKeyframe!.Value.Width + widthDiff * keyframeProgressEased, CurrentKeyframe!.Value.Height + heightDiff * keyframeProgressEased);
        }
    }
}
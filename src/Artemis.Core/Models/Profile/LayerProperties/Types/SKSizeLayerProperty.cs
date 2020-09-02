using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SkiaSharp;

namespace Artemis.Core
{
    /// <inheritdoc />
    public class SKSizeLayerProperty : LayerProperty<SKSize>
    {
        internal SKSizeLayerProperty()
        {
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
            var widthDiff = NextKeyframe.Value.Width - CurrentKeyframe.Value.Width;
            var heightDiff = NextKeyframe.Value.Height - CurrentKeyframe.Value.Height;
            CurrentValue = new SKSize(CurrentKeyframe.Value.Width + widthDiff * keyframeProgressEased, CurrentKeyframe.Value.Height + heightDiff * keyframeProgressEased);
        }

        /// <inheritdoc />
        public override List<PropertyInfo> GetDataBindingProperties()
        {
            return typeof(SKSize).GetProperties().Where(p => p.CanWrite).ToList();
        }

        /// <inheritdoc />
        protected override void ApplyDataBinding(DataBinding dataBinding)
        {
            if (dataBinding.TargetProperty.Name == nameof(CurrentValue.Height))
                CurrentValue = new SKSize(CurrentValue.Width, (float) dataBinding.GetValue(BaseValue));
            else if (dataBinding.TargetProperty.Name == nameof(CurrentValue.Width))
                CurrentValue = new SKSize((float) dataBinding.GetValue(BaseValue), CurrentValue.Width);
        }
    }
}
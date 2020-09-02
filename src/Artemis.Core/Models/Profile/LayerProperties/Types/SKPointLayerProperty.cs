using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SkiaSharp;

namespace Artemis.Core
{
    /// <inheritdoc />
    public class SKPointLayerProperty : LayerProperty<SKPoint>
    {
        internal SKPointLayerProperty()
        {
        }
        
        /// <summary>
        ///     Implicitly converts an <see cref="SKPointLayerProperty" /> to an <see cref="SKPoint" />
        /// </summary>
        public static implicit operator SKPoint(SKPointLayerProperty p)
        {
            return p.CurrentValue;
        }

        /// <inheritdoc />
        protected override void UpdateCurrentValue(float keyframeProgress, float keyframeProgressEased)
        {
            var xDiff = NextKeyframe.Value.X - CurrentKeyframe.Value.X;
            var yDiff = NextKeyframe.Value.Y - CurrentKeyframe.Value.Y;
            CurrentValue = new SKPoint(CurrentKeyframe.Value.X + xDiff * keyframeProgressEased, CurrentKeyframe.Value.Y + yDiff * keyframeProgressEased);
        }

        /// <inheritdoc />
        public override List<PropertyInfo> GetDataBindingProperties()
        {
            return typeof(SKPoint).GetProperties().Where(p => p.CanWrite).ToList();
        }

        /// <inheritdoc />
        protected override void ApplyDataBinding(DataBinding dataBinding)
        {
            if (dataBinding.TargetProperty.Name == nameof(CurrentValue.X))
                CurrentValue = new SKPoint((float) dataBinding.GetValue(BaseValue), CurrentValue.Y);
            else if (dataBinding.TargetProperty.Name == nameof(CurrentValue.Y))
                CurrentValue = new SKPoint(CurrentValue.X, (float) dataBinding.GetValue(BaseValue));
        }
    }
}
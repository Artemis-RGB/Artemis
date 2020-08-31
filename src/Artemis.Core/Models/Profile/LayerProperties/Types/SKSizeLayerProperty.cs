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

        public static implicit operator SKSize(SKSizeLayerProperty p)
        {
            return p.CurrentValue;
        }

        public override List<PropertyInfo> GetDataBindingProperties()
        {
            return typeof(SKSize).GetProperties().Where(p => p.CanWrite).ToList();
        }

        protected override void UpdateCurrentValue(float keyframeProgress, float keyframeProgressEased)
        {
            var widthDiff = NextKeyframe.Value.Width - CurrentKeyframe.Value.Width;
            var heightDiff = NextKeyframe.Value.Height - CurrentKeyframe.Value.Height;
            CurrentValue = new SKSize(CurrentKeyframe.Value.Width + widthDiff * keyframeProgressEased, CurrentKeyframe.Value.Height + heightDiff * keyframeProgressEased);
        }
    }
}
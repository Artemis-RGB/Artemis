using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SkiaSharp;

namespace Artemis.Core
{
    /// <inheritdoc />
    public class SKColorLayerProperty : LayerProperty<SKColor>
    {
        internal SKColorLayerProperty()
        {
        }

        /// <summary>
        ///  Implicitly converts an <see cref="SKColorLayerProperty" /> to an <see cref="SKColor" />
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static implicit operator SKColor(SKColorLayerProperty p)
        {
            return p.CurrentValue;
        }

        /// <inheritdoc />
        protected override void UpdateCurrentValue(float keyframeProgress, float keyframeProgressEased)
        {
            CurrentValue = CurrentKeyframe.Value.Interpolate(NextKeyframe.Value, keyframeProgressEased);
        }

        /// <inheritdoc />
        public override List<PropertyInfo> GetDataBindingProperties()
        {
            return typeof(SKColor).GetProperties().ToList();
        }

        /// <inheritdoc />
        protected override void ApplyDataBinding(DataBinding dataBinding)
        {
            if (dataBinding.TargetProperty.Name == nameof(CurrentValue.Alpha))
                CurrentValue = CurrentValue.WithAlpha((byte) dataBinding.GetValue(BaseValue.Alpha));
            else if (dataBinding.TargetProperty.Name == nameof(CurrentValue.Red))
                CurrentValue = CurrentValue.WithRed((byte) dataBinding.GetValue(BaseValue.Red));
            else if (dataBinding.TargetProperty.Name == nameof(CurrentValue.Green))
                CurrentValue = CurrentValue.WithGreen((byte) dataBinding.GetValue(BaseValue.Green));
            else if (dataBinding.TargetProperty.Name == nameof(CurrentValue.Blue))
                CurrentValue = CurrentValue.WithBlue((byte) dataBinding.GetValue(BaseValue.Blue));
            else if (dataBinding.TargetProperty.Name == nameof(CurrentValue.Hue))
            {
                CurrentValue.ToHsv(out var h, out var s, out var v);
                CurrentValue = SKColor.FromHsv((float) dataBinding.GetValue(h), s, v);
            }
        }
    }
}
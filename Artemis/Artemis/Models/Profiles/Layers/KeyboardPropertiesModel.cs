using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace Artemis.Models.Profiles.Layers
{
    public class KeyboardPropertiesModel : LayerPropertiesModel
    {
        public KeyboardPropertiesModel()
        {
            DynamicProperties = new List<DynamicPropertiesModel>();
        }

        public string GifFile { get; set; }
        public List<DynamicPropertiesModel> DynamicProperties { get; set; }

        public Rect GetRect(int scale = 4)
        {
            return new Rect(X*scale, Y*scale, Width*scale, Height*scale);
        }

        //    {
        //    var applied = new AppliedProperties
        //{

        //public override AppliedProperties GetAppliedProperties(IDataModel dataModel, bool ignoreDynamic = false)
        //        X = X,
        //        Y = Y,
        //        Width = Width,
        //        Height = Height,
        //        Opacity = Opacity,
        //        Brush = Brush.CloneCurrentValue()
        //    };

        //    if (ignoreDynamic)
        //        return applied;

        //    foreach (var dynamicProperty in DynamicProperties)
        //        dynamicProperty.ApplyProperty(dataModel, ref applied);

        //    if (Math.Abs(applied.Opacity - 1) > 0.001)
        //    {
        //        applied.Brush = Brush.CloneCurrentValue();
        //        applied.Brush.Opacity = applied.Opacity;
        //    }

        //    return applied;
        //}
    }

    public enum LayerAnimation
    {
        [Description("None")] None,
        [Description("Slide left")] SlideLeft,
        [Description("Slide right")] SlideRight,
        [Description("Slide up")] SlideUp,
        [Description("Slide down")] SlideDown,
        [Description("Grow")] Grow,
        [Description("Pulse")] Pulse
    }
}
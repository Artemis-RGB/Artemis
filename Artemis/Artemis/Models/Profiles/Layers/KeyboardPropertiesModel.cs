using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Xml.Serialization;
using Artemis.Models.Interfaces;

namespace Artemis.Models.Profiles.Layers
{
    public class KeyboardPropertiesModel : LayerPropertiesModel
    {
        public KeyboardPropertiesModel()
        {
            DynamicProperties = new List<DynamicPropertiesModel>();
        }

        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Opacity { get; set; }
        public bool Contain { get; set; }
        public LayerAnimation Animation { get; set; }
        public double AnimationSpeed { get; set; }
        public string GifFile { get; set; }
        public List<DynamicPropertiesModel> DynamicProperties { get; set; }

        [XmlIgnore]
        public double AnimationProgress { get; set; }

        public Rect GetRect(int scale = 4)
        {
            return new Rect(X*scale, Y*scale, Width*scale, Height*scale);
        }

        public override AppliedProperties GetAppliedProperties(IDataModel dataModel, bool ignoreDynamic = false)
        {
            var applied = new AppliedProperties
            {
                X = X,
                Y = Y,
                Width = Width,
                Height = Height,
                Opacity = Opacity,
                Brush = Brush.CloneCurrentValue()
            };

            if (ignoreDynamic)
                return applied;

            foreach (var dynamicProperty in DynamicProperties)
                dynamicProperty.ApplyProperty(dataModel, ref applied);

            if (Math.Abs(applied.Opacity - 1) > 0.001)
            {
                applied.Brush = Brush.CloneCurrentValue();
                applied.Brush.Opacity = applied.Opacity;
            }

            return applied;
        }
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
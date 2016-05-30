using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Xml.Serialization;
using Artemis.Models.Interfaces;
using Artemis.Utilities;

namespace Artemis.Models.Profiles.Properties
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

        public override LayerPropertiesModel GetAppliedProperties(IGameDataModel dataModel)
        {
            var properties = GeneralHelpers.Clone(this);

            foreach (var dynamicProperty in properties.DynamicProperties)
                dynamicProperty.ApplyProperty(dataModel, properties);
            properties.Brush.Opacity = Opacity;

            return properties;
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
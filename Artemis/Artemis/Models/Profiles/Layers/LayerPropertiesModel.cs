using System.Collections.Generic;
using System.Windows.Media;
using System.Xml.Serialization;
using Artemis.Models.Interfaces;

namespace Artemis.Models.Profiles.Layers
{
    [XmlInclude(typeof(SolidColorBrush))]
    [XmlInclude(typeof(LinearGradientBrush))]
    [XmlInclude(typeof(RadialGradientBrush))]
    [XmlInclude(typeof(MatrixTransform))]
    [XmlInclude(typeof(KeyboardPropertiesModel))]
    [XmlInclude(typeof(MousePropertiesModel))]
    [XmlInclude(typeof(HeadsetPropertiesModel))]
    [XmlInclude(typeof(FolderPropertiesModel))]
    public abstract class LayerPropertiesModel
    {
        private Brush _brush;

        public List<LayerConditionModel> Conditions { get; set; } = new List<LayerConditionModel>();
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public bool Contain { get; set; }
        public double Opacity { get; set; }
        public LayerAnimation Animation { get; set; }
        public double AnimationSpeed { get; set; }

        [XmlIgnore]
        public double AnimationProgress { get; set; }

        public Brush Brush
        {
            get { return _brush; }
            set
            {
                if (value == null)
                {
                    _brush = null;
                    return;
                }

                if (value.IsFrozen)
                {
                    _brush = value;
                    return;
                }

                // Clone the brush off of the UI thread and freeze it
                var cloned = value.Dispatcher.Invoke(value.CloneCurrentValue);
                cloned.Freeze();
                _brush = cloned;
            }
        }
    }
}
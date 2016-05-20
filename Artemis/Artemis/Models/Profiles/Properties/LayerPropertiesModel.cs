using System.Collections.Generic;
using System.Windows.Media;
using System.Xml.Serialization;
using Artemis.Models.Interfaces;

namespace Artemis.Models.Profiles.Properties
{
    [XmlInclude(typeof(SolidColorBrush))]
    [XmlInclude(typeof(LinearGradientBrush))]
    [XmlInclude(typeof(RadialGradientBrush))]
    [XmlInclude(typeof(MatrixTransform))]
    [XmlInclude(typeof(KeyboardPropertiesModel))]
    [XmlInclude(typeof(MousePropertiesModel))]
    [XmlInclude(typeof(HeadsetPropertiesModel))]
    [XmlInclude(typeof(GenericPropertiesModel))]
    public abstract class LayerPropertiesModel
    {
        protected LayerPropertiesModel()
        {
            Conditions = new List<LayerConditionModel>();
        }

        public abstract LayerPropertiesModel GetAppliedProperties(IGameDataModel dataModel);

        public List<LayerConditionModel> Conditions { get; set; }
        public Brush Brush { get; set; }
    }
}
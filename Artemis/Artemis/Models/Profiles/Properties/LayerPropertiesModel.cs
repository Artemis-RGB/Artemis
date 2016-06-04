using System.Collections.Generic;
using System.Windows.Media;
using System.Xml.Serialization;
using Artemis.Models.Interfaces;
using Artemis.Utilities;

namespace Artemis.Models.Profiles.Properties
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

        protected LayerPropertiesModel()
        {
            Conditions = new List<LayerConditionModel>();
        }

        public abstract LayerPropertiesModel GetAppliedProperties(IGameDataModel dataModel);

        public List<LayerConditionModel> Conditions { get; set; }

        public Brush Brush
        {
            get { return _brush; }
            set
            {
                var cloned = value.Dispatcher.Invoke(() => GeneralHelpers.CloneAlt(value));
                cloned.Freeze();
                _brush = cloned;
            }
        }
    }
}
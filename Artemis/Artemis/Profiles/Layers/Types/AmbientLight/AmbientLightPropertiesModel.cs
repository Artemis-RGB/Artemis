using System.Windows.Media;
using Artemis.Profiles.Layers.Models;
using Newtonsoft.Json;

namespace Artemis.Profiles.Layers.Types.AmbientLight
{
    public class AmbientLightPropertiesModel : LayerPropertiesModel
    {
        #region Properties & Fields

        //HACK DarthAffe 30.10.2016: The 'normal' Brush-Property destoys the profile since Drawing-Brushes cannot be deserialized.
        [JsonIgnore]
        public Brush AmbientLightBrush { get; set; }

        #endregion

        #region Constructors

        public AmbientLightPropertiesModel(LayerPropertiesModel properties)
            : base(properties)
        {
            Brush = new SolidColorBrush(Color.FromRgb(0, 0, 0));
        }

        #endregion
    }
}

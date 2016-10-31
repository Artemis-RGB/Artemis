using System.Windows.Media;
using Artemis.Profiles.Layers.Models;
using Artemis.Profiles.Layers.Types.AmbientLight.Model;
using Newtonsoft.Json;

namespace Artemis.Profiles.Layers.Types.AmbientLight
{
    public class AmbientLightPropertiesModel : LayerPropertiesModel
    {
        #region Properties & Fields

        //HACK DarthAffe 30.10.2016: The 'normal' Brush-Property destoys the profile since Drawing-Brushes cannot be deserialized.
        [JsonIgnore]
        public Brush AmbientLightBrush { get; set; }

        public int OffsetLeft { get; set; } = 0;
        public int OffsetRight { get; set; } = 0;
        public int OffsetTop { get; set; } = 0;
        public int OffsetBottom { get; set; } = 0;

        public double MirroredAmount { get; set; } = 10;

        public SmoothMode SmoothMode { get; set; } = SmoothMode.Low;
        public BlackBarDetectionMode BlackBarDetectionMode { get; set; } = BlackBarDetectionMode.Bottom;
        public FlipMode FlipMode { get; set; } = FlipMode.Vertical;

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

using System;
using System.Collections.Generic;
using System.Windows.Media;
using Artemis.Profiles.Layers.Models;

namespace Artemis.Profiles.Layers.Types.AngularBrush
{
    public class AngularBrushPropertiesModel : LayerPropertiesModel
    {
        #region Properties & Fields

        public IList<Tuple<double, Color>> GradientStops { get; set; }

        #endregion

        #region Constructors

        public AngularBrushPropertiesModel(LayerPropertiesModel properties = null)
            : base(properties)
        { }

        #endregion

        #region Methods

        #endregion
    }
}

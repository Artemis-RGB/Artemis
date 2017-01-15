using System;
using System.Collections.Generic;
using System.Windows.Media;
using Artemis.Profiles.Layers.Models;

namespace Artemis.Profiles.Layers.Types.ConicalBrush
{
    public class ConicalBrushPropertiesModel : LayerPropertiesModel
    {
        #region Properties & Fields

        public IList<Tuple<double, Color>> GradientStops { get; set; }

        #endregion

        #region Constructors

        public ConicalBrushPropertiesModel(LayerPropertiesModel properties = null)
            : base(properties)
        { }

        #endregion
    }
}

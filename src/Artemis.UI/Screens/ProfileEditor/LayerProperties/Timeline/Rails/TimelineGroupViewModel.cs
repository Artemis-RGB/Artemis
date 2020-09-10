using System;
using System.Collections.Generic;
using System.Text;
using Artemis.Core;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.Timeline.Rails
{
    public class TimelineGroupViewModel
    {
        public LayerPropertyGroup LayerPropertyGroup { get; }

        public TimelineGroupViewModel(LayerPropertyGroup layerPropertyGroup)
        {
            LayerPropertyGroup = layerPropertyGroup;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Artemis.Core.Models.Profile;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties
{
    public class LayerPropertyViewModel : PropertyChangedBase
    {
        public LayerProperty LayerProperty { get; }

        public LayerPropertyViewModel(LayerProperty layerProperty)
        {
            LayerProperty = layerProperty;
        }

        public bool IsCollapsed { get; set; }
    }
}
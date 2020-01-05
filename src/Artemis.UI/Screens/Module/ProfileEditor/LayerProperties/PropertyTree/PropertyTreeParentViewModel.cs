using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.PropertyTree
{
    public class PropertyTreeParentViewModel : PropertyTreeItemViewModel
    {
        public string Name { get; set; }
        public PropertyTreeItemViewModel Children { get; set; }
    }

    public class PropertyTreeItemViewModel : PropertyChangedBase
    {
    }
}

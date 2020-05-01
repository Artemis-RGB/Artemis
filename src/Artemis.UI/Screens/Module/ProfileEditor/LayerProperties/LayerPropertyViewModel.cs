using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Models.Profile.LayerProperties;
using Artemis.Core.Models.Profile.LayerProperties.Attributes;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Abstract;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties
{
    public class LayerPropertyViewModel<T> : LayerPropertyBaseViewModel
    {
        public LayerPropertyViewModel(LayerProperty<T> layerProperty, PropertyDescriptionAttribute propertyDescription)
        {
            LayerProperty = layerProperty;
            PropertyDescription = propertyDescription;
        }

        public LayerProperty<T> LayerProperty { get; }
        public PropertyDescriptionAttribute PropertyDescription { get; }

        public override List<BaseLayerPropertyKeyframe> GetKeyframes(bool visibleOnly)
        {
            return LayerProperty.BaseKeyframes.ToList();
        }
    }
}
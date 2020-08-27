using System.Reflection;
using Artemis.Core.Models.Profile.LayerProperties;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.DataBindings
{
    public class DataBindingsTabViewModel : PropertyChangedBase
    {
        public DataBindingsTabViewModel(BaseLayerProperty layerProperty, PropertyInfo dataBindingProperty)
        {
            DisplayName = dataBindingProperty.Name.ToUpper();
            LayerProperty = layerProperty;
            DataBindingProperty = dataBindingProperty;
        }

        public string DisplayName { get; }
        public BaseLayerProperty LayerProperty { get; }
        public PropertyInfo DataBindingProperty { get; }
    }
}
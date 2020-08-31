using System.Reflection;
using Artemis.Core;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.DataBindings
{
    public class DataBindingViewModel : PropertyChangedBase
    {
        public DataBindingViewModel(BaseLayerProperty layerProperty, PropertyInfo dataBindingProperty)
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
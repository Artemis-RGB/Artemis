using Artemis.Core.Models.Profile.LayerProperties;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.DataBindings
{
    public class DataBindingsViewModel : PropertyChangedBase
    {
        public DataBindingsViewModel(BaseLayerProperty layerProperty)
        {
            Tabs = new BindableCollection<DataBindingsTabViewModel>();
            LayerProperty = layerProperty;

            Initialise();
        }

        public BindableCollection<DataBindingsTabViewModel> Tabs { get; set; }
        public BaseLayerProperty LayerProperty { get; }

        private void Initialise()
        {
            foreach (var dataBindingProperty in LayerProperty.GetDataBindingProperties())
                Tabs.Add(new DataBindingsTabViewModel(LayerProperty, dataBindingProperty));
        }
    }
}
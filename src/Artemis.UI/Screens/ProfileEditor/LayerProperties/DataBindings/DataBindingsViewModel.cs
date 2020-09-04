using System.Linq;
using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.DataBindings
{
    public class DataBindingsViewModel : PropertyChangedBase
    {
        private readonly IDataBindingsVmFactory _dataBindingsVmFactory;
        private DataBindingsTabsViewModel _dataBindingsTabsViewModel;
        private DataBindingViewModel _dataBindingViewModel;

        public DataBindingsViewModel(BaseLayerProperty layerProperty, IDataBindingsVmFactory dataBindingsVmFactory)
        {
            _dataBindingsVmFactory = dataBindingsVmFactory;
            LayerProperty = layerProperty;
            Initialise();
        }

        public BaseLayerProperty LayerProperty { get; }

        public DataBindingViewModel DataBindingViewModel
        {
            get => _dataBindingViewModel;
            set => SetAndNotify(ref _dataBindingViewModel, value);
        }

        public DataBindingsTabsViewModel DataBindingsTabsViewModel
        {
            get => _dataBindingsTabsViewModel;
            set => SetAndNotify(ref _dataBindingsTabsViewModel, value);
        }

        private void Initialise()
        {
            DataBindingViewModel = null;
            DataBindingsTabsViewModel = null;

            var properties = LayerProperty.GetDataBindingProperties();
            if (properties == null || properties.Count == 0)
                return;

            // Create a data binding VM for each data bindable property. These VMs will be responsible for retrieving
            // and creating the actual data bindings
            if (properties.Count == 1)
                DataBindingViewModel = _dataBindingsVmFactory.DataBindingViewModel(LayerProperty, properties.First());
            else
            {
                DataBindingsTabsViewModel = new DataBindingsTabsViewModel();
                foreach (var dataBindingProperty in properties)
                    DataBindingsTabsViewModel.Tabs.Add(_dataBindingsVmFactory.DataBindingViewModel(LayerProperty, dataBindingProperty));
            }
        }
    }
}
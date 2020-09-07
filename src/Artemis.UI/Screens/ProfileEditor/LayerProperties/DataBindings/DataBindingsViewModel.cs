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

            var registrations = LayerProperty.DataBindingRegistrations;
            if (registrations == null || registrations.Count == 0)
                return;

            // Create a data binding VM for each data bindable property. These VMs will be responsible for retrieving
            // and creating the actual data bindings
            if (registrations.Count == 1)
                DataBindingViewModel = _dataBindingsVmFactory.DataBindingViewModel(registrations.First());
            else
            {
                DataBindingsTabsViewModel = new DataBindingsTabsViewModel();
                foreach (var registration in registrations)
                    DataBindingsTabsViewModel.Tabs.Add(_dataBindingsVmFactory.DataBindingViewModel(registration));
            }
        }
    }
}
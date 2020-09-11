using System;
using System.Linq;
using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.DataBindings
{
    public class DataBindingsViewModel<T> : Conductor<IDataBindingViewModel>.Collection.AllActive
    {
        private readonly IDataBindingsVmFactory _dataBindingsVmFactory;
        
        public DataBindingsViewModel(LayerProperty<T> layerProperty, IDataBindingsVmFactory dataBindingsVmFactory)
        {
            _dataBindingsVmFactory = dataBindingsVmFactory;
            LayerProperty = layerProperty;
            Initialise();
        }

        public LayerProperty<T> LayerProperty { get; }

        private void Initialise()
        {
            var registrations = LayerProperty.GetAllDataBindingRegistrations();

            // Create a data binding VM for each data bindable property. These VMs will be responsible for retrieving
            // and creating the actual data bindings
            foreach (var registration in registrations)
                ActivateItem(_dataBindingsVmFactory.DataBindingViewModel(registration));
        }
    }
}
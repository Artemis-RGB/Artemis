using System;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.DataBindings
{
    public class DataBindingsViewModel : Conductor<IDataBindingViewModel>.Collection.AllActive
    {
        private readonly IDataBindingsVmFactory _dataBindingsVmFactory;
        private readonly IProfileEditorService _profileEditorService;

        public DataBindingsViewModel(IProfileEditorService profileEditorService, IDataBindingsVmFactory dataBindingsVmFactory)
        {
            _profileEditorService = profileEditorService;
            _dataBindingsVmFactory = dataBindingsVmFactory;

            _profileEditorService.SelectedDataBindingChanged += ProfileEditorServiceOnSelectedDataBindingChanged;
            CreateDataBindingViewModels();
        }

        private void CreateDataBindingViewModels()
        {
            Items.Clear();

            var layerProperty = _profileEditorService.SelectedDataBinding;
            if (layerProperty == null)
                return;

            var registrations = layerProperty.GetAllDataBindingRegistrations();

            // Create a data binding VM for each data bindable property. These VMs will be responsible for retrieving
            // and creating the actual data bindings
            foreach (var registration in registrations)
                ActivateItem(_dataBindingsVmFactory.DataBindingViewModel(registration));
        }

        protected override void OnClose()
        {
            _profileEditorService.SelectedDataBindingChanged -= ProfileEditorServiceOnSelectedDataBindingChanged;
            base.OnClose();
        }

        private void ProfileEditorServiceOnSelectedDataBindingChanged(object? sender, EventArgs e)
        {
            CreateDataBindingViewModels();
        }
    }
}
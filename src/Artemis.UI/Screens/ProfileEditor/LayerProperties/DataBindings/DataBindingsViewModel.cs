using System;
using System.Collections.Generic;
using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.DataBindings
{
    public class DataBindingsViewModel : Conductor<IDataBindingViewModel>.Collection.AllActive
    {
        private readonly IDataBindingsVmFactory _dataBindingsVmFactory;
        private readonly IProfileEditorService _profileEditorService;
        private int _selectedItemIndex;

        public DataBindingsViewModel(IProfileEditorService profileEditorService, IDataBindingsVmFactory dataBindingsVmFactory)
        {
            _profileEditorService = profileEditorService;
            _dataBindingsVmFactory = dataBindingsVmFactory;
        }

        public int SelectedItemIndex
        {
            get => _selectedItemIndex;
            set => SetAndNotify(ref _selectedItemIndex, value);
        }
        
        private void CreateDataBindingViewModels()
        {
            Items.Clear();

            ILayerProperty layerProperty = _profileEditorService.SelectedDataBinding;
            if (layerProperty == null)
                return;

            List<IDataBindingRegistration> registrations = layerProperty.GetAllDataBindingRegistrations();

            // Create a data binding VM for each data bindable property. These VMs will be responsible for retrieving
            // and creating the actual data bindings
            foreach (IDataBindingRegistration registration in registrations)
                Items.Add(_dataBindingsVmFactory.DataBindingViewModel(registration));

            SelectedItemIndex = 0;
        }

        private void ProfileEditorServiceOnSelectedDataBindingChanged(object sender, EventArgs e)
        {
            CreateDataBindingViewModels();
        }

        #region Overrides of Screen

        /// <inheritdoc />
        protected override void OnInitialActivate()
        {
            _profileEditorService.SelectedDataBindingChanged += ProfileEditorServiceOnSelectedDataBindingChanged;
            CreateDataBindingViewModels();
            base.OnInitialActivate();
        }

        protected override void OnClose()
        {
            _profileEditorService.SelectedDataBindingChanged -= ProfileEditorServiceOnSelectedDataBindingChanged;
            base.OnClose();
        }

        #endregion
    }
}
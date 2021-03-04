using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        private ILayerProperty? _selectedDataBinding;
        private int _selectedItemIndex;
        private bool _updating;

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
            int oldIndex = SelectedItemIndex;
            Items.Clear();

            ILayerProperty layerProperty = _profileEditorService.SelectedDataBinding;
            if (layerProperty == null)
                return;

            List<IDataBindingRegistration> registrations = layerProperty.GetAllDataBindingRegistrations();

            // Create a data binding VM for each data bindable property. These VMs will be responsible for retrieving
            // and creating the actual data bindings
            Items.AddRange(registrations.Select(registration => _dataBindingsVmFactory.DataBindingViewModel(registration)));

            SelectedItemIndex = Items.Count < oldIndex ? 0 : oldIndex;
        }

        private void ProfileEditorServiceOnSelectedDataBindingChanged(object sender, EventArgs e)
        {
            CreateDataBindingViewModels();
            SubscribeToSelectedDataBinding();

            SelectedItemIndex = 0;
        }

        private void SubscribeToSelectedDataBinding()
        {
            if (_selectedDataBinding != null)
            {
                _selectedDataBinding.DataBindingPropertyRegistered -= DataBindingRegistrationsChanged;
                _selectedDataBinding.DataBindingPropertiesCleared -= DataBindingRegistrationsChanged;
            }

            _selectedDataBinding = _profileEditorService.SelectedDataBinding;
            if (_selectedDataBinding != null)
            {
                _selectedDataBinding.DataBindingPropertyRegistered += DataBindingRegistrationsChanged;
                _selectedDataBinding.DataBindingPropertiesCleared += DataBindingRegistrationsChanged;
            }
        }

        private void DataBindingRegistrationsChanged(object sender, LayerPropertyEventArgs e)
        {
            if (_updating)
                return;

            _updating = true;
            Execute.PostToUIThread(async () =>
            {
                await Task.Delay(200);
                CreateDataBindingViewModels();
                _updating = false;
            });
        }

        #region Overrides of Screen

        protected override void OnInitialActivate()
        {
            _profileEditorService.SelectedDataBindingChanged += ProfileEditorServiceOnSelectedDataBindingChanged;
            CreateDataBindingViewModels();
            SubscribeToSelectedDataBinding();
            base.OnInitialActivate();
        }
        
        protected override void OnActivate()
        {
            SelectedItemIndex = 0;
            base.OnActivate();
        }

        protected override void OnClose()
        {
            _profileEditorService.SelectedDataBindingChanged -= ProfileEditorServiceOnSelectedDataBindingChanged;
            base.OnClose();
        }

        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.DataBindings
{
    public class DataBindingsViewModel : Screen
    {
        private readonly IProfileEditorService _profileEditorService;
        private IDataBinding _dataBinding;

        public DataBindingsViewModel(IProfileEditorService profileEditorService, INodeService nodeService, ISettingsService settingsService)
        {
            _profileEditorService = profileEditorService;
            AvailableNodes =  nodeService.AvailableNodes.ToList();
            AlwaysShowValues = settingsService.GetSetting("ProfileEditor.AlwaysShowValues", true);
        }

        public List<NodeData> AvailableNodes { get; }
        public PluginSetting<bool> AlwaysShowValues { get; }

        public IDataBinding DataBinding
        {
            get => _dataBinding;
            set => SetAndNotify(ref _dataBinding, value);
        }

        public bool DataBindingEnabled
        {
            get => _dataBinding?.IsEnabled ?? false;
            set
            {
                if (_dataBinding != null)
                    _dataBinding.IsEnabled = value;
            }
        }


        private void ProfileEditorServiceOnSelectedDataBindingChanged(object sender, EventArgs e)
        {
            SubscribeToSelectedDataBinding();
        }

        private void SubscribeToSelectedDataBinding()
        {
            if (DataBinding != null)
            {
                DataBinding.DataBindingEnabled -= DataBindingOnDataBindingToggled;
                DataBinding.DataBindingDisabled -= DataBindingOnDataBindingToggled;
            }

            DataBinding = _profileEditorService.SelectedDataBinding;
            if (DataBinding != null)
            {
                DataBinding.DataBindingEnabled += DataBindingOnDataBindingToggled;
                DataBinding.DataBindingDisabled += DataBindingOnDataBindingToggled;

                OnPropertyChanged(nameof(DataBindingEnabled));
            }
        }

        private void DataBindingOnDataBindingToggled(object sender, DataBindingEventArgs e)
        {
            OnPropertyChanged(nameof(DataBindingEnabled));
        }

        #region Overrides of Screen

        protected override void OnInitialActivate()
        {
            _profileEditorService.SelectedDataBindingChanged += ProfileEditorServiceOnSelectedDataBindingChanged;
            SubscribeToSelectedDataBinding();
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
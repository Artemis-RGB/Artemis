using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.UI.Utilities;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.DataBindings
{
    public class DataBindingViewModel : PropertyChangedBase
    {
        private readonly IDataModelUIService _dataModelUIService;
        private readonly IDataBindingsVmFactory _dataBindingsVmFactory;
        private readonly IProfileEditorService _profileEditorService;
        private readonly Timer _updateTimer;
        private DataBinding _dataBinding;
        private bool _isDataBindingEnabled;
        private DataModelPropertiesViewModel _sourceDataModel;
        private DataModelVisualizationViewModel _selectedSourceProperty;
        private bool _sourceDataModelOpen;
        private object _testInputValue;
        private object _testResultValue;

        public DataBindingViewModel(BaseLayerProperty layerProperty,
            PropertyInfo targetProperty,
            IProfileEditorService profileEditorService,
            IDataModelUIService dataModelUIService,
            ISettingsService settingsService,
            IDataBindingsVmFactory dataBindingsVmFactory)
        {
            _profileEditorService = profileEditorService;
            _dataModelUIService = dataModelUIService;
            _dataBindingsVmFactory = dataBindingsVmFactory;
            _updateTimer = new Timer(500);

            LayerProperty = layerProperty;
            TargetProperty = targetProperty;

            DisplayName = TargetProperty.Name.ToUpper();
            SelectSourcePropertyCommand = new DelegateCommand(ExecuteSelectSourceProperty);
            ModifierViewModels = new BindableCollection<DataBindingModifierViewModel>();
            DataBinding = layerProperty.DataBindings.FirstOrDefault(d => d.TargetProperty == targetProperty);

            ShowDataModelValues = settingsService.GetSetting<bool>("ProfileEditor.ShowDataModelValues");

            _isDataBindingEnabled = DataBinding != null;

            // Initialize async, no need to wait for it
            Task.Run(Initialize);
        }

        public DataModelPropertiesViewModel SourceDataModel
        {
            get => _sourceDataModel;
            set => SetAndNotify(ref _sourceDataModel, value);
        }

        public bool SourceDataModelOpen
        {
            get => _sourceDataModelOpen;
            set => SetAndNotify(ref _sourceDataModelOpen, value);
        }

        public DataModelVisualizationViewModel SelectedSourceProperty
        {
            get => _selectedSourceProperty;
            set => SetAndNotify(ref _selectedSourceProperty, value);
        }

        public PluginSetting<bool> ShowDataModelValues { get; }
        public BaseLayerProperty LayerProperty { get; }
        public PropertyInfo TargetProperty { get; }
        public string DisplayName { get; }
        public BindableCollection<DataBindingModifierViewModel> ModifierViewModels { get; }

        public DelegateCommand SelectSourcePropertyCommand { get; }

        public bool IsDataBindingEnabled
        {
            get => _isDataBindingEnabled;
            set
            {
                if (!SetAndNotify(ref _isDataBindingEnabled, value)) return;
                if (value)
                    EnableDataBinding();
                else
                    RemoveDataBinding();
            }
        }

        public DataBinding DataBinding
        {
            get => _dataBinding;
            set
            {
                if (!SetAndNotify(ref _dataBinding, value)) return;
                UpdateModifierViewModels();
            }
        }

        public void EnableDataBinding()
        {
            if (DataBinding != null)
                return;

            DataBinding = LayerProperty.AddDataBinding(TargetProperty);
        }

        public void RemoveDataBinding()
        {
            if (DataBinding == null)
                return;

            var toRemove = DataBinding;
            DataBinding = null;
            LayerProperty.RemoveDataBinding(toRemove);
        }

        public void AddModifier()
        {
            if (DataBinding == null)
                return;

            var modifier = new DataBindingModifier(ProfileRightSideType.Dynamic);
            DataBinding.AddModifier(modifier);

            ModifierViewModels.Add(_dataBindingsVmFactory.DataBindingModifierViewModel(modifier));
        }

        private void Initialize()
        {
            // Get the data models
            SourceDataModel = _dataModelUIService.GetMainDataModelVisualization();
            if (!_dataModelUIService.GetPluginExtendsDataModel(_profileEditorService.GetCurrentModule()))
                SourceDataModel.Children.Add(_dataModelUIService.GetPluginDataModelVisualization(_profileEditorService.GetCurrentModule()));

            SourceDataModel.UpdateRequested += SourceDataModelOnUpdateRequested;

            Update();

            _updateTimer.Start();
            _updateTimer.Elapsed += OnUpdateTimerOnElapsed;
        }

        private void Update()
        {
            if (DataBinding == null || SourceDataModel == null)
                return;

            // Determine the source property
            if (DataBinding.SourceDataModel == null)
                SelectedSourceProperty = null;
            else
                SelectedSourceProperty = SourceDataModel.GetChildByPath(DataBinding.SourceDataModel.PluginInfo.Guid, DataBinding.SourcePropertyPath);
        }

        private void SourceDataModelOnUpdateRequested(object sender, EventArgs e)
        {
            SourceDataModel.ApplyTypeFilter(true, DataBinding.TargetProperty.PropertyType);
        }

        private void OnUpdateTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            UpdateTestResult();
            if (SourceDataModelOpen)
                SourceDataModel.Update(_dataModelUIService);
        }

        private void UpdateTestResult()
        {
            var currentValue = SelectedSourceProperty?.GetCurrentValue();
            if (currentValue == null && TargetProperty.PropertyType.IsValueType)
                currentValue = Activator.CreateInstance(TargetProperty.PropertyType);

            TestInputValue = Convert.ChangeType(currentValue, TargetProperty.PropertyType);
            TestResultValue = DataBinding?.GetValue(TestInputValue);
        }

        public object TestInputValue
        {
            get => _testInputValue;
            set => SetAndNotify(ref _testInputValue, value);
        }

        public object TestResultValue
        {
            get => _testResultValue;
            set => SetAndNotify(ref _testResultValue, value);
        }

        private void UpdateModifierViewModels()
        {
            ModifierViewModels.Clear();
            if (DataBinding == null)
                return;

            foreach (var dataBindingModifier in DataBinding.Modifiers)
                ModifierViewModels.Add(_dataBindingsVmFactory.DataBindingModifierViewModel(dataBindingModifier));
        }

        private void ExecuteSelectSourceProperty(object context)
        {
            if (!(context is DataModelVisualizationViewModel selected))
                return;

            DataBinding.UpdateSource(selected.DataModel, selected.PropertyPath);
            _profileEditorService.UpdateSelectedProfileElement();

            Update();
        }
    }
}
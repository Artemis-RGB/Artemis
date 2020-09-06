using System;
using System.Linq;
using System.Reflection;
using System.Timers;
using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Input;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.DataBindings
{
    public class DataBindingViewModel : PropertyChangedBase
    {
        private readonly IDataBindingsVmFactory _dataBindingsVmFactory;
        private readonly IDataModelUIService _dataModelUIService;
        private readonly IProfileEditorService _profileEditorService;
        private readonly Timer _updateTimer;
        private DataBinding _dataBinding;
        private bool _isDataBindingEnabled;
        private DataModelDynamicViewModel _targetSelectionViewModel;
        private object _testInputValue;
        private object _testResultValue;

        public DataBindingViewModel(BaseLayerProperty layerProperty,
            PropertyInfo targetProperty,
            IProfileEditorService profileEditorService,
            IDataModelUIService dataModelUIService,
            IDataBindingsVmFactory dataBindingsVmFactory)
        {
            _profileEditorService = profileEditorService;
            _dataModelUIService = dataModelUIService;
            _dataBindingsVmFactory = dataBindingsVmFactory;
            _updateTimer = new Timer(500);

            DisplayName = targetProperty.Name.ToUpper();
            ModifierViewModels = new BindableCollection<DataBindingModifierViewModel>();

            LayerProperty = layerProperty;
            TargetProperty = targetProperty;
            DataBinding = layerProperty.DataBindings.FirstOrDefault(d => d.TargetProperty == targetProperty);

            if (DataBinding != null)
                DataBinding.ModifiersUpdated += DataBindingOnModifiersUpdated;

            _isDataBindingEnabled = DataBinding != null;

            // Initialize async, no need to wait for it
            Execute.PostToUIThread(Initialize);
        }

        public BaseLayerProperty LayerProperty { get; }
        public PropertyInfo TargetProperty { get; }
        public string DisplayName { get; }
        public BindableCollection<DataBindingModifierViewModel> ModifierViewModels { get; }

        public DataModelDynamicViewModel TargetSelectionViewModel
        {
            get => _targetSelectionViewModel;
            private set => SetAndNotify(ref _targetSelectionViewModel, value);
        }

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

        public void EnableDataBinding()
        {
            if (DataBinding != null)
                return;

            DataBinding = LayerProperty.AddDataBinding(TargetProperty);
            DataBinding.ModifiersUpdated += DataBindingOnModifiersUpdated;
            Update();

            _profileEditorService.UpdateSelectedProfileElement();
        }


        public void RemoveDataBinding()
        {
            if (DataBinding == null)
                return;

            var toRemove = DataBinding;
            DataBinding = null;
            LayerProperty.RemoveDataBinding(toRemove);
            toRemove.ModifiersUpdated -= DataBindingOnModifiersUpdated;
            Update();

            _profileEditorService.UpdateSelectedProfileElement();
        }

        public void AddModifier()
        {
            if (DataBinding == null)
                return;

            var modifier = new DataBindingModifier(ProfileRightSideType.Dynamic);
            DataBinding.AddModifier(modifier);

            _profileEditorService.UpdateSelectedProfileElement();
        }

        private void Initialize()
        {
            TargetSelectionViewModel = _dataModelUIService.GetDynamicSelectionViewModel(_profileEditorService.GetCurrentModule());
            TargetSelectionViewModel.PropertySelected += TargetSelectionViewModelOnPropertySelected;

            Update();

            _updateTimer.Start();
            _updateTimer.Elapsed += OnUpdateTimerOnElapsed;
        }

        private void Update()
        {
            if (DataBinding == null)
            {
                TargetSelectionViewModel.IsEnabled = false;
                return;
            }

            TargetSelectionViewModel.IsEnabled = true;
            TargetSelectionViewModel.PopulateSelectedPropertyViewModel(DataBinding.SourceDataModel, DataBinding.SourcePropertyPath);
            TargetSelectionViewModel.FilterTypes = new[] {DataBinding.TargetProperty.PropertyType};

            UpdateModifierViewModels();
        }

        private void DataBindingOnModifiersUpdated(object sender, EventArgs e)
        {
            UpdateModifierViewModels();
        }

        private void TargetSelectionViewModelOnPropertySelected(object sender, DataModelInputDynamicEventArgs e)
        {
            DataBinding.UpdateSource(e.DataModelVisualizationViewModel.DataModel, e.DataModelVisualizationViewModel.PropertyPath);
            _profileEditorService.UpdateSelectedProfileElement();
        }

        private void OnUpdateTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            UpdateTestResult();
        }

        private void UpdateTestResult()
        {
            var currentValue = TargetSelectionViewModel.SelectedPropertyViewModel?.GetCurrentValue();
            if (currentValue == null && TargetProperty.PropertyType.IsValueType)
                currentValue = Activator.CreateInstance(TargetProperty.PropertyType);

            TestInputValue = Convert.ChangeType(currentValue, TargetProperty.PropertyType);
            TestResultValue = DataBinding?.GetValue(TestInputValue);
        }

        private void UpdateModifierViewModels()
        {
            ModifierViewModels.Clear();
            if (DataBinding == null)
                return;

            foreach (var dataBindingModifier in DataBinding.Modifiers)
                ModifierViewModels.Add(_dataBindingsVmFactory.DataBindingModifierViewModel(dataBindingModifier));
        }
    }
}
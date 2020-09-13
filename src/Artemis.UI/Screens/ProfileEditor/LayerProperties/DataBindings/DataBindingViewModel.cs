using System;
using System.Linq;
using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.ProfileEditor.LayerProperties.Timeline;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Input;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.DataBindings
{
    public class DataBindingViewModel<TLayerProperty, TProperty> : Screen, IDataBindingViewModel
    {
        private readonly IDataBindingsVmFactory _dataBindingsVmFactory;
        private readonly IDataModelUIService _dataModelUIService;
        private readonly IProfileEditorService _profileEditorService;
        private DataBinding<TLayerProperty, TProperty> _dataBinding;
        private int _easingTime;
        private bool _isDataBindingEnabled;
        private bool _isEasingTimeEnabled;
        private DataBindingMode _selectedDataBindingMode;
        private TimelineEasingViewModel _selectedEasingViewModel;
        private DataModelDynamicViewModel _targetSelectionViewModel;
        private TProperty _testInputValue;
        private TProperty _testResultValue;
        private bool _updating;

        public DataBindingViewModel(DataBindingRegistration<TLayerProperty, TProperty> registration,
            IProfileEditorService profileEditorService,
            IDataModelUIService dataModelUIService,
            IDataBindingsVmFactory dataBindingsVmFactory)
        {
            Registration = registration;
            _profileEditorService = profileEditorService;
            _dataModelUIService = dataModelUIService;
            _dataBindingsVmFactory = dataBindingsVmFactory;

            if (Registration.Member != null)
                DisplayName = Registration.Member.Name.ToUpper();
            else
                DisplayName = Registration.LayerProperty.PropertyDescription.Name.ToUpper();

            DataBindingModes = new BindableCollection<ValueDescription>(EnumUtilities.GetAllValuesAndDescriptions(typeof(DataBindingMode)));
            EasingViewModels = new BindableCollection<TimelineEasingViewModel>();
            ModifierViewModels = new BindableCollection<DataBindingModifierViewModel<TLayerProperty, TProperty>>();

            DataBinding = Registration.DataBinding;
            if (DataBinding != null)
                DataBinding.ModifiersUpdated += DataBindingOnModifiersUpdated;

            _isDataBindingEnabled = DataBinding != null;

            // Initialize async, no need to wait for it
            Execute.PostToUIThread(Initialize);
        }

        public DataBindingRegistration<TLayerProperty, TProperty> Registration { get; }

        public BindableCollection<ValueDescription> DataBindingModes { get; }
        public BindableCollection<TimelineEasingViewModel> EasingViewModels { get; }
        public BindableCollection<DataBindingModifierViewModel<TLayerProperty, TProperty>> ModifierViewModels { get; }

        public DataBindingMode SelectedDataBindingMode
        {
            get => _selectedDataBindingMode;
            set => SetAndNotify(ref _selectedDataBindingMode, value);
        }

        public TimelineEasingViewModel SelectedEasingViewModel
        {
            get => _selectedEasingViewModel;
            set
            {
                if (!SetAndNotify(ref _selectedEasingViewModel, value)) return;
                ApplyChanges();
            }
        }

        public int EasingTime
        {
            get => _easingTime;
            set
            {
                if (!SetAndNotify(ref _easingTime, value)) return;
                ApplyChanges();
            }
        }

        public bool IsEasingTimeEnabled
        {
            get => _isEasingTimeEnabled;
            set
            {
                if (!SetAndNotify(ref _isEasingTimeEnabled, value)) return;
                ApplyChanges();
            }
        }

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

        public DataBinding<TLayerProperty, TProperty> DataBinding
        {
            get => _dataBinding;
            set
            {
                if (!SetAndNotify(ref _dataBinding, value)) return;
                UpdateModifierViewModels();
            }
        }

        public TProperty TestInputValue
        {
            get => _testInputValue;
            set => SetAndNotify(ref _testInputValue, value);
        }

        public TProperty TestResultValue
        {
            get => _testResultValue;
            set => SetAndNotify(ref _testResultValue, value);
        }

        public void EnableDataBinding()
        {
            if (DataBinding != null)
                return;

            DataBinding = Registration.LayerProperty.EnableDataBinding(Registration);
            DataBinding.ModifiersUpdated += DataBindingOnModifiersUpdated;
            Update();

            _profileEditorService.UpdateSelectedProfileElement();
        }

        public void RemoveDataBinding()
        {
            if (DataBinding == null)
                return;

            var toDisable = DataBinding;
            DataBinding = null;
            Registration.LayerProperty.DisableDataBinding(toDisable);
            toDisable.ModifiersUpdated -= DataBindingOnModifiersUpdated;
            Update();

            _profileEditorService.UpdateSelectedProfileElement();
        }

        public void AddModifier()
        {
            if (DataBinding == null)
                return;

            var modifier = new DataBindingModifier<TLayerProperty, TProperty>(ProfileRightSideType.Dynamic);
            DataBinding.AddModifier(modifier);

            _profileEditorService.UpdateSelectedProfileElement();
        }

        private void Initialize()
        {
            EasingViewModels.AddRange(Enum.GetValues(typeof(Easings.Functions)).Cast<Easings.Functions>().Select(v => new TimelineEasingViewModel(v, false)));
            TargetSelectionViewModel = _dataModelUIService.GetDynamicSelectionViewModel(_profileEditorService.GetCurrentModule());
            TargetSelectionViewModel.PropertySelected += TargetSelectionViewModelOnPropertySelected;
            _profileEditorService.ProfilePreviewUpdated += ProfileEditorServiceOnProfilePreviewUpdated;

            Update();
        }

        private void Update()
        {
            if (_updating)
                return;

            if (DataBinding == null)
            {
                TargetSelectionViewModel.IsEnabled = false;
                IsEasingTimeEnabled = false;
                return;
            }

            _updating = true;

            SelectedDataBindingMode = DataBinding.Mode;
            EasingTime = (int) DataBinding.EasingTime.TotalMilliseconds;
            SelectedEasingViewModel = EasingViewModels.First(vm => vm.EasingFunction == DataBinding.EasingFunction);
            IsEasingTimeEnabled = EasingTime > 0;

            TargetSelectionViewModel.IsEnabled = true;
            TargetSelectionViewModel.PopulateSelectedPropertyViewModel(DataBinding.SourceDataModel, DataBinding.SourcePropertyPath);
            TargetSelectionViewModel.FilterTypes = new[] {DataBinding.GetTargetType()};

            UpdateModifierViewModels();

            _updating = false;
        }

        private void ApplyChanges()
        {
            if (_updating)
                return;

            DataBinding.Mode = SelectedDataBindingMode;
            DataBinding.EasingTime = TimeSpan.FromMilliseconds(EasingTime);
            DataBinding.EasingFunction = SelectedEasingViewModel?.EasingFunction ?? Easings.Functions.Linear;

            _profileEditorService.UpdateSelectedProfileElement();
            Update();
        }

        private void UpdateTestResult()
        {
            if (DataBinding == null)
            {
                TestInputValue = default;
                TestResultValue = default;
                return;
            }

            var currentValue = TargetSelectionViewModel.SelectedPropertyViewModel?.GetCurrentValue();
            if (currentValue == null)
                currentValue = default(TProperty);

            TestInputValue = (TProperty) Convert.ChangeType(currentValue, typeof(TProperty));
            if (DataBinding != null)
                TestResultValue = DataBinding.GetValue(TestInputValue);
            else
                TestInputValue = default;
        }

        private void UpdateModifierViewModels()
        {
            ModifierViewModels.Clear();
            if (DataBinding == null)
                return;

            foreach (var dataBindingModifier in DataBinding.Modifiers)
                ModifierViewModels.Add(_dataBindingsVmFactory.DataBindingModifierViewModel(dataBindingModifier));
        }

        private void ProfileEditorServiceOnProfilePreviewUpdated(object sender, EventArgs e)
        {
            UpdateTestResult();
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

        public void Dispose()
        {
            _profileEditorService.ProfilePreviewUpdated -= ProfileEditorServiceOnProfilePreviewUpdated;
            TargetSelectionViewModel.PropertySelected -= TargetSelectionViewModelOnPropertySelected;
            if (DataBinding != null)
                DataBinding.ModifiersUpdated -= DataBindingOnModifiersUpdated;
        }
    }

    public interface IDataBindingViewModel : IScreen, IDisposable
    {
    }
}
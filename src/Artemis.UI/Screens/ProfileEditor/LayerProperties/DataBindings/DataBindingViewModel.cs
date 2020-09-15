using System;
using System.Linq;
using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.ProfileEditor.LayerProperties.Timeline;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.DataBindings
{
    public class DataBindingViewModel<TLayerProperty, TProperty> : Conductor<IDataBindingModeViewModel>, IDataBindingViewModel
    {
        private readonly IDataBindingsVmFactory _dataBindingsVmFactory;
        private readonly IProfileEditorService _profileEditorService;
        private DataBinding<TLayerProperty, TProperty> _dataBinding;
        private int _easingTime;
        private bool _isEasingTimeEnabled;
        private DataBindingModeType _selectedDataBindingMode;
        private TimelineEasingViewModel _selectedEasingViewModel;

        private TProperty _testInputValue;
        private TProperty _testResultValue;
        private bool _updating;
        private bool _isDataBindingEnabled;

        public DataBindingViewModel(DataBindingRegistration<TLayerProperty, TProperty> registration,
            IProfileEditorService profileEditorService,
            IDataBindingsVmFactory dataBindingsVmFactory)
        {
            Registration = registration;
            _profileEditorService = profileEditorService;
            _dataBindingsVmFactory = dataBindingsVmFactory;

            if (Registration.Member != null)
                DisplayName = Registration.Member.Name.ToUpper();
            else
                DisplayName = Registration.LayerProperty.PropertyDescription.Name.ToUpper();

            DataBindingModes = new BindableCollection<ValueDescription>(EnumUtilities.GetAllValuesAndDescriptions(typeof(DataBindingModeType)));
            EasingViewModels = new BindableCollection<TimelineEasingViewModel>();

            DataBinding = Registration.DataBinding;

            Initialize();
        }

        public DataBindingRegistration<TLayerProperty, TProperty> Registration { get; }

        public BindableCollection<ValueDescription> DataBindingModes { get; }
        public BindableCollection<TimelineEasingViewModel> EasingViewModels { get; }

        public DataBindingModeType SelectedDataBindingMode
        {
            get => _selectedDataBindingMode;
            set
            {
                if (!SetAndNotify(ref _selectedDataBindingMode, value)) return;
                ApplyDataBindingMode();
            }
        }

        public bool IsDataBindingEnabled
        {
            get => _isDataBindingEnabled;
            set => SetAndNotify(ref _isDataBindingEnabled, value);
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

        public DataBinding<TLayerProperty, TProperty> DataBinding
        {
            get => _dataBinding;
            set => SetAndNotify(ref _dataBinding, value);
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

        public void Dispose()
        {
            _profileEditorService.ProfilePreviewUpdated -= ProfileEditorServiceOnProfilePreviewUpdated;
        }

        private void Initialize()
        {
            EasingViewModels.AddRange(Enum.GetValues(typeof(Easings.Functions)).Cast<Easings.Functions>().Select(v => new TimelineEasingViewModel(v, false)));
            _profileEditorService.ProfilePreviewUpdated += ProfileEditorServiceOnProfilePreviewUpdated;

            CreateDataBindingModeModeViewModel();
            Update();
        }

        private void CreateDataBindingModeModeViewModel()
        {
            if (DataBinding?.DataBindingMode == null)
            {
                ActiveItem = null;
                return;
            }

            switch (DataBinding.DataBindingMode)
            {
                case DirectDataBinding<TLayerProperty, TProperty> directDataBinding:
                    ActiveItem = _dataBindingsVmFactory.DirectDataBindingModeViewModel(directDataBinding);
                    break;
                case ConditionalDataBinding<TLayerProperty, TProperty> conditionalDataBinding:
                    ActiveItem = _dataBindingsVmFactory.ConditionalDataBindingModeViewModel(conditionalDataBinding);
                    break;
            }
        }

        private void Update()
        {
            if (_updating)
                return;

            if (DataBinding == null)
            {
                IsEasingTimeEnabled = false;
                return;
            }

            _updating = true;

            IsDataBindingEnabled = ActiveItem != null;
            EasingTime = (int) DataBinding.EasingTime.TotalMilliseconds;
            SelectedEasingViewModel = EasingViewModels.First(vm => vm.EasingFunction == DataBinding.EasingFunction);
            IsEasingTimeEnabled = EasingTime > 0;
            switch (DataBinding.DataBindingMode)
            {
                case DirectDataBinding<TLayerProperty, TProperty> _:
                    SelectedDataBindingMode = DataBindingModeType.Direct;
                    break;
                case ConditionalDataBinding<TLayerProperty, TProperty> _:
                    SelectedDataBindingMode = DataBindingModeType.Conditional;
                    break;
                default:
                    SelectedDataBindingMode = DataBindingModeType.None;
                    break;
            }

            ActiveItem?.Update();

            _updating = false;
        }

        private void ApplyChanges()
        {
            if (_updating)
                return;

            if (DataBinding != null)
            {
                DataBinding.EasingTime = TimeSpan.FromMilliseconds(EasingTime);
                DataBinding.EasingFunction = SelectedEasingViewModel?.EasingFunction ?? Easings.Functions.Linear;
            }

            _profileEditorService.UpdateSelectedProfileElement();
            Update();
        }

        private void ApplyDataBindingMode()
        {
            if (_updating)
                return;

            if (DataBinding != null && SelectedDataBindingMode == DataBindingModeType.None)
            {
                RemoveDataBinding();
                CreateDataBindingModeModeViewModel();
                return;
            }

            if (DataBinding == null && SelectedDataBindingMode != DataBindingModeType.None)
                EnableDataBinding();

            DataBinding.ChangeDataBindingMode(SelectedDataBindingMode);
            CreateDataBindingModeModeViewModel();

            _profileEditorService.UpdateSelectedProfileElement();
        }

        private void UpdateTestResult()
        {
            if (DataBinding == null)
            {
                TestInputValue = default;
                TestResultValue = default;
                return;
            }

            var currentValue = ActiveItem?.GetTestValue() ?? default(TProperty);

            TestInputValue = Registration.Converter.ConvertFromObject(currentValue);
            if (DataBinding != null)
                TestResultValue = DataBinding.GetValue(TestInputValue);
            else
                TestInputValue = default;
        }

        private void EnableDataBinding()
        {
            if (DataBinding != null)
                return;

            DataBinding = Registration.LayerProperty.EnableDataBinding(Registration);
            _profileEditorService.UpdateSelectedProfileElement();
        }

        private void RemoveDataBinding()
        {
            if (DataBinding == null)
                return;

            var toDisable = DataBinding;
            DataBinding = null;
            Registration.LayerProperty.DisableDataBinding(toDisable);
            Update();

            _profileEditorService.UpdateSelectedProfileElement();
        }

        private void ProfileEditorServiceOnProfilePreviewUpdated(object sender, EventArgs e)
        {
            UpdateTestResult();
        }
    }

    public interface IDataBindingViewModel : IScreen, IDisposable
    {
    }
}
using System;
using System.Linq;
using System.Timers;
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
        private readonly Timer _updateTimer;
        private int _easingTime;
        private bool _isDataBindingEnabled;
        private bool _isEasingTimeEnabled;
        private DataBindingModeType _selectedDataBindingMode;
        private TimelineEasingViewModel _selectedEasingViewModel;
        private bool _applyTestResultToLayer;

        private bool _updating;
        private bool _updatingTestResult;

        public DataBindingViewModel(DataBindingRegistration<TLayerProperty, TProperty> registration,
            IProfileEditorService profileEditorService,
            IDataModelUIService dataModelUIService,
            IDataBindingsVmFactory dataBindingsVmFactory)
        {
            Registration = registration;
            _profileEditorService = profileEditorService;
            _dataBindingsVmFactory = dataBindingsVmFactory;
            _updateTimer = new Timer(40);

            if (Registration.Member != null)
                DisplayName = Registration.Member.Name.ToUpper();
            else
                DisplayName = Registration.LayerProperty.PropertyDescription.Name.ToUpper();

            DataBindingModes = new BindableCollection<ValueDescription>(EnumUtilities.GetAllValuesAndDescriptions(typeof(DataBindingModeType)));
            EasingViewModels = new BindableCollection<TimelineEasingViewModel>();
            TestInputValue = dataModelUIService.GetDataModelDisplayViewModel(typeof(TProperty), null, true);
            TestResultValue = dataModelUIService.GetDataModelDisplayViewModel(typeof(TProperty), null, true);
        }

        protected override void OnInitialActivate()
        {
            base.OnInitialActivate();
            Initialize();
        }

        public DataBindingRegistration<TLayerProperty, TProperty> Registration { get; }

        public BindableCollection<ValueDescription> DataBindingModes { get; }
        public BindableCollection<TimelineEasingViewModel> EasingViewModels { get; }
        public DataModelDisplayViewModel TestInputValue { get; }
        public DataModelDisplayViewModel TestResultValue { get; }

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

        public bool ApplyTestResultToLayer
        {
            get => _applyTestResultToLayer;
            set
            {
                if (!SetAndNotify(ref _applyTestResultToLayer, value)) return;
                _profileEditorService.UpdateProfilePreview();
            }
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

        public void Dispose()
        {
            _updateTimer.Dispose();
            _updateTimer.Elapsed -= OnUpdateTimerOnElapsed;

            Registration.LayerProperty.Updated -= LayerPropertyOnUpdated;
        }

        private void Initialize()
        {
            EasingViewModels.AddRange(Enum.GetValues(typeof(Easings.Functions)).Cast<Easings.Functions>().Select(v => new TimelineEasingViewModel(v, false)));

            _updateTimer.Start();
            _updateTimer.Elapsed += OnUpdateTimerOnElapsed;
            Registration.LayerProperty.Updated += LayerPropertyOnUpdated;

            CreateDataBindingModeModeViewModel();
            Update();
        }

        private void CreateDataBindingModeModeViewModel()
        {
            if (Registration.DataBinding?.DataBindingMode == null)
            {
                ActiveItem = null;
                return;
            }

            switch (Registration.DataBinding.DataBindingMode)
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

            if (Registration.DataBinding == null)
            {
                IsEasingTimeEnabled = false;
                return;
            }

            _updating = true;

            IsDataBindingEnabled = ActiveItem != null;
            EasingTime = (int) Registration.DataBinding.EasingTime.TotalMilliseconds;
            SelectedEasingViewModel = EasingViewModels.First(vm => vm.EasingFunction == Registration.DataBinding.EasingFunction);
            IsEasingTimeEnabled = EasingTime > 0;
            switch (Registration.DataBinding.DataBindingMode)
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

            if (Registration.DataBinding != null)
            {
                Registration.DataBinding.EasingTime = TimeSpan.FromMilliseconds(EasingTime);
                Registration.DataBinding.EasingFunction = SelectedEasingViewModel?.EasingFunction ?? Easings.Functions.Linear;
            }

            _profileEditorService.UpdateSelectedProfileElement();
            Update();
        }

        private void ApplyDataBindingMode()
        {
            if (_updating)
                return;

            if (Registration.DataBinding != null && SelectedDataBindingMode == DataBindingModeType.None)
            {
                RemoveDataBinding();
                CreateDataBindingModeModeViewModel();
                return;
            }

            if (Registration.DataBinding == null && SelectedDataBindingMode != DataBindingModeType.None)
                EnableDataBinding();

            Registration.DataBinding.ChangeDataBindingMode(SelectedDataBindingMode);
            CreateDataBindingModeModeViewModel();

            _profileEditorService.UpdateSelectedProfileElement();
        }

        private void UpdateTestResult()
        {
            if (_updating || _updatingTestResult)
                return;

            _updatingTestResult = true;

            if (Registration.DataBinding == null || ActiveItem == null)
            {
                TestInputValue.UpdateValue(default);
                TestResultValue.UpdateValue(default);
                _updatingTestResult = false;
                return;
            }

            // While playing in preview data bindings aren't updated
            Registration.DataBinding.UpdateWithDelta(TimeSpan.FromMilliseconds(40));

            if (ActiveItem.SupportsTestValue)
            {
                TProperty currentValue = Registration.Converter.ConvertFromObject(ActiveItem?.GetTestValue() ?? default(TProperty));
                TestInputValue.UpdateValue(currentValue);
                TestResultValue.UpdateValue(Registration.DataBinding != null ? Registration.DataBinding.GetValue(currentValue) : default);
            }
            else
                TestResultValue.UpdateValue(Registration.DataBinding != null ? Registration.DataBinding.GetValue(default) : default);

            if (ApplyTestResultToLayer)
            {
                // TODO: A bit crappy, the ProfileEditorService should really be doing this
                bool playing = ((Parent as Screen)?.Parent as LayerPropertiesViewModel)?.Playing ?? true;
                if (!playing)
                    _profileEditorService.UpdateProfilePreview();
            }

            _updatingTestResult = false;
        }

        private void EnableDataBinding()
        {
            if (Registration.DataBinding != null)
                return;

            Registration.LayerProperty.EnableDataBinding(Registration);
            _profileEditorService.UpdateSelectedProfileElement();
        }

        private void RemoveDataBinding()
        {
            if (Registration.DataBinding == null)
                return;

            Registration.LayerProperty.DisableDataBinding(Registration.DataBinding);
            Update();

            _profileEditorService.UpdateSelectedProfileElement();
        }

        private void OnUpdateTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            UpdateTestResult();
        }

        private void LayerPropertyOnUpdated(object? sender, LayerPropertyEventArgs<TLayerProperty> e)
        {
            if (ApplyTestResultToLayer) 
                Registration.DataBinding?.Apply();
        }
    }

    public interface IDataBindingViewModel : IScreen, IDisposable
    {
    }
}
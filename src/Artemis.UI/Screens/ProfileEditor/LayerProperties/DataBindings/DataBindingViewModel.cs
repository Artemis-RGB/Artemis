using System;
using System.Linq;
using System.Timers;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.Storage.Entities.Profile.DataBindings;
using Artemis.UI.Exceptions;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.ProfileEditor.LayerProperties.Timeline;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.DataBindings
{
    public sealed class DataBindingViewModel<TLayerProperty, TProperty> : Conductor<IDataBindingModeViewModel>, IDataBindingViewModel
    {
        private readonly IDataBindingsVmFactory _dataBindingsVmFactory;
        private readonly ICoreService _coreService;
        private readonly IProfileEditorService _profileEditorService;
        private int _easingTime;
        private bool _isDataBindingEnabled;
        private bool _isEasingTimeEnabled;
        private DataBindingModeType _selectedDataBindingMode;
        private TimelineEasingViewModel _selectedEasingViewModel;

        private bool _updating;
        private bool _updatingTestResult;
        private DateTime _lastUpdate;

        public DataBindingViewModel(DataBindingRegistration<TLayerProperty, TProperty> registration,
            ICoreService coreService,
            ISettingsService settingsService,
            IProfileEditorService profileEditorService,
            IDataModelUIService dataModelUIService,
            IDataBindingsVmFactory dataBindingsVmFactory)
        {
            Registration = registration;
            _coreService = coreService;
            _profileEditorService = profileEditorService;
            _dataBindingsVmFactory = dataBindingsVmFactory;

            DisplayName = Registration.DisplayName.ToUpper();
            AlwaysApplyDataBindings = settingsService.GetSetting("ProfileEditor.AlwaysApplyDataBindings", true);
            DataBindingModes = new BindableCollection<ValueDescription>(EnumUtilities.GetAllValuesAndDescriptions(typeof(DataBindingModeType)));
            EasingViewModels = new BindableCollection<TimelineEasingViewModel>();
            TestInputValue = dataModelUIService.GetDataModelDisplayViewModel(typeof(TProperty), null, true);
            TestResultValue = dataModelUIService.GetDataModelDisplayViewModel(typeof(TProperty), null, true);
        }

        public DataBindingRegistration<TLayerProperty, TProperty> Registration { get; }
        public PluginSetting<bool> AlwaysApplyDataBindings { get; }
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
                Update();
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

        protected override void OnInitialActivate()
        {
            Initialize();
            base.OnInitialActivate();
        }

        private void Initialize()
        {
            EasingViewModels.AddRange(Enum.GetValues(typeof(Easings.Functions)).Cast<Easings.Functions>().Select(v => new TimelineEasingViewModel(v, false)));

            _lastUpdate = DateTime.Now;
            _coreService.FrameRendered += OnFrameRendered;
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
                IsDataBindingEnabled = false;
                IsEasingTimeEnabled = false;
                return;
            }

            _updating = true;

            IsDataBindingEnabled = ActiveItem != null;
            EasingTime = (int) Registration.DataBinding.EasingTime.TotalMilliseconds;
            SelectedEasingViewModel = EasingViewModels.First(vm => vm.EasingFunction == Registration.DataBinding.EasingFunction);
            IsEasingTimeEnabled = EasingTime > 0;
            SelectedDataBindingMode = Registration.DataBinding.DataBindingMode switch
            {
                DirectDataBinding<TLayerProperty, TProperty> _ => DataBindingModeType.Direct,
                ConditionalDataBinding<TLayerProperty, TProperty> _ => DataBindingModeType.Conditional,
                _ => DataBindingModeType.None
            };

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

            _profileEditorService.SaveSelectedProfileElement();
            Update();
        }

        private void ApplyDataBindingMode()
        {
            if (_updating)
                return;

            if (Registration.DataBinding != null && SelectedDataBindingMode == DataBindingModeType.None)
                RemoveDataBinding();
            else
            {
                if (Registration.DataBinding == null && SelectedDataBindingMode != DataBindingModeType.None)
                    EnableDataBinding();

                Registration.DataBinding!.ChangeDataBindingMode(SelectedDataBindingMode);
            }

            CreateDataBindingModeModeViewModel();
            _profileEditorService.SaveSelectedProfileElement();
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

            // If always update is disabled, do constantly update the data binding as long as the view model is open
            // If always update is enabled, this is done for all data bindings in ProfileViewModel
            if (!AlwaysApplyDataBindings.Value)
            {
                Registration.DataBinding.UpdateWithDelta(DateTime.Now - _lastUpdate);
                _profileEditorService.UpdateProfilePreview();
            }

            if (ActiveItem.SupportsTestValue)
            {
                TProperty currentValue = Registration.Converter.ConvertFromObject(ActiveItem?.GetTestValue() ?? default(TProperty));
                TestInputValue.UpdateValue(currentValue);
                TestResultValue.UpdateValue(Registration.DataBinding != null ? Registration.DataBinding.GetValue(currentValue) : default);
            }
            else
            {
                TestResultValue.UpdateValue(Registration.DataBinding != null ? Registration.DataBinding.GetValue(default) : default);
            }

            _updatingTestResult = false;
        }

        private void EnableDataBinding()
        {
            if (Registration.DataBinding != null)
                return;

            Registration.LayerProperty.EnableDataBinding(Registration);
            _profileEditorService.SaveSelectedProfileElement();
        }

        private void RemoveDataBinding()
        {
            if (Registration.DataBinding == null)
                return;

            ActiveItem = null;
            Registration.LayerProperty.DisableDataBinding(Registration.DataBinding);
            Update();

            _profileEditorService.SaveSelectedProfileElement();
        }

        public void CopyDataBinding()
        {
            if (Registration.DataBinding != null)
                JsonClipboard.SetObject(Registration.DataBinding.Entity);
        }

        public void PasteDataBinding()
        {
            if (Registration.DataBinding == null)
                Registration.LayerProperty.EnableDataBinding(Registration);
            if (Registration.DataBinding == null)
                throw new ArtemisUIException("Failed to create a data binding in order to paste");

            DataBindingEntity dataBindingEntity = JsonClipboard.GetData<DataBindingEntity>();
            if (dataBindingEntity == null)
                return;

            Registration.DataBinding.EasingTime = dataBindingEntity.EasingTime;
            Registration.DataBinding.EasingFunction = (Easings.Functions) dataBindingEntity.EasingFunction;
            Registration.DataBinding.ApplyDataBindingEntity(dataBindingEntity.DataBindingMode);
            CreateDataBindingModeModeViewModel();
            Update();
            

            _profileEditorService.SaveSelectedProfileElement();
        }

        private void OnFrameRendered(object sender, FrameRenderedEventArgs e)
        {
            UpdateTestResult();
            _lastUpdate = DateTime.Now;
        }

        private void OnUpdateTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            UpdateTestResult();
        }

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            _coreService.FrameRendered -= OnFrameRendered;
        }

        #endregion
    }

    public interface IDataBindingViewModel : IScreen, IDisposable
    {
    }
}
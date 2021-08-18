using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.Storage.Entities.Profile.DataBindings;
using Artemis.UI.Exceptions;
using Artemis.UI.Screens.ProfileEditor.LayerProperties.Timeline;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.DataBindings
{
    public sealed class DataBindingViewModel<TLayerProperty, TProperty> : Screen, IDataBindingViewModel
    {
        private readonly ICoreService _coreService;
        private readonly IProfileEditorService _profileEditorService;
        private int _easingTime;
        private bool _isDataBindingEnabled;
        private bool _isEasingTimeEnabled;
        private DateTime _lastUpdate;
        private TimelineEasingViewModel _selectedEasingViewModel;

        private bool _updating;
        private bool _updatingTestResult;

        public DataBindingViewModel(DataBindingRegistration<TLayerProperty, TProperty> registration,
            ICoreService coreService,
            ISettingsService settingsService,
            IProfileEditorService profileEditorService,
            IDataModelUIService dataModelUIService,
            INodeService nodeService)
        {
            Registration = registration;
            _coreService = coreService;
            _profileEditorService = profileEditorService;

            DisplayName = Registration.DisplayName.ToUpper();
            AlwaysApplyDataBindings = settingsService.GetSetting("ProfileEditor.AlwaysApplyDataBindings", true);
            EasingViewModels = new BindableCollection<TimelineEasingViewModel>();
            AvailableNodes = nodeService.AvailableNodes;
            TestInputValue = dataModelUIService.GetDataModelDisplayViewModel(typeof(TProperty), null, true);
            TestResultValue = dataModelUIService.GetDataModelDisplayViewModel(typeof(TProperty), null, true);
        }

        public DataBindingRegistration<TLayerProperty, TProperty> Registration { get; }
        public NodeScript<TProperty> Script => Registration.DataBinding?.Script;

        public PluginSetting<bool> AlwaysApplyDataBindings { get; }
        public BindableCollection<TimelineEasingViewModel> EasingViewModels { get; }
        public IEnumerable<NodeData> AvailableNodes { get; }
        public DataModelDisplayViewModel TestInputValue { get; }
        public DataModelDisplayViewModel TestResultValue { get; }


        public bool IsDataBindingEnabled
        {
            get => _isDataBindingEnabled;
            set
            {
                SetAndNotify(ref _isDataBindingEnabled, value);
                if (_updating)
                    return;

                if (value)
                    EnableDataBinding();
                else
                    DisableDataBinding();
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

            // TODO - Paste visual script

            Update();

            _profileEditorService.SaveSelectedProfileElement();
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
            Update();
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

            EasingTime = (int) Registration.DataBinding.EasingTime.TotalMilliseconds;
            SelectedEasingViewModel = EasingViewModels.First(vm => vm.EasingFunction == Registration.DataBinding.EasingFunction);
            IsDataBindingEnabled = true;
            IsEasingTimeEnabled = EasingTime > 0;

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

        private void UpdateTestResult()
        {
            if (_updating || _updatingTestResult)
                return;

            _updatingTestResult = true;

            if (Registration.DataBinding == null)
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

            TProperty currentValue = Registration.Converter.ConvertFromObject(Registration.DataBinding.Script.Result ?? default(TProperty));
            TestInputValue.UpdateValue(currentValue);
            TestResultValue.UpdateValue(Registration.DataBinding != null ? Registration.DataBinding.GetValue(currentValue) : default);

            _updatingTestResult = false;
        }

        private void EnableDataBinding()
        {
            if (Registration.DataBinding != null)
                return;

            Registration.LayerProperty.EnableDataBinding(Registration);
            NotifyOfPropertyChange(nameof(Script));

            _profileEditorService.SaveSelectedProfileElement();
        }

        private void DisableDataBinding()
        {
            if (Registration.DataBinding == null)
                return;

            Registration.LayerProperty.DisableDataBinding(Registration.DataBinding);
            NotifyOfPropertyChange(nameof(Script));
            Update();

            _profileEditorService.SaveSelectedProfileElement();
        }

        private void OnFrameRendered(object sender, FrameRenderedEventArgs e)
        {
            UpdateTestResult();
            _lastUpdate = DateTime.Now;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _coreService.FrameRendered -= OnFrameRendered;
        }
    }

    public interface IDataBindingViewModel : IScreen, IDisposable
    {
    }
}
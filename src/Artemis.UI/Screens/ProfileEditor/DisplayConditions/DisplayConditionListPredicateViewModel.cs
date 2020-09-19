using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Events;
using Artemis.UI.Exceptions;
using Artemis.UI.Screens.ProfileEditor.DisplayConditions.Abstract;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.UI.Utilities;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.DisplayConditions
{
    public class DisplayConditionListPredicateViewModel : DisplayConditionViewModel, IHandle<MainWindowKeyEvent>, IHandle<MainWindowMouseEvent>, IDisposable
    {
        private readonly IConditionOperatorService _conditionOperatorService;
        private readonly IDataModelUIService _dataModelUIService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IProfileEditorService _profileEditorService;
        private readonly Timer _updateTimer;
        private bool _isInitialized;
        private DataModelVisualizationViewModel _leftSideDataModel;
        private BindableCollection<ConditionOperator> _operators;
        private DataModelVisualizationViewModel _rightSideDataModel;
        private DataModelInputViewModel _rightSideInputViewModel;
        private int _rightSideTransitionIndex;
        private object _rightStaticValue;
        private DataModelVisualizationViewModel _selectedLeftSideProperty;
        private ConditionOperator _selectedOperator;
        private DataModelVisualizationViewModel _selectedRightSideProperty;

        private List<Type> _supportedInputTypes;

        public DisplayConditionListPredicateViewModel(
            DataModelConditionListPredicate dataModelConditionListPredicate,
            IProfileEditorService profileEditorService,
            IDataModelUIService dataModelUIService,
            IConditionOperatorService conditionOperatorService,
            ISettingsService settingsService,
            IEventAggregator eventAggregator) : base(dataModelConditionListPredicate)
        {
            _profileEditorService = profileEditorService;
            _dataModelUIService = dataModelUIService;
            _conditionOperatorService = conditionOperatorService;
            _eventAggregator = eventAggregator;
            _updateTimer = new Timer(500);
            _supportedInputTypes = new List<Type>();

            SelectLeftPropertyCommand = new DelegateCommand(ExecuteSelectLeftProperty);
            SelectRightPropertyCommand = new DelegateCommand(ExecuteSelectRightProperty);
            SelectOperatorCommand = new DelegateCommand(ExecuteSelectOperatorCommand);
            Operators = new BindableCollection<ConditionOperator>();

            ShowDataModelValues = settingsService.GetSetting<bool>("ProfileEditor.ShowDataModelValues");

            // Initialize async, no need to wait for it
            Task.Run(Initialize);
        }

        public DataModelConditionListPredicate DataModelConditionListPredicate => (DataModelConditionListPredicate) Model;
        public bool ShowRightSidePropertySelection => DataModelConditionListPredicate.PredicateType == ProfileRightSideType.Dynamic;
        public bool CanActivateRightSideInputViewModel => SelectedLeftSideProperty?.PropertyInfo != null;
        public PluginSetting<bool> ShowDataModelValues { get; }

        public bool IsInitialized
        {
            get => _isInitialized;
            private set => SetAndNotify(ref _isInitialized, value);
        }

        public bool LeftSideDataModelOpen { get; set; }

        public DataModelVisualizationViewModel LeftSideDataModel
        {
            get => _leftSideDataModel;
            set => SetAndNotify(ref _leftSideDataModel, value);
        }

        public DataModelVisualizationViewModel RightSideDataModel
        {
            get => _rightSideDataModel;
            set => SetAndNotify(ref _rightSideDataModel, value);
        }

        public bool RightSideDataModelOpen { get; set; }

        public DataModelVisualizationViewModel SelectedLeftSideProperty
        {
            get => _selectedLeftSideProperty;
            set
            {
                if (!SetAndNotify(ref _selectedLeftSideProperty, value)) return;
                NotifyOfPropertyChange(nameof(CanActivateRightSideInputViewModel));
            }
        }

        public DataModelVisualizationViewModel SelectedRightSideProperty
        {
            get => _selectedRightSideProperty;
            set => SetAndNotify(ref _selectedRightSideProperty, value);
        }

        public object RightStaticValue
        {
            get => _rightStaticValue;
            set => SetAndNotify(ref _rightStaticValue, value);
        }

        public int RightSideTransitionIndex
        {
            get => _rightSideTransitionIndex;
            set => SetAndNotify(ref _rightSideTransitionIndex, value);
        }

        public DataModelInputViewModel RightSideInputViewModel
        {
            get => _rightSideInputViewModel;
            set => SetAndNotify(ref _rightSideInputViewModel, value);
        }

        public BindableCollection<ConditionOperator> Operators
        {
            get => _operators;
            set => SetAndNotify(ref _operators, value);
        }

        public ConditionOperator SelectedOperator
        {
            get => _selectedOperator;
            set => SetAndNotify(ref _selectedOperator, value);
        }

        public DelegateCommand SelectLeftPropertyCommand { get; }
        public DelegateCommand SelectRightPropertyCommand { get; }
        public DelegateCommand SelectOperatorCommand { get; }

        public void Handle(MainWindowKeyEvent message)
        {
            if (RightSideInputViewModel == null)
                return;

            if (!message.KeyDown && message.EventArgs.Key == Key.Escape)
                RightSideInputViewModel.Cancel();
            if (!message.KeyDown && message.EventArgs.Key == Key.Enter)
                RightSideInputViewModel.Submit();
        }

        public void Handle(MainWindowMouseEvent message)
        {
            if (RightSideInputViewModel == null)
                return;

            if (message.Sender is FrameworkElement frameworkElement && !frameworkElement.IsDescendantOf(RightSideInputViewModel.View))
                RightSideInputViewModel.Submit();
        }

        public override void Delete()
        {
            base.Delete();
            _profileEditorService.UpdateSelectedProfileElement();
        }

        public void Initialize()
        {
            // Get the data models
            LeftSideDataModel = GetListDataModel();
            RightSideDataModel = GetListDataModel();
            LeftSideDataModel.UpdateRequested += LeftDataModelUpdateRequested;
            RightSideDataModel.UpdateRequested += RightDataModelUpdateRequested;

            // Determine which types are currently supported
            var editors = _dataModelUIService.RegisteredDataModelEditors;
            _supportedInputTypes = editors.Select(e => e.SupportedType).ToList();
            _supportedInputTypes.AddRange(editors.Where(e => e.CompatibleConversionTypes != null).SelectMany(e => e.CompatibleConversionTypes));

            Update();

            _updateTimer.Start();
            _updateTimer.Elapsed += OnUpdateTimerOnElapsed;

            IsInitialized = true;
        }

        public override void Update()
        {
            // Not yet initialized if these are null
            if (LeftSideDataModel == null || RightSideDataModel == null)
                return;

            var listDataModelGuid = DataModelConditionListPredicate.ListDataModel.PluginInfo.Guid;

            // If static, only allow selecting properties also supported by input
            if (DataModelConditionListPredicate.PredicateType == ProfileRightSideType.Static)
                LeftSideDataModel.ApplyTypeFilter(false, _supportedInputTypes.ToArray());

            // Determine the left side property first
            SelectedLeftSideProperty = LeftSideDataModel.GetChildByPath(listDataModelGuid, DataModelConditionListPredicate.LeftPropertyPath);
            var leftSideType = SelectedLeftSideProperty?.PropertyInfo?.PropertyType;

            // Get the supported operators
            Operators.Clear();
            Operators.AddRange(_conditionOperatorService.GetConditionOperatorsForType(leftSideType));
            if (DataModelConditionListPredicate.Operator == null)
                DataModelConditionListPredicate.UpdateOperator(Operators.FirstOrDefault(o => o.SupportsType(leftSideType)));
            SelectedOperator = DataModelConditionListPredicate.Operator;

            // Determine the right side
            if (DataModelConditionListPredicate.PredicateType == ProfileRightSideType.Dynamic)
            {
                SelectedRightSideProperty = RightSideDataModel.GetChildByPath(listDataModelGuid, DataModelConditionListPredicate.RightPropertyPath);
                RightSideDataModel.ApplyTypeFilter(true, leftSideType);
            }
            else
                RightStaticValue = DataModelConditionListPredicate.RightStaticValue;
        }

        public void ApplyLeftSide()
        {
            DataModelConditionListPredicate.UpdateLeftSide(SelectedLeftSideProperty.PropertyPath);
            _profileEditorService.UpdateSelectedProfileElement();

            SelectedOperator = DataModelConditionListPredicate.Operator;
            Update();
        }

        public void ApplyRightSideDynamic()
        {
            DataModelConditionListPredicate.UpdateRightSideDynamic(SelectedRightSideProperty.PropertyPath);
            _profileEditorService.UpdateSelectedProfileElement();

            Update();
        }

        public void ApplyRightSideStatic(object value, bool isSubmitted)
        {
            if (isSubmitted)
            {
                DataModelConditionListPredicate.UpdateRightSideStatic(value);
                _profileEditorService.UpdateSelectedProfileElement();

                Update();
            }

            RightSideTransitionIndex = 0;
            RightSideInputViewModel = null;
            RightStaticValue = value;
            _eventAggregator.Unsubscribe(this);
        }

        public void ApplyOperator()
        {
            DataModelConditionListPredicate.UpdateOperator(SelectedOperator);
            _profileEditorService.UpdateSelectedProfileElement();

            Update();
        }

        public void ActivateRightSideInputViewModel()
        {
            if (SelectedLeftSideProperty?.PropertyInfo == null)
                return;

            RightSideTransitionIndex = 1;
            RightSideInputViewModel = _dataModelUIService.GetDataModelInputViewModel(
                SelectedLeftSideProperty.PropertyInfo.PropertyType,
                SelectedLeftSideProperty.PropertyDescription,
                DataModelConditionListPredicate.RightStaticValue,
                ApplyRightSideStatic
            );
            _eventAggregator.Subscribe(this);
        }

        private void OnUpdateTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            if (LeftSideDataModelOpen)
                LeftSideDataModel.Update(_dataModelUIService);
            else if (RightSideDataModelOpen)
                RightSideDataModel.Update(_dataModelUIService);
        }

        private void RightDataModelUpdateRequested(object sender, EventArgs e)
        {
            var listDataModelGuid = DataModelConditionListPredicate.ListDataModel.PluginInfo.Guid;
            var leftSideType = SelectedLeftSideProperty?.PropertyInfo?.PropertyType;

            // If the right side property is missing it may be available now that the data model has been updated
            if (SelectedRightSideProperty == null && DataModelConditionListPredicate.RightPropertyPath != null)
                SelectedRightSideProperty = RightSideDataModel.GetChildByPath(listDataModelGuid, DataModelConditionListPredicate.RightPropertyPath);

            // With the data model updated, also reapply the filter
            RightSideDataModel.ApplyTypeFilter(true, leftSideType);
        }

        private void LeftDataModelUpdateRequested(object sender, EventArgs e)
        {
            if (DataModelConditionListPredicate.PredicateType == ProfileRightSideType.Static)
                LeftSideDataModel.ApplyTypeFilter(false, _supportedInputTypes.ToArray());
        }

        private DataModelVisualizationViewModel GetListDataModel()
        {
            if (DataModelConditionListPredicate.ListDataModel == null || DataModelConditionListPredicate.ListPropertyPath == null)
                throw new ArtemisUIException("Cannot create a list predicate without first selecting a target list");

            var dataModel = _dataModelUIService.GetPluginDataModelVisualization(_profileEditorService.GetCurrentModule(), true);
            var listDataModel = (DataModelListViewModel) dataModel.GetChildByPath(
                DataModelConditionListPredicate.ListDataModel.PluginInfo.Guid,
                DataModelConditionListPredicate.ListPropertyPath
            );

            return listDataModel.GetListTypeViewModel(_dataModelUIService);
        }

        private void ExecuteSelectLeftProperty(object context)
        {
            if (!(context is DataModelVisualizationViewModel dataModelVisualizationViewModel))
                return;

            SelectedLeftSideProperty = dataModelVisualizationViewModel;
            ApplyLeftSide();
        }

        private void ExecuteSelectRightProperty(object context)
        {
            if (!(context is DataModelVisualizationViewModel dataModelVisualizationViewModel))
                return;

            SelectedRightSideProperty = dataModelVisualizationViewModel;
            ApplyRightSideDynamic();
        }

        private void ExecuteSelectOperatorCommand(object context)
        {
            if (!(context is ConditionOperator displayConditionOperator))
                return;

            SelectedOperator = displayConditionOperator;
            ApplyOperator();
        }

        public void Dispose()
        {
            _updateTimer.Dispose();
            _updateTimer.Elapsed -= OnUpdateTimerOnElapsed;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using Artemis.Core.Models.Profile.Conditions;
using Artemis.Core.Plugins.Models;
using Artemis.Core.Services;
using Artemis.Core.Services.Interfaces;
using Artemis.UI.Events;
using Artemis.UI.Exceptions;
using Artemis.UI.Screens.ProfileEditor.DisplayConditions.Abstract;
using Artemis.UI.Shared.DataModelVisualization;
using Artemis.UI.Shared.DataModelVisualization.Shared;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Interfaces;
using Artemis.UI.Utilities;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.DisplayConditions
{
    public class DisplayConditionListPredicateViewModel : DisplayConditionViewModel, IHandle<MainWindowKeyEvent>, IHandle<MainWindowMouseEvent>
    {
        private readonly IDataModelService _dataModelService;
        private readonly IDataModelVisualizationService _dataModelVisualizationService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IProfileEditorService _profileEditorService;
        private readonly Timer _updateTimer;
        private bool _isInitialized;
        private DataModelVisualizationViewModel _leftSideDataModel;
        private BindableCollection<DisplayConditionOperator> _operators;
        private DataModelVisualizationViewModel _rightSideDataModel;
        private DataModelInputViewModel _rightSideInputViewModel;
        private int _rightSideTransitionIndex;
        private object _rightStaticValue;
        private DataModelVisualizationViewModel _selectedLeftSideProperty;
        private DisplayConditionOperator _selectedOperator;
        private DataModelVisualizationViewModel _selectedRightSideProperty;

        private List<Type> _supportedInputTypes;

        public DisplayConditionListPredicateViewModel(
            DisplayConditionListPredicate displayConditionListPredicate,
            DisplayConditionViewModel parent,
            IProfileEditorService profileEditorService,
            IDataModelVisualizationService dataModelVisualizationService,
            IDataModelService dataModelService,
            ISettingsService settingsService,
            IEventAggregator eventAggregator) : base(displayConditionListPredicate, parent)
        {
            _profileEditorService = profileEditorService;
            _dataModelVisualizationService = dataModelVisualizationService;
            _dataModelService = dataModelService;
            _eventAggregator = eventAggregator;
            _updateTimer = new Timer(500);
            _supportedInputTypes = new List<Type>();

            SelectLeftPropertyCommand = new DelegateCommand(ExecuteSelectLeftProperty);
            SelectRightPropertyCommand = new DelegateCommand(ExecuteSelectRightProperty);
            SelectOperatorCommand = new DelegateCommand(ExecuteSelectOperatorCommand);
            Operators = new BindableCollection<DisplayConditionOperator>();

            ShowDataModelValues = settingsService.GetSetting<bool>("ProfileEditor.ShowDataModelValues");

            // Initialize async, no need to wait for it
            Task.Run(Initialize);
        }

        public DisplayConditionListPredicate DisplayConditionListPredicate => (DisplayConditionListPredicate) Model;
        public bool ShowRightSidePropertySelection => DisplayConditionListPredicate.PredicateType == PredicateType.Dynamic;
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

        public BindableCollection<DisplayConditionOperator> Operators
        {
            get => _operators;
            set => SetAndNotify(ref _operators, value);
        }

        public DisplayConditionOperator SelectedOperator
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
            var editors = _dataModelVisualizationService.RegisteredDataModelEditors;
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

            var listDataModelGuid = DisplayConditionListPredicate.ListDataModel.PluginInfo.Guid;

            // If static, only allow selecting properties also supported by input
            if (DisplayConditionListPredicate.PredicateType == PredicateType.Static)
                LeftSideDataModel.ApplyTypeFilter(false, _supportedInputTypes.ToArray());

            // Determine the left side property first
            SelectedLeftSideProperty = LeftSideDataModel.GetChildByPath(listDataModelGuid, DisplayConditionListPredicate.LeftPropertyPath);
            var leftSideType = SelectedLeftSideProperty?.PropertyInfo?.PropertyType;

            // Get the supported operators
            Operators.Clear();
            Operators.AddRange(_dataModelService.GetCompatibleConditionOperators(leftSideType));
            if (DisplayConditionListPredicate.Operator == null)
                DisplayConditionListPredicate.UpdateOperator(Operators.FirstOrDefault(o => o.SupportsType(leftSideType)));
            SelectedOperator = DisplayConditionListPredicate.Operator;

            // Determine the right side
            if (DisplayConditionListPredicate.PredicateType == PredicateType.Dynamic)
            {
                SelectedRightSideProperty = RightSideDataModel.GetChildByPath(listDataModelGuid, DisplayConditionListPredicate.RightPropertyPath);
                RightSideDataModel.ApplyTypeFilter(true, leftSideType);
            }
            else
                RightStaticValue = DisplayConditionListPredicate.RightStaticValue;
        }

        public void ApplyLeftSide()
        {
            DisplayConditionListPredicate.UpdateLeftSide(SelectedLeftSideProperty.PropertyPath);
            _profileEditorService.UpdateSelectedProfileElement();

            SelectedOperator = DisplayConditionListPredicate.Operator;
            Update();
        }

        public void ApplyRightSideDynamic()
        {
            DisplayConditionListPredicate.UpdateRightSideDynamic(SelectedRightSideProperty.PropertyPath);
            _profileEditorService.UpdateSelectedProfileElement();

            Update();
        }

        public void ApplyRightSideStatic(object value, bool isSubmitted)
        {
            if (isSubmitted)
            {
                DisplayConditionListPredicate.UpdateRightSideStatic(value);
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
            DisplayConditionListPredicate.UpdateOperator(SelectedOperator);
            _profileEditorService.UpdateSelectedProfileElement();

            Update();
        }

        public void ActivateRightSideInputViewModel()
        {
            if (SelectedLeftSideProperty?.PropertyInfo == null)
                return;

            RightSideTransitionIndex = 1;
            RightSideInputViewModel = _dataModelVisualizationService.GetDataModelInputViewModel(
                SelectedLeftSideProperty.PropertyInfo.PropertyType,
                SelectedLeftSideProperty.PropertyDescription,
                DisplayConditionListPredicate.RightStaticValue,
                ApplyRightSideStatic
            );
            _eventAggregator.Subscribe(this);
        }

        protected override void Dispose(bool disposing)
        {
            _updateTimer.Stop();
            _updateTimer.Elapsed -= OnUpdateTimerOnElapsed;
        }

        private void OnUpdateTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            if (LeftSideDataModelOpen)
                LeftSideDataModel.Update(_dataModelVisualizationService);
            else if (RightSideDataModelOpen)
                RightSideDataModel.Update(_dataModelVisualizationService);
        }

        private void RightDataModelUpdateRequested(object sender, EventArgs e)
        {
            var listDataModelGuid = DisplayConditionListPredicate.ListDataModel.PluginInfo.Guid;
            var leftSideType = SelectedLeftSideProperty?.PropertyInfo?.PropertyType;

            // If the right side property is missing it may be available now that the data model has been updated
            if (SelectedRightSideProperty == null && DisplayConditionListPredicate.RightPropertyPath != null)
                SelectedRightSideProperty = RightSideDataModel.GetChildByPath(listDataModelGuid, DisplayConditionListPredicate.RightPropertyPath);

            // With the data model updated, also reapply the filter
            RightSideDataModel.ApplyTypeFilter(true, leftSideType);
        }

        private void LeftDataModelUpdateRequested(object sender, EventArgs e)
        {
            if (DisplayConditionListPredicate.PredicateType == PredicateType.Static)
                LeftSideDataModel.ApplyTypeFilter(false, _supportedInputTypes.ToArray());
        }

        private DataModelVisualizationViewModel GetListDataModel()
        {
            if (DisplayConditionListPredicate.ListDataModel == null || DisplayConditionListPredicate.ListPropertyPath == null)
                throw new ArtemisUIException("Cannot create a list predicate without first selecting a target list");

            var dataModel = _dataModelVisualizationService.GetMainDataModelVisualization();
            if (!_dataModelVisualizationService.GetPluginExtendsDataModel(_profileEditorService.GetCurrentModule()))
                dataModel.Children.Add(_dataModelVisualizationService.GetPluginDataModelVisualization(_profileEditorService.GetCurrentModule()));

            var listDataModel = (DataModelListViewModel) dataModel.GetChildByPath(
                DisplayConditionListPredicate.ListDataModel.PluginInfo.Guid,
                DisplayConditionListPredicate.ListPropertyPath
            );

            return listDataModel.GetListTypeViewModel(_dataModelVisualizationService);
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
            if (!(context is DisplayConditionOperator displayConditionOperator))
                return;

            SelectedOperator = displayConditionOperator;
            ApplyOperator();
        }
    }
}
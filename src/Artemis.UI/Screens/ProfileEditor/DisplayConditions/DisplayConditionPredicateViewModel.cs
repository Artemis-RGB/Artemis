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
using Artemis.UI.Screens.ProfileEditor.DisplayConditions.Abstract;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.UI.Utilities;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.DisplayConditions
{
    public class DisplayConditionPredicateViewModel : DisplayConditionViewModel, IHandle<MainWindowKeyEvent>, IHandle<MainWindowMouseEvent>
    {
        private readonly IDataModelService _dataModelService;
        private readonly IDataModelUIService _dataModelUIService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IProfileEditorService _profileEditorService;
        private readonly Timer _updateTimer;
        private bool _isInitialized;
        private DataModelPropertiesViewModel _leftSideDataModel;
        private BindableCollection<DisplayConditionOperator> _operators;
        private DataModelPropertiesViewModel _rightSideDataModel;
        private DataModelInputViewModel _rightSideInputViewModel;
        private int _rightSideTransitionIndex;
        private object _rightStaticValue;
        private DataModelVisualizationViewModel _selectedLeftSideProperty;
        private DisplayConditionOperator _selectedOperator;
        private DataModelVisualizationViewModel _selectedRightSideProperty;

        private List<Type> _supportedInputTypes;

        public DisplayConditionPredicateViewModel(
            DisplayConditionPredicate displayConditionPredicate,
            DisplayConditionViewModel parent,
            IProfileEditorService profileEditorService,
            IDataModelUIService dataModelUIService,
            IDataModelService dataModelService,
            ISettingsService settingsService,
            IEventAggregator eventAggregator) : base(displayConditionPredicate, parent)
        {
            _profileEditorService = profileEditorService;
            _dataModelUIService = dataModelUIService;
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

        public DisplayConditionPredicate DisplayConditionPredicate => (DisplayConditionPredicate) Model;
        public bool ShowRightSidePropertySelection => DisplayConditionPredicate.PredicateType == ProfileRightSideType.Dynamic;
        public bool CanActivateRightSideInputViewModel => SelectedLeftSideProperty?.PropertyInfo != null;
        public PluginSetting<bool> ShowDataModelValues { get; }

        public bool IsInitialized
        {
            get => _isInitialized;
            private set => SetAndNotify(ref _isInitialized, value);
        }

        public bool LeftSideDataModelOpen { get; set; }

        public DataModelPropertiesViewModel LeftSideDataModel
        {
            get => _leftSideDataModel;
            set => SetAndNotify(ref _leftSideDataModel, value);
        }

        public DataModelPropertiesViewModel RightSideDataModel
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
            LeftSideDataModel = _dataModelUIService.GetMainDataModelVisualization();
            RightSideDataModel = _dataModelUIService.GetMainDataModelVisualization();
            if (!_dataModelUIService.GetPluginExtendsDataModel(_profileEditorService.GetCurrentModule()))
            {
                LeftSideDataModel.Children.Add(_dataModelUIService.GetPluginDataModelVisualization(_profileEditorService.GetCurrentModule()));
                RightSideDataModel.Children.Add(_dataModelUIService.GetPluginDataModelVisualization(_profileEditorService.GetCurrentModule()));
            }

            // Determine which types are currently supported
            var editors = _dataModelUIService.RegisteredDataModelEditors;
            _supportedInputTypes = editors.Select(e => e.SupportedType).ToList();
            _supportedInputTypes.AddRange(editors.Where(e => e.CompatibleConversionTypes != null).SelectMany(e => e.CompatibleConversionTypes));

            LeftSideDataModel.UpdateRequested += LeftDataModelUpdateRequested;
            RightSideDataModel.UpdateRequested += RightDataModelUpdateRequested;

            Update();

            _updateTimer.Start();
            _updateTimer.Elapsed += OnUpdateTimerOnElapsed;

            IsInitialized = true;
        }

        public override void Update()
        {
            if (LeftSideDataModel == null || DisplayConditionPredicate.PredicateType == ProfileRightSideType.Dynamic && RightSideDataModel == null)
                return;

            // If static, only allow selecting properties also supported by input
            if (DisplayConditionPredicate.PredicateType == ProfileRightSideType.Static)
                LeftSideDataModel.ApplyTypeFilter(false, _supportedInputTypes.ToArray());

            // Determine the left side property first
            SelectedLeftSideProperty = LeftSideDataModel.GetChildForCondition(DisplayConditionPredicate, DisplayConditionSide.Left);
            var leftSideType = SelectedLeftSideProperty?.PropertyInfo?.PropertyType;

            // Get the supported operators
            Operators.Clear();
            Operators.AddRange(_dataModelService.GetCompatibleConditionOperators(leftSideType));
            if (DisplayConditionPredicate.Operator == null)
                DisplayConditionPredicate.UpdateOperator(Operators.FirstOrDefault(o => o.SupportsType(leftSideType)));
            SelectedOperator = DisplayConditionPredicate.Operator;

            // Determine the right side
            if (DisplayConditionPredicate.PredicateType == ProfileRightSideType.Dynamic)
            {
                SelectedRightSideProperty = LeftSideDataModel.GetChildForCondition(DisplayConditionPredicate, DisplayConditionSide.Right);
                RightSideDataModel.ApplyTypeFilter(true, leftSideType);
            }
            else
                RightStaticValue = DisplayConditionPredicate.RightStaticValue;
        }

        public void ApplyLeftSide()
        {
            DisplayConditionPredicate.UpdateLeftSide(SelectedLeftSideProperty.DataModel, SelectedLeftSideProperty.PropertyPath);
            _profileEditorService.UpdateSelectedProfileElement();

            SelectedOperator = DisplayConditionPredicate.Operator;
            Update();
        }

        public void ApplyRightSideDynamic()
        {
            DisplayConditionPredicate.UpdateRightSide(SelectedRightSideProperty.DataModel, SelectedRightSideProperty.PropertyPath);
            _profileEditorService.UpdateSelectedProfileElement();

            Update();
        }

        public void ApplyRightSideStatic(object value, bool isSubmitted)
        {
            if (isSubmitted)
            {
                DisplayConditionPredicate.UpdateRightSide(value);
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
            DisplayConditionPredicate.UpdateOperator(SelectedOperator);
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
                DisplayConditionPredicate.RightStaticValue,
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
                LeftSideDataModel.Update(_dataModelUIService);
            else if (RightSideDataModelOpen)
                RightSideDataModel.Update(_dataModelUIService);
        }

        private void RightDataModelUpdateRequested(object sender, EventArgs e)
        {
            var leftSideType = SelectedLeftSideProperty?.PropertyInfo?.PropertyType;
            if (DisplayConditionPredicate.PredicateType == ProfileRightSideType.Dynamic)
                SelectedRightSideProperty = LeftSideDataModel.GetChildForCondition(DisplayConditionPredicate, DisplayConditionSide.Right);

            // With the data model updated, also reapply the filter
            RightSideDataModel.ApplyTypeFilter(true, leftSideType);
        }

        private void LeftDataModelUpdateRequested(object sender, EventArgs e)
        {
            if (DisplayConditionPredicate.PredicateType == ProfileRightSideType.Static)
                LeftSideDataModel.ApplyTypeFilter(false, _supportedInputTypes.ToArray());
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
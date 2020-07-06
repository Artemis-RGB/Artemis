using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core.Models.Profile.Conditions;
using Artemis.Core.Services.Interfaces;
using Artemis.UI.Screens.Module.ProfileEditor.DisplayConditions.Abstract;
using Artemis.UI.Shared.DataModelVisualization;
using Artemis.UI.Shared.DataModelVisualization.Shared;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Interfaces;
using Artemis.UI.Utilities;

namespace Artemis.UI.Screens.Module.ProfileEditor.DisplayConditions
{
    public class DisplayConditionPredicateViewModel : DisplayConditionViewModel
    {
        private readonly IDataModelService _dataModelService;
        private readonly IDataModelVisualizationService _dataModelVisualizationService;
        private readonly IProfileEditorService _profileEditorService;
        private DataModelPropertiesViewModel _leftSideDataModel;
        private List<DisplayConditionOperator> _operators;
        private DataModelPropertiesViewModel _rightSideDataModel;
        private DataModelInputViewModel _rightSideInputViewModel;
        private int _rightSideTransitionIndex;
        private DataModelVisualizationViewModel _selectedLeftSideProperty;
        private DataModelVisualizationViewModel _selectedRightSideProperty;

        private List<Type> _supportedInputTypes;

        public DisplayConditionPredicateViewModel(DisplayConditionPredicate displayConditionPredicate, DisplayConditionViewModel parent,
            IProfileEditorService profileEditorService, IDataModelVisualizationService dataModelVisualizationService, IDataModelService dataModelService)
            : base(displayConditionPredicate, parent)
        {
            _profileEditorService = profileEditorService;
            _dataModelVisualizationService = dataModelVisualizationService;
            _dataModelService = dataModelService;

            SelectLeftPropertyCommand = new DelegateCommand(ExecuteSelectLeftProperty);
            SelectRightPropertyCommand = new DelegateCommand(ExecuteSelectRightProperty);
            SelectOperatorCommand = new DelegateCommand(ExecuteSelectOperatorCommand);

            Initialize();
        }

        public DisplayConditionPredicate DisplayConditionPredicate => (DisplayConditionPredicate) Model;
        public DelegateCommand SelectLeftPropertyCommand { get; }
        public DelegateCommand SelectRightPropertyCommand { get; }
        public DelegateCommand SelectOperatorCommand { get; }
        public bool ShowRightSidePropertySelection => DisplayConditionPredicate.PredicateType == PredicateType.Dynamic;

        public bool IsInitialized { get; private set; }

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

        public DataModelVisualizationViewModel SelectedLeftSideProperty
        {
            get => _selectedLeftSideProperty;
            set => SetAndNotify(ref _selectedLeftSideProperty, value);
        }

        public DataModelVisualizationViewModel SelectedRightSideProperty
        {
            get => _selectedRightSideProperty;
            set => SetAndNotify(ref _selectedRightSideProperty, value);
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

        public List<DisplayConditionOperator> Operators
        {
            get => _operators;
            set => SetAndNotify(ref _operators, value);
        }

        public void Initialize()
        {
            Task.Run(() =>
            {
                // Get the data models
                LeftSideDataModel = _dataModelVisualizationService.GetMainDataModelVisualization();
                RightSideDataModel = _dataModelVisualizationService.GetMainDataModelVisualization();
                if (!_dataModelVisualizationService.GetPluginExtendsDataModel(_profileEditorService.GetCurrentModule()))
                {
                    LeftSideDataModel.Children.Add(_dataModelVisualizationService.GetPluginDataModelVisualization(_profileEditorService.GetCurrentModule()));
                    RightSideDataModel.Children.Add(_dataModelVisualizationService.GetPluginDataModelVisualization(_profileEditorService.GetCurrentModule()));
                }

                // Determine which types are currently supported
                _supportedInputTypes = _dataModelVisualizationService.RegisteredDataModelEditors.Select(e => e.SupportedType).ToList();

                IsInitialized = true;
                Update();
            });
        }

        public override void Update()
        {
            if (!IsInitialized)
                return;

            // If static, only allow selecting properties also supported by input
            if (DisplayConditionPredicate.PredicateType == PredicateType.Static)
                LeftSideDataModel.ApplyTypeFilter(_supportedInputTypes.ToArray());

            // Determine the left side property first
            SelectedLeftSideProperty = DisplayConditionPredicate.LeftPropertyPath != null
                ? LeftSideDataModel.GetChildByPath(DisplayConditionPredicate.LeftDataModelGuid, DisplayConditionPredicate.LeftPropertyPath)
                : null;
            var leftSideType = SelectedLeftSideProperty?.PropertyInfo?.PropertyType;

            // Right side may only select properties matching the left side
            if (SelectedLeftSideProperty != null)
                RightSideDataModel.ApplyTypeFilter(leftSideType);
            else
                RightSideDataModel.ApplyTypeFilter();

            // Determine the right side property first
            if (DisplayConditionPredicate.RightPropertyPath != null)
            {
                // Ensure the right side property still matches the left side type, else set it to null
                var selectedProperty = RightSideDataModel.GetChildByPath(DisplayConditionPredicate.RightDataModelGuid, DisplayConditionPredicate.RightPropertyPath);
                SelectedRightSideProperty = selectedProperty.IsMatchingFilteredTypes ? selectedProperty : null;
            }
            else
                SelectedRightSideProperty = null;

            // Get the supported operators
            Operators = _dataModelService.GetCompatibleConditionOperators(leftSideType);
            if (DisplayConditionPredicate.Operator == null || !DisplayConditionPredicate.Operator.SupportsType(leftSideType))
                DisplayConditionPredicate.Operator = Operators.FirstOrDefault(o => o.SupportsType(leftSideType));

            NotifyOfPropertyChange(nameof(DisplayConditionPredicate));
        }

        public void ActivateRightSideInputViewModel()
        {
            RightSideTransitionIndex = 1;
        }

        private void ExecuteSelectLeftProperty(object context)
        {
            if (!(context is DataModelVisualizationViewModel vm))
                return;

            DisplayConditionPredicate.LeftPropertyPath = vm.GetCurrentPath();
            DisplayConditionPredicate.LeftDataModelGuid = vm.DataModel.PluginInfo.Guid;
            Update();
        }

        private void ExecuteSelectRightProperty(object context)
        {
            if (!(context is DataModelVisualizationViewModel vm))
                return;

            DisplayConditionPredicate.RightPropertyPath = vm.GetCurrentPath();
            DisplayConditionPredicate.RightDataModelGuid = vm.DataModel.PluginInfo.Guid;
            Update();
        }

        private void ExecuteSelectOperatorCommand(object context)
        {
            if (!(context is DisplayConditionOperator displayConditionOperator))
                return;

            DisplayConditionPredicate.Operator = displayConditionOperator;
            Update();
        }
    }
}
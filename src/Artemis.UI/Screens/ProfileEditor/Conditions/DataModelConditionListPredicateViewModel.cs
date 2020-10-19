using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Exceptions;
using Artemis.UI.Screens.ProfileEditor.Conditions.Abstract;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Input;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.Conditions
{
    public class DataModelConditionListPredicateViewModel : DataModelConditionViewModel, IDisposable
    {
        private readonly IConditionOperatorService _conditionOperatorService;
        private readonly IDataModelUIService _dataModelUIService;
        private readonly IProfileEditorService _profileEditorService;
        private bool _isPrimitiveList;
        private DataModelDynamicViewModel _leftSideSelectionViewModel;
        private BindableCollection<BaseConditionOperator> _operators;
        private DataModelStaticViewModel _rightSideInputViewModel;
        private DataModelDynamicViewModel _rightSideSelectionViewModel;
        private BaseConditionOperator _selectedOperator;

        private List<Type> _supportedInputTypes;

        public DataModelConditionListPredicateViewModel(
            DataModelConditionListPredicate dataModelConditionListPredicate,
            IProfileEditorService profileEditorService,
            IDataModelUIService dataModelUIService,
            IConditionOperatorService conditionOperatorService) : base(dataModelConditionListPredicate)
        {
            _profileEditorService = profileEditorService;
            _dataModelUIService = dataModelUIService;
            _conditionOperatorService = conditionOperatorService;
            _supportedInputTypes = new List<Type>();

            SelectOperatorCommand = new DelegateCommand(ExecuteSelectOperatorCommand);
            Operators = new BindableCollection<BaseConditionOperator>();

            Initialize();
        }

        public DataModelConditionListPredicate DataModelConditionListPredicate => (DataModelConditionListPredicate) Model;

        public BindableCollection<BaseConditionOperator> Operators
        {
            get => _operators;
            set => SetAndNotify(ref _operators, value);
        }

        public DataModelDynamicViewModel LeftSideSelectionViewModel
        {
            get => _leftSideSelectionViewModel;
            set => SetAndNotify(ref _leftSideSelectionViewModel, value);
        }

        public DataModelDynamicViewModel RightSideSelectionViewModel
        {
            get => _rightSideSelectionViewModel;
            set => SetAndNotify(ref _rightSideSelectionViewModel, value);
        }

        public DataModelStaticViewModel RightSideInputViewModel
        {
            get => _rightSideInputViewModel;
            set => SetAndNotify(ref _rightSideInputViewModel, value);
        }

        public BaseConditionOperator SelectedOperator
        {
            get => _selectedOperator;
            set => SetAndNotify(ref _selectedOperator, value);
        }

        public DelegateCommand SelectOperatorCommand { get; }

        public override void Delete()
        {
            base.Delete();
            _profileEditorService.UpdateSelectedProfileElement();
        }

        public void Initialize()
        {
            DataModelPropertiesViewModel listDataModel = GetListDataModel();
            if (listDataModel.Children.Count == 1 && listDataModel.Children.First() is DataModelListPropertyViewModel)
                _isPrimitiveList = true;
            else
                _isPrimitiveList = false;

            // Get the data models
            if (!_isPrimitiveList)
            {
                LeftSideSelectionViewModel = _dataModelUIService.GetDynamicSelectionViewModel(_profileEditorService.GetCurrentModule());
                LeftSideSelectionViewModel.ChangeDataModel(listDataModel);
                LeftSideSelectionViewModel.PropertySelected += LeftSideOnPropertySelected;
            }

            // Determine which types are currently supported
            IReadOnlyCollection<DataModelVisualizationRegistration> editors = _dataModelUIService.RegisteredDataModelEditors;
            _supportedInputTypes = editors.Select(e => e.SupportedType).ToList();
            _supportedInputTypes.AddRange(editors.Where(e => e.CompatibleConversionTypes != null).SelectMany(e => e.CompatibleConversionTypes));

            Update();
        }

        public override void Update()
        {
            Guid? listDataModelGuid = DataModelConditionListPredicate.DataModelConditionList.ListPath?.DataModelGuid;
            if (listDataModelGuid == null)
                return;

            if (!_isPrimitiveList)
            {
                // Lists use a different color
                LeftSideSelectionViewModel.ButtonBrush = new SolidColorBrush(Color.FromRgb(71, 108, 188));
                LeftSideSelectionViewModel.FilterTypes = _supportedInputTypes.ToArray();
                LeftSideSelectionViewModel.ChangeDataModelPath(DataModelConditionListPredicate.LeftPath);
            }

            Type leftSideType = _isPrimitiveList
                ? DataModelConditionListPredicate.DataModelConditionList.ListType
                : LeftSideSelectionViewModel.DataModelPath?.GetPropertyType();

            // Get the supported operators
            Operators.Clear();
            Operators.AddRange(_conditionOperatorService.GetConditionOperatorsForType(leftSideType ?? typeof(object), ConditionParameterSide.Left));
            if (DataModelConditionListPredicate.Operator == null)
                DataModelConditionListPredicate.UpdateOperator(Operators.FirstOrDefault(o => o.SupportsType(leftSideType ?? typeof(object), ConditionParameterSide.Left)));
            SelectedOperator = DataModelConditionListPredicate.Operator;

            // Without a selected operator or one that supports a right side, leave the right side input empty
            if (SelectedOperator == null || SelectedOperator.RightSideType == null)
            {
                DisposeRightSideStaticViewModel();
                DisposeRightSideDynamicViewModel();
                return;
            }

            // Ensure the right side has the proper VM
            if (DataModelConditionListPredicate.PredicateType == ProfileRightSideType.Dynamic)
            {
                DisposeRightSideStaticViewModel();
                if (RightSideSelectionViewModel == null)
                    CreateRightSideSelectionViewModel();

                RightSideSelectionViewModel.ChangeDataModelPath(DataModelConditionListPredicate.RightPath);
                RightSideSelectionViewModel.FilterTypes = new[] {SelectedOperator.RightSideType};
            }
            else
            {
                DisposeRightSideDynamicViewModel();
                if (RightSideInputViewModel == null)
                    CreateRightSideInputViewModel(SelectedOperator.RightSideType);

                if (SelectedOperator.RightSideType.IsValueType && DataModelConditionListPredicate.RightStaticValue == null)
                    RightSideInputViewModel.Value = SelectedOperator.RightSideType.GetDefault();
                else
                    RightSideInputViewModel.Value = DataModelConditionListPredicate.RightStaticValue;
                if (RightSideInputViewModel.TargetType != SelectedOperator.RightSideType)
                    RightSideInputViewModel.UpdateTargetType(SelectedOperator.RightSideType);
            }
        }

        public void ApplyLeftSide()
        {
            DataModelConditionListPredicate.UpdateLeftSide(LeftSideSelectionViewModel.DataModelPath);
            _profileEditorService.UpdateSelectedProfileElement();

            SelectedOperator = DataModelConditionListPredicate.Operator;
            Update();
        }

        public void ApplyRightSideDynamic()
        {
            DataModelConditionListPredicate.UpdateRightSideDynamic(RightSideSelectionViewModel.DataModelPath);
            _profileEditorService.UpdateSelectedProfileElement();

            Update();
        }

        public void ApplyRightSideStatic(object value)
        {
            DataModelConditionListPredicate.UpdateRightSideStatic(value);
            _profileEditorService.UpdateSelectedProfileElement();

            Update();
        }

        public void ApplyOperator()
        {
            DataModelConditionListPredicate.UpdateOperator(SelectedOperator);
            _profileEditorService.UpdateSelectedProfileElement();

            Update();
        }

        private DataModelPropertiesViewModel GetListDataModel()
        {
            if (DataModelConditionListPredicate.DataModelConditionList.ListPath?.DataModelGuid == null)
                throw new ArtemisUIException("Failed to retrieve the list data model VM for this list predicate because it has no list path");

            DataModelPropertiesViewModel dataModel = _dataModelUIService.GetPluginDataModelVisualization(_profileEditorService.GetCurrentModule(), true);
            DataModelListViewModel listDataModel = (DataModelListViewModel) dataModel.GetChildByPath(
                DataModelConditionListPredicate.DataModelConditionList.ListPath.DataModelGuid.Value,
                DataModelConditionListPredicate.DataModelConditionList.ListPath.Path
            );

            return listDataModel.GetListTypeViewModel(_dataModelUIService);
        }

        private void ExecuteSelectOperatorCommand(object context)
        {
            if (!(context is BaseConditionOperator dataModelConditionOperator))
                return;

            SelectedOperator = dataModelConditionOperator;
            ApplyOperator();
        }

        #region IDisposable

        public void Dispose()
        {
            if (!_isPrimitiveList)
            {
                LeftSideSelectionViewModel.PropertySelected -= LeftSideOnPropertySelected;
                LeftSideSelectionViewModel.Dispose();
            }

            DisposeRightSideStaticViewModel();
            DisposeRightSideDynamicViewModel();
        }

        #endregion

        #region Event handlers

        private void LeftSideOnPropertySelected(object sender, DataModelInputDynamicEventArgs e)
        {
            ApplyLeftSide();
        }

        private void RightSideOnPropertySelected(object sender, DataModelInputDynamicEventArgs e)
        {
            ApplyRightSideDynamic();
        }

        private void RightSideOnValueEntered(object sender, DataModelInputStaticEventArgs e)
        {
            ApplyRightSideStatic(e.Value);
        }

        private void RightSideSelectionViewModelOnSwitchToStaticRequested(object sender, EventArgs e)
        {
            DataModelConditionListPredicate.PredicateType = ProfileRightSideType.Static;
            Update();
        }

        private void RightSideInputViewModelOnSwitchToDynamicRequested(object? sender, EventArgs e)
        {
            DataModelConditionListPredicate.PredicateType = ProfileRightSideType.Dynamic;
            Update();
        }

        #endregion

        #region View model management

        private void CreateRightSideSelectionViewModel()
        {
            RightSideSelectionViewModel = _dataModelUIService.GetDynamicSelectionViewModel(_profileEditorService.GetCurrentModule());
            RightSideSelectionViewModel.ButtonBrush = (Brush) Application.Current.FindResource("PrimaryHueMidBrush");
            RightSideSelectionViewModel.DisplaySwitchButton = true;
            RightSideSelectionViewModel.PropertySelected += RightSideOnPropertySelected;
            RightSideSelectionViewModel.SwitchToStaticRequested += RightSideSelectionViewModelOnSwitchToStaticRequested;

            // Add an extra data model to the selection VM to allow self-referencing the current item
            // The safe cast prevents adding this extra VM on primitive lists where they serve no purpose
            if (GetListDataModel()?.Children?.FirstOrDefault() is DataModelPropertiesViewModel listValue)
                RightSideSelectionViewModel.ExtraDataModelViewModels.Add(listValue);
        }

        private void CreateRightSideInputViewModel(Type leftSideType)
        {
            RightSideInputViewModel = _dataModelUIService.GetStaticInputViewModel(leftSideType, LeftSideSelectionViewModel.DataModelPath?.GetPropertyDescription());
            RightSideInputViewModel.ButtonBrush = (Brush) Application.Current.FindResource("PrimaryHueMidBrush");
            RightSideInputViewModel.ValueUpdated += RightSideOnValueEntered;
            RightSideInputViewModel.SwitchToDynamicRequested += RightSideInputViewModelOnSwitchToDynamicRequested;
        }

        private void DisposeRightSideStaticViewModel()
        {
            if (RightSideInputViewModel == null)
                return;
            RightSideInputViewModel.ValueUpdated -= RightSideOnValueEntered;
            RightSideInputViewModel.SwitchToDynamicRequested -= RightSideInputViewModelOnSwitchToDynamicRequested;
            RightSideInputViewModel.Dispose();
            RightSideInputViewModel = null;
        }

        private void DisposeRightSideDynamicViewModel()
        {
            if (RightSideSelectionViewModel == null)
                return;
            RightSideSelectionViewModel.PropertySelected -= RightSideOnPropertySelected;
            RightSideSelectionViewModel.SwitchToStaticRequested -= RightSideSelectionViewModelOnSwitchToStaticRequested;
            RightSideSelectionViewModel.Dispose();
            RightSideSelectionViewModel = null;
        }

        #endregion
    }
}
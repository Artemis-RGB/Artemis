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
        private BindableCollection<ConditionOperator> _operators;
        private DataModelStaticViewModel _rightSideInputViewModel;
        private DataModelDynamicViewModel _rightSideSelectionViewModel;
        private ConditionOperator _selectedOperator;

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
            Operators = new BindableCollection<ConditionOperator>();

            Initialize();
        }

        public DataModelConditionListPredicate DataModelConditionListPredicate => (DataModelConditionListPredicate) Model;

        public BindableCollection<ConditionOperator> Operators
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

        public ConditionOperator SelectedOperator
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
            DataModelVisualizationViewModel listDataModel = GetListDataModel();
            if (listDataModel.Children.Count == 1 && listDataModel.Children.First() is DataModelListPropertyViewModel)
                _isPrimitiveList = true;
            else
                _isPrimitiveList = false;

            // Get the data models
            if (!_isPrimitiveList)
            {
                LeftSideSelectionViewModel = _dataModelUIService.GetDynamicSelectionViewModel(_profileEditorService.GetCurrentModule());
                LeftSideSelectionViewModel.ChangeDataModel((DataModelPropertiesViewModel) listDataModel);
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
                LeftSideSelectionViewModel.PopulateSelectedPropertyViewModel(
                    DataModelConditionListPredicate.LeftPath?.Target,
                    DataModelConditionListPredicate.LeftPath?.Path
                );
            }

            Type leftSideType = _isPrimitiveList
                ? DataModelConditionListPredicate.DataModelConditionList.ListType
                : LeftSideSelectionViewModel.SelectedPropertyViewModel?.DataModelPath?.GetPropertyType();

            // Get the supported operators
            Operators.Clear();
            Operators.AddRange(_conditionOperatorService.GetConditionOperatorsForType(leftSideType ?? typeof(object)));
            if (DataModelConditionListPredicate.Operator == null)
                DataModelConditionListPredicate.UpdateOperator(Operators.FirstOrDefault(o => o.SupportsType(leftSideType ?? typeof(object))));
            SelectedOperator = DataModelConditionListPredicate.Operator;
            if (SelectedOperator == null || !SelectedOperator.SupportsRightSide)
            {
                DisposeRightSideStatic();
                DisposeRightSideDynamic();
            }

            // Ensure the right side has the proper VM
            ListRightSideType type = DataModelConditionListPredicate.PredicateType;
            if ((type == ListRightSideType.Dynamic || type == ListRightSideType.DynamicList) && SelectedOperator.SupportsRightSide)
            {
                DisposeRightSideStatic();
                if (RightSideSelectionViewModel == null)
                {
                    RightSideSelectionViewModel = _dataModelUIService.GetDynamicSelectionViewModel(_profileEditorService.GetCurrentModule());
                    RightSideSelectionViewModel.ButtonBrush = (Brush) Application.Current.FindResource("PrimaryHueMidBrush");
                    RightSideSelectionViewModel.PropertySelected += RightSideOnPropertySelected;

                    if (DataModelConditionListPredicate.PredicateType == ListRightSideType.DynamicList)
                        RightSideSelectionViewModel.ChangeDataModel((DataModelPropertiesViewModel) GetListDataModel());
                }

                RightSideSelectionViewModel.FilterTypes = new[] {leftSideType};
                RightSideSelectionViewModel.PopulateSelectedPropertyViewModel(
                    DataModelConditionListPredicate.RightPath?.Target,
                    DataModelConditionListPredicate.RightPath?.Path
                );
            }
            else if (SelectedOperator.SupportsRightSide)
            {
                DisposeRightSideDynamic();
                if (RightSideInputViewModel == null)
                {
                    RightSideInputViewModel = _dataModelUIService.GetStaticInputViewModel(leftSideType);
                    RightSideInputViewModel.Value = DataModelConditionListPredicate.RightStaticValue;
                    RightSideInputViewModel.ButtonBrush = (Brush) Application.Current.FindResource("PrimaryHueMidBrush");
                    RightSideInputViewModel.ValueUpdated += RightSideOnValueEntered;
                }

                if (RightSideInputViewModel.TargetType != leftSideType)
                    RightSideInputViewModel.UpdateTargetType(leftSideType);
            }
        }

        public void ApplyLeftSide()
        {
            DataModelConditionListPredicate.UpdateLeftSide(LeftSideSelectionViewModel.SelectedPropertyViewModel.Path);
            _profileEditorService.UpdateSelectedProfileElement();

            SelectedOperator = DataModelConditionListPredicate.Operator;
            Update();
        }

        public void ApplyRightSideDynamic()
        {
            if (DataModelConditionListPredicate.PredicateType == ListRightSideType.Dynamic)
                DataModelConditionListPredicate.UpdateRightSideDynamic(
                    RightSideSelectionViewModel.SelectedPropertyViewModel.DataModel,
                    RightSideSelectionViewModel.SelectedPropertyViewModel.Path
                );
            else if (DataModelConditionListPredicate.PredicateType == ListRightSideType.DynamicList)
                DataModelConditionListPredicate.UpdateRightSideDynamic(RightSideSelectionViewModel.SelectedPropertyViewModel.Path);

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

        private DataModelVisualizationViewModel GetListDataModel()
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
            if (!(context is ConditionOperator dataModelConditionOperator))
                return;

            SelectedOperator = dataModelConditionOperator;
            ApplyOperator();
        }

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

        private void DisposeRightSideStatic()
        {
            if (RightSideInputViewModel != null)
            {
                RightSideInputViewModel.ValueUpdated -= RightSideOnValueEntered;
                RightSideInputViewModel.Dispose();
                RightSideInputViewModel = null;
            }
        }

        private void DisposeRightSideDynamic()
        {
            if (RightSideSelectionViewModel != null)
            {
                RightSideSelectionViewModel.PropertySelected -= RightSideOnPropertySelected;
                RightSideSelectionViewModel.Dispose();
                RightSideSelectionViewModel = null;
            }
        }

        public void Dispose()
        {
            if (!_isPrimitiveList)
            {
                LeftSideSelectionViewModel.PropertySelected -= LeftSideOnPropertySelected;
                LeftSideSelectionViewModel.Dispose();
            }

            DisposeRightSideDynamic();
            DisposeRightSideStatic();
        }
    }
}
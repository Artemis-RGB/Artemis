using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Screens.ProfileEditor.Conditions.Abstract;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Input;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.Conditions
{
    public class DataModelConditionPredicateViewModel : DataModelConditionViewModel, IDisposable
    {
        private readonly IConditionOperatorService _conditionOperatorService;
        private readonly IDataModelUIService _dataModelUIService;
        private readonly IProfileEditorService _profileEditorService;
        private DataModelDynamicViewModel _leftSideSelectionViewModel;
        private BindableCollection<ConditionOperator> _operators;
        private DataModelStaticViewModel _rightSideInputViewModel;
        private DataModelDynamicViewModel _rightSideSelectionViewModel;
        private ConditionOperator _selectedOperator;

        private List<Type> _supportedInputTypes;

        public DataModelConditionPredicateViewModel(
            DataModelConditionPredicate dataModelConditionPredicate,
            IProfileEditorService profileEditorService,
            IDataModelUIService dataModelUIService,
            IConditionOperatorService conditionOperatorService,
            ISettingsService settingsService) : base(dataModelConditionPredicate)
        {
            _profileEditorService = profileEditorService;
            _dataModelUIService = dataModelUIService;
            _conditionOperatorService = conditionOperatorService;
            _supportedInputTypes = new List<Type>();

            SelectOperatorCommand = new DelegateCommand(ExecuteSelectOperatorCommand);
            Operators = new BindableCollection<ConditionOperator>();

            ShowDataModelValues = settingsService.GetSetting<bool>("ProfileEditor.ShowDataModelValues");

            Initialize();
        }

        public DataModelConditionPredicate DataModelConditionPredicate => (DataModelConditionPredicate) Model;
        public PluginSetting<bool> ShowDataModelValues { get; }


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

        public ConditionOperator SelectedOperator
        {
            get => _selectedOperator;
            set => SetAndNotify(ref _selectedOperator, value);
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

        public DelegateCommand SelectOperatorCommand { get; }

        public void Dispose()
        {
            if (LeftSideSelectionViewModel != null)
            {
                LeftSideSelectionViewModel.PropertySelected -= LeftSideOnPropertySelected;
                LeftSideSelectionViewModel.Dispose();
                LeftSideSelectionViewModel = null;
            }

            DisposeRightSideStatic();
            DisposeRightSideDynamic();
        }

        public override void Delete()
        {
            base.Delete();
            _profileEditorService.UpdateSelectedProfileElement();
        }

        public void Initialize()
        {
            LeftSideSelectionViewModel = _dataModelUIService.GetDynamicSelectionViewModel(_profileEditorService.GetCurrentModule());
            LeftSideSelectionViewModel.PropertySelected += LeftSideOnPropertySelected;
            // Determine which types are currently supported
            var editors = _dataModelUIService.RegisteredDataModelEditors;
            _supportedInputTypes = editors.Select(e => e.SupportedType).ToList();
            _supportedInputTypes.AddRange(editors.Where(e => e.CompatibleConversionTypes != null).SelectMany(e => e.CompatibleConversionTypes));

            Update();
        }

        public override void Update()
        {
            LeftSideSelectionViewModel.FilterTypes = _supportedInputTypes.ToArray();
            LeftSideSelectionViewModel.PopulateSelectedPropertyViewModel(
                DataModelConditionPredicate.LeftDataModel,
                DataModelConditionPredicate.LeftPropertyPath
            );
            var leftSideType = LeftSideSelectionViewModel.SelectedPropertyViewModel?.PropertyInfo?.PropertyType;

            // Get the supported operators
            Operators.Clear();
            Operators.AddRange(_conditionOperatorService.GetConditionOperatorsForType(leftSideType));
            if (DataModelConditionPredicate.Operator == null)
                DataModelConditionPredicate.UpdateOperator(Operators.FirstOrDefault(o => o.SupportsType(leftSideType)));
            SelectedOperator = DataModelConditionPredicate.Operator;
            if (!SelectedOperator.SupportsRightSide)
            {
                DisposeRightSideStatic();
                DisposeRightSideDynamic();
            }

            // Ensure the right side has the proper VM
            var targetType = LeftSideSelectionViewModel?.SelectedPropertyViewModel?.PropertyInfo?.PropertyType;
            if (DataModelConditionPredicate.PredicateType == ProfileRightSideType.Dynamic && SelectedOperator.SupportsRightSide)
            {
                DisposeRightSideStatic();
                if (RightSideSelectionViewModel == null)
                {
                    RightSideSelectionViewModel = _dataModelUIService.GetDynamicSelectionViewModel(_profileEditorService.GetCurrentModule());
                    RightSideSelectionViewModel.ButtonBrush = (Brush) Application.Current.FindResource("PrimaryHueMidBrush");
                    RightSideSelectionViewModel.PropertySelected += RightSideOnPropertySelected;
                }

                RightSideSelectionViewModel.PopulateSelectedPropertyViewModel(
                    DataModelConditionPredicate.RightDataModel,
                    DataModelConditionPredicate.RightPropertyPath
                );
                RightSideSelectionViewModel.FilterTypes = new[] {targetType};
            }
            else if (SelectedOperator.SupportsRightSide)
            {
                DisposeRightSideDynamic();
                if (RightSideInputViewModel == null)
                {
                    RightSideInputViewModel = _dataModelUIService.GetStaticInputViewModel(targetType);
                    RightSideInputViewModel.ButtonBrush = (Brush) Application.Current.FindResource("PrimaryHueMidBrush");
                    RightSideInputViewModel.ValueUpdated += RightSideOnValueEntered;
                }

                RightSideInputViewModel.Value = DataModelConditionPredicate.RightStaticValue;
                if (RightSideInputViewModel.TargetType != targetType)
                    RightSideInputViewModel.UpdateTargetType(targetType);
            }
        }

        public void ApplyLeftSide()
        {
            DataModelConditionPredicate.UpdateLeftSide(
                LeftSideSelectionViewModel.SelectedPropertyViewModel.DataModel,
                LeftSideSelectionViewModel.SelectedPropertyViewModel.PropertyPath
            );
            _profileEditorService.UpdateSelectedProfileElement();

            SelectedOperator = DataModelConditionPredicate.Operator;
            Update();
        }

        public void ApplyRightSideDynamic()
        {
            DataModelConditionPredicate.UpdateRightSide(
                RightSideSelectionViewModel.SelectedPropertyViewModel.DataModel,
                RightSideSelectionViewModel.SelectedPropertyViewModel.PropertyPath
            );
            _profileEditorService.UpdateSelectedProfileElement();

            Update();
        }

        public void ApplyRightSideStatic(object value)
        {
            DataModelConditionPredicate.UpdateRightSide(value);
            _profileEditorService.UpdateSelectedProfileElement();

            Update();
        }

        public void ApplyOperator()
        {
            DataModelConditionPredicate.UpdateOperator(SelectedOperator);
            _profileEditorService.UpdateSelectedProfileElement();

            Update();
        }

        private void LeftSideOnPropertySelected(object sender, DataModelInputDynamicEventArgs e)
        {
            ApplyLeftSide();
        }

        private void RightSideOnPropertySelected(object? sender, DataModelInputDynamicEventArgs e)
        {
            ApplyRightSideDynamic();
        }

        private void RightSideOnValueEntered(object sender, DataModelInputStaticEventArgs e)
        {
            ApplyRightSideStatic(e.Value);
        }

        private void ExecuteSelectOperatorCommand(object context)
        {
            if (!(context is ConditionOperator DataModelConditionOperator))
                return;

            SelectedOperator = DataModelConditionOperator;
            ApplyOperator();
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
    }
}
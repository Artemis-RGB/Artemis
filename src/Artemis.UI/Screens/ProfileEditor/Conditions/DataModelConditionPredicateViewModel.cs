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
        private BindableCollection<BaseConditionOperator> _operators;
        private DataModelStaticViewModel _rightSideInputViewModel;
        private DataModelDynamicViewModel _rightSideSelectionViewModel;
        private BaseConditionOperator _selectedOperator;

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
            Operators = new BindableCollection<BaseConditionOperator>();

            ShowDataModelValues = settingsService.GetSetting<bool>("ProfileEditor.ShowDataModelValues");

            Initialize();
        }

        public DataModelConditionPredicate DataModelConditionPredicate => (DataModelConditionPredicate) Model;
        public PluginSetting<bool> ShowDataModelValues { get; }


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

        public BaseConditionOperator SelectedOperator
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
            IReadOnlyCollection<DataModelVisualizationRegistration> editors = _dataModelUIService.RegisteredDataModelEditors;
            _supportedInputTypes = editors.Select(e => e.SupportedType).ToList();
            _supportedInputTypes.AddRange(editors.Where(e => e.CompatibleConversionTypes != null).SelectMany(e => e.CompatibleConversionTypes));
            _supportedInputTypes.Add(typeof(IEnumerable<>));
            Update();
        }

        public override void Update()
        {
            LeftSideSelectionViewModel.FilterTypes = _supportedInputTypes.ToArray();
            LeftSideSelectionViewModel.ChangeDataModelPath(DataModelConditionPredicate.LeftPath);

            Type leftSideType = LeftSideSelectionViewModel.DataModelPath?.GetPropertyType();

            // Get the supported operators
            Operators.Clear();
            Operators.AddRange(_conditionOperatorService.GetConditionOperatorsForType(leftSideType ?? typeof(object), ConditionParameterSide.Left));
            if (DataModelConditionPredicate.Operator == null)
                DataModelConditionPredicate.UpdateOperator(Operators.FirstOrDefault(o => o.SupportsType(leftSideType ?? typeof(object), ConditionParameterSide.Left)));
            SelectedOperator = DataModelConditionPredicate.Operator;

            // Without a selected operator or one that supports a right side, leave the right side input empty
            if (SelectedOperator == null || SelectedOperator.RightSideType == null)
            {
                DisposeRightSideStaticViewModel();
                DisposeRightSideDynamicViewModel();
                return;
            }

            // Ensure the right side has the proper VM
            if (DataModelConditionPredicate.PredicateType == ProfileRightSideType.Dynamic)
            {
                DisposeRightSideStaticViewModel();
                if (RightSideSelectionViewModel == null)
                    CreateRightSideSelectionViewModel();

                RightSideSelectionViewModel.ChangeDataModelPath(DataModelConditionPredicate.RightPath);
                RightSideSelectionViewModel.FilterTypes = new[] {SelectedOperator.RightSideType};
            }
            else
            {
                DisposeRightSideDynamicViewModel();
                if (RightSideInputViewModel == null)
                    CreateRightSideInputViewModel(SelectedOperator.RightSideType);

                if (SelectedOperator.RightSideType.IsValueType && DataModelConditionPredicate.RightStaticValue == null)
                    RightSideInputViewModel.Value = SelectedOperator.RightSideType.GetDefault();
                else
                    RightSideInputViewModel.Value = DataModelConditionPredicate.RightStaticValue;
                if (RightSideInputViewModel.TargetType != SelectedOperator.RightSideType)
                    RightSideInputViewModel.UpdateTargetType(SelectedOperator.RightSideType);
            }
        }

        public void ApplyLeftSide()
        {
            if (LeftSideSelectionViewModel.DataModelPath.GetPropertyType().IsGenericEnumerable())
            {
                if (Parent is DataModelConditionGroupViewModel groupViewModel)
                    groupViewModel.ConvertToConditionList(this);
                return;
            }

            DataModelConditionPredicate.UpdateLeftSide(LeftSideSelectionViewModel.DataModelPath);
            _profileEditorService.UpdateSelectedProfileElement();

            SelectedOperator = DataModelConditionPredicate.Operator;
            Update();
        }

        public void ApplyRightSideDynamic()
        {
            DataModelConditionPredicate.UpdateRightSideDynamic(RightSideSelectionViewModel.DataModelPath);
            _profileEditorService.UpdateSelectedProfileElement();

            Update();
        }

        public void ApplyRightSideStatic(object value)
        {
            DataModelConditionPredicate.UpdateRightSideStatic(value);
            _profileEditorService.UpdateSelectedProfileElement();

            Update();
        }

        public void ApplyOperator()
        {
            DataModelConditionPredicate.UpdateOperator(SelectedOperator);
            _profileEditorService.UpdateSelectedProfileElement();

            Update();
        }

        private void ExecuteSelectOperatorCommand(object context)
        {
            if (!(context is BaseConditionOperator DataModelConditionOperator))
                return;

            SelectedOperator = DataModelConditionOperator;
            ApplyOperator();
        }

        #region IDisposable

        public void Dispose()
        {
            if (LeftSideSelectionViewModel != null)
            {
                LeftSideSelectionViewModel.PropertySelected -= LeftSideOnPropertySelected;
                LeftSideSelectionViewModel.Dispose();
                LeftSideSelectionViewModel = null;
            }

            DisposeRightSideStaticViewModel();
            DisposeRightSideDynamicViewModel();
        }

        #endregion

        #region View model creation

        private void CreateRightSideSelectionViewModel()
        {
            RightSideSelectionViewModel = _dataModelUIService.GetDynamicSelectionViewModel(_profileEditorService.GetCurrentModule());
            RightSideSelectionViewModel.ButtonBrush = (SolidColorBrush) Application.Current.FindResource("PrimaryHueMidBrush");
            RightSideSelectionViewModel.DisplaySwitchButton = true;
            RightSideSelectionViewModel.PropertySelected += RightSideOnPropertySelected;
            RightSideSelectionViewModel.SwitchToStaticRequested += RightSideSelectionViewModelOnSwitchToStaticRequested;
        }

        private void CreateRightSideInputViewModel(Type leftSideType)
        {
            RightSideInputViewModel = _dataModelUIService.GetStaticInputViewModel(leftSideType, LeftSideSelectionViewModel.DataModelPath?.GetPropertyDescription());
            RightSideInputViewModel.ButtonBrush = (SolidColorBrush) Application.Current.FindResource("PrimaryHueMidBrush");
            RightSideInputViewModel.DisplaySwitchButton = true;
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
            DataModelConditionPredicate.PredicateType = ProfileRightSideType.Static;
            Update();
        }


        private void RightSideInputViewModelOnSwitchToDynamicRequested(object? sender, EventArgs e)
        {
            DataModelConditionPredicate.PredicateType = ProfileRightSideType.Dynamic;
            Update();
        }

        #endregion
    }
}
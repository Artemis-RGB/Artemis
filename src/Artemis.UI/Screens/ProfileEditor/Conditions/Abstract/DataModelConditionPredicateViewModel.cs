using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Input;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.Conditions.Abstract
{
    public abstract class DataModelConditionPredicateViewModel : DataModelConditionViewModel, IDisposable
    {
        private readonly IConditionOperatorService _conditionOperatorService;
        private readonly IDataModelUIService _dataModelUIService;
        private readonly IProfileEditorService _profileEditorService;
        private DataModelStaticViewModel _rightSideInputViewModel;
        private DataModelDynamicViewModel _rightSideSelectionViewModel;
        private BaseConditionOperator _selectedOperator;

        private List<Type> _supportedInputTypes;

        protected DataModelConditionPredicateViewModel(
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
        }

        public DataModelConditionPredicate DataModelConditionPredicate => (DataModelConditionPredicate) Model;
        public PluginSetting<bool> ShowDataModelValues { get; }

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
        public BindableCollection<BaseConditionOperator> Operators { get; }

        protected SolidColorBrush LeftSideColor { get; set; }

        public override void Evaluate()
        {
            IsConditionMet = DataModelConditionPredicate.Evaluate();
        }

        public override void Delete()
        {
            base.Delete();
            _profileEditorService.UpdateSelectedProfileElement();
        }

        public virtual void Initialize()
        {
            LeftSideSelectionViewModel = _dataModelUIService.GetDynamicSelectionViewModel(_profileEditorService.GetCurrentModule());
            LeftSideSelectionViewModel.PropertySelected += LeftSideOnPropertySelected;
            if (LeftSideColor != null)
                LeftSideSelectionViewModel.ButtonBrush = LeftSideColor;

            // Determine which types are currently supported
            _supportedInputTypes = GetSupportedInputTypes();

            Update();
        }

        public override void Update()
        {
            LeftSideSelectionViewModel.FilterTypes = _supportedInputTypes.ToArray();
            LeftSideSelectionViewModel.ChangeDataModelPath(DataModelConditionPredicate.LeftPath);

            Type leftSideType = GetLeftSideType();

            // Get the supported operators
            Operators.Clear();
            Operators.AddRange(_conditionOperatorService.GetConditionOperatorsForType(leftSideType ?? typeof(object), ConditionParameterSide.Left));
            if (DataModelConditionPredicate.Operator == null)
                DataModelConditionPredicate.UpdateOperator(Operators.FirstOrDefault());
            // The core doesn't care about best matches so if there is a new preferred operator, use that instead
            else if (!Operators.Contains(DataModelConditionPredicate.Operator))
                DataModelConditionPredicate.UpdateOperator(Operators.FirstOrDefault(o => o.Description == DataModelConditionPredicate.Operator.Description) ?? Operators.FirstOrDefault());

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
                    CreateRightSideInputViewModel();
                
                Type preferredType = DataModelConditionPredicate.GetPreferredRightSideType();
                if (preferredType != null && RightSideInputViewModel.TargetType != preferredType)
                    RightSideInputViewModel.UpdateTargetType(preferredType);

                RightSideInputViewModel.Value = DataModelConditionPredicate.RightStaticValue;
            }
        }

        public void ApplyLeftSide()
        {
            Type newType = LeftSideSelectionViewModel.DataModelPath.GetPropertyType();
            bool converted = ConvertIfRequired(newType);
            if (converted)
                return;

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

        protected abstract List<Type> GetSupportedInputTypes();
        protected abstract Type GetLeftSideType();

        protected virtual List<DataModelPropertiesViewModel> GetExtraRightSideDataModelViewModels()
        {
            return null;
        }

        private void ExecuteSelectOperatorCommand(object context)
        {
            if (!(context is BaseConditionOperator dataModelConditionOperator))
                return;

            SelectedOperator = dataModelConditionOperator;
            ApplyOperator();
        }

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
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
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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

            List<DataModelPropertiesViewModel> extra = GetExtraRightSideDataModelViewModels();
            if (extra != null)
                RightSideSelectionViewModel.ExtraDataModelViewModels.AddRange(extra);
        }

        private void CreateRightSideInputViewModel()
        {
            Type preferredType = DataModelConditionPredicate.GetPreferredRightSideType();
            RightSideInputViewModel = _dataModelUIService.GetStaticInputViewModel(preferredType, LeftSideSelectionViewModel.DataModelPath?.GetPropertyDescription());
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

            // Ensure the right static value is never null when the preferred type is a value type
            Type preferredType = DataModelConditionPredicate.GetPreferredRightSideType();
            if (DataModelConditionPredicate.RightStaticValue == null && preferredType != null && preferredType.IsValueType)
                DataModelConditionPredicate.UpdateRightSideStatic(preferredType.GetDefault());

            Update();
        }

        private void RightSideInputViewModelOnSwitchToDynamicRequested(object sender, EventArgs e)
        {
            DataModelConditionPredicate.PredicateType = ProfileRightSideType.Dynamic;
            Update();
        }

        #endregion
    }
}
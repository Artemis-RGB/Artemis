using Artemis.Core.Models.Profile.Conditions;
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
        private readonly IDataModelVisualizationService _dataModelVisualizationService;
        private readonly IProfileEditorService _profileEditorService;
        private DataModelPropertiesViewModel _dataModel;
        private DataModelVisualizationViewModel _selectedLeftSideProperty;
        private DataModelVisualizationViewModel _selectedRightSideProperty;
        private int _rightSideTransitionIndex;
        private DataModelInputViewModel _rightSideInputViewModel;

        public DisplayConditionPredicateViewModel(DisplayConditionPredicate displayConditionPredicate, DisplayConditionViewModel parent,
            IProfileEditorService profileEditorService, IDataModelVisualizationService dataModelVisualizationService) : base(displayConditionPredicate, parent)
        {
            _profileEditorService = profileEditorService;
            _dataModelVisualizationService = dataModelVisualizationService;

            SelectLeftPropertyCommand = new DelegateCommand(ExecuteSelectLeftProperty);
            SelectRightPropertyCommand = new DelegateCommand(ExecuteSelectRightProperty);

            GetDataModel();
        }

        public DisplayConditionPredicate DisplayConditionPredicate => (DisplayConditionPredicate) Model;
        public DelegateCommand SelectLeftPropertyCommand { get; }
        public DelegateCommand SelectRightPropertyCommand { get; }
        public bool ShowRightSidePropertySelection => DisplayConditionPredicate.PredicateType == PredicateType.Dynamic;

        public DataModelPropertiesViewModel DataModel
        {
            get => _dataModel;
            set => SetAndNotify(ref _dataModel, value);
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

        public void GetDataModel()
        {
            var mainDataModel = _dataModelVisualizationService.GetMainDataModelVisualization(true);
            if (!_dataModelVisualizationService.GetPluginExtendsDataModel(_profileEditorService.GetCurrentModule()))
                mainDataModel.Children.Add(_dataModelVisualizationService.GetPluginDataModelVisualization(_profileEditorService.GetCurrentModule(), true));

            DataModel = mainDataModel;

            Update();
        }

        public override void Update()
        {
            SelectedLeftSideProperty = DisplayConditionPredicate.LeftPropertyPath != null 
                ? DataModel.GetChildByPath(DisplayConditionPredicate.LeftDataModelGuid, DisplayConditionPredicate.LeftPropertyPath) 
                : null;

            SelectedRightSideProperty = DisplayConditionPredicate.RightPropertyPath != null 
                ? DataModel.GetChildByPath(DisplayConditionPredicate.RightDataModelGuid, DisplayConditionPredicate.RightPropertyPath) 
                : null;
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
    }
}
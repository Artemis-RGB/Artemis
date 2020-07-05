using Artemis.Core.Models.Profile.Conditions;
using Artemis.UI.Screens.Module.ProfileEditor.DisplayConditions.Abstract;
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
        private bool _leftSidePropertySelectionOpen;

        public DisplayConditionPredicateViewModel(DisplayConditionPredicate displayConditionPredicate, DisplayConditionViewModel parent,
            IProfileEditorService profileEditorService, IDataModelVisualizationService dataModelVisualizationService) : base(displayConditionPredicate, parent)
        {
            _profileEditorService = profileEditorService;
            _dataModelVisualizationService = dataModelVisualizationService;

            SelectPropertyCommand = new DelegateCommand(ExecuteSelectProperty);

            GetDataModel();
        }

        public DelegateCommand SelectPropertyCommand { get; }

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

        public bool LeftSidePropertySelectionOpen
        {
            get => _leftSidePropertySelectionOpen;
            set => SetAndNotify(ref _leftSidePropertySelectionOpen, value);
        }


        public DisplayConditionPredicate DisplayConditionPredicate => (DisplayConditionPredicate) Model;

        public void ToggleLeftSidePropertySelectionOpen()
        {
            LeftSidePropertySelectionOpen = !LeftSidePropertySelectionOpen;
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
            if (DisplayConditionPredicate.PropertyPath != null)
                SelectedLeftSideProperty = DataModel.GetChildByPath(DisplayConditionPredicate.DataModelGuid, DisplayConditionPredicate.PropertyPath);
            else
                SelectedLeftSideProperty = null;
        }

        private void ExecuteSelectProperty(object context)
        {
            if (!(context is DataModelVisualizationViewModel vm))
                return;

            DisplayConditionPredicate.PropertyPath = vm.GetCurrentPath();
            DisplayConditionPredicate.DataModelGuid = vm.DataModel.PluginInfo.Guid;
            Update();
        }
    }
}
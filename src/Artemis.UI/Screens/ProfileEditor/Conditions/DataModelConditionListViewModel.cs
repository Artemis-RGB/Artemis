using System;
using System.Collections;
using System.Linq;
using System.Windows.Media;
using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.ProfileEditor.Conditions.Abstract;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Input;
using Artemis.UI.Shared.Services;
using Humanizer;

namespace Artemis.UI.Screens.ProfileEditor.Conditions
{
    public class DataModelConditionListViewModel : DataModelConditionViewModel, IDisposable
    {
        private readonly IDataModelConditionsVmFactory _dataModelConditionsVmFactory;
        private readonly IDataModelUIService _dataModelUIService;
        private readonly IProfileEditorService _profileEditorService;
        private DataModelDynamicViewModel _targetSelectionViewModel;

        public DataModelConditionListViewModel(
            DataModelConditionList dataModelConditionList,
            IProfileEditorService profileEditorService,
            IDataModelUIService dataModelUIService,
            IDataModelConditionsVmFactory dataModelConditionsVmFactory) : base(dataModelConditionList)
        {
            _profileEditorService = profileEditorService;
            _dataModelUIService = dataModelUIService;
            _dataModelConditionsVmFactory = dataModelConditionsVmFactory;

            Initialize();
        }

        public DataModelDynamicViewModel TargetSelectionViewModel
        {
            get => _targetSelectionViewModel;
            set => SetAndNotify(ref _targetSelectionViewModel, value);
        }

        public DataModelConditionList DataModelConditionList => (DataModelConditionList) Model;

        public string SelectedListOperator => DataModelConditionList.ListOperator.Humanize();

        public void Dispose()
        {
            TargetSelectionViewModel.Dispose();
            TargetSelectionViewModel.PropertySelected -= TargetSelectionViewModelOnPropertySelected;
        }

        public void SelectListOperator(string type)
        {
            var enumValue = Enum.Parse<ListOperator>(type);
            DataModelConditionList.ListOperator = enumValue;
            NotifyOfPropertyChange(nameof(SelectedListOperator));

            _profileEditorService.UpdateSelectedProfileElement();
        }

        public void AddCondition(string type)
        {
            if (type == "Static")
                DataModelConditionList.AddChild(new DataModelConditionPredicate(DataModelConditionList, ProfileRightSideType.Static));
            else if (type == "Dynamic")
                DataModelConditionList.AddChild(new DataModelConditionPredicate(DataModelConditionList, ProfileRightSideType.Dynamic));

            Update();
            _profileEditorService.UpdateSelectedProfileElement();
        }

        public void AddGroup()
        {
            DataModelConditionList.AddChild(new DataModelConditionGroup(DataModelConditionList));

            Update();
            _profileEditorService.UpdateSelectedProfileElement();
        }

        public override void Delete()
        {
            base.Delete();
            _profileEditorService.UpdateSelectedProfileElement();
        }

        public void Initialize()
        {
            TargetSelectionViewModel = _dataModelUIService.GetDynamicSelectionViewModel(_profileEditorService.GetCurrentModule());
            TargetSelectionViewModel.FilterTypes = new[] {typeof(IList)};
            TargetSelectionViewModel.ButtonBrush = new SolidColorBrush(Color.FromRgb(71, 108, 188));
            TargetSelectionViewModel.Placeholder = "Select a list";
            TargetSelectionViewModel.PropertySelected += TargetSelectionViewModelOnPropertySelected;

            Update();
        }

        public void ApplyList()
        {
            DataModelConditionList.UpdateList(
                TargetSelectionViewModel.SelectedPropertyViewModel.DataModel,
                TargetSelectionViewModel.SelectedPropertyViewModel.Path
            );
            _profileEditorService.UpdateSelectedProfileElement();

            Update();
        }

        public override void Update()
        {
            TargetSelectionViewModel.PopulateSelectedPropertyViewModel(DataModelConditionList.ListDataModel, DataModelConditionList.ListPropertyPath);
            NotifyOfPropertyChange(nameof(SelectedListOperator));

            // Remove VMs of effects no longer applied on the layer
            var toRemove = Items.Where(c => !DataModelConditionList.Children.Contains(c.Model)).ToList();
            // Using RemoveRange breaks our lovely animations
            foreach (var conditionViewModel in toRemove)
                Items.Remove(conditionViewModel);

            foreach (var childModel in Model.Children)
            {
                if (Items.Any(c => c.Model == childModel))
                    continue;
                if (!(childModel is DataModelConditionGroup dataModelConditionGroup))
                    continue;

                var viewModel = _dataModelConditionsVmFactory.DataModelConditionGroupViewModel(dataModelConditionGroup, true);
                viewModel.IsRootGroup = true;
                Items.Add(viewModel);
            }

            foreach (var childViewModel in Items)
                childViewModel.Update();
        }

        private void TargetSelectionViewModelOnPropertySelected(object? sender, DataModelInputDynamicEventArgs e)
        {
            ApplyList();
        }
    }
}
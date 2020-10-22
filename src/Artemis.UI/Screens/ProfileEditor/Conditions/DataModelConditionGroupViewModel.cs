using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.UI.Extensions;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.ProfileEditor.Conditions.Abstract;
using Artemis.UI.Screens.ProfileEditor.DisplayConditions;
using Artemis.UI.Shared.Services;
using Humanizer;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.Conditions
{
    public class DataModelConditionGroupViewModel : DataModelConditionViewModel
    {
        private readonly IDataModelConditionsVmFactory _dataModelConditionsVmFactory;
        private readonly IProfileEditorService _profileEditorService;
        private bool _isInitialized;
        private bool _isRootGroup;

        public DataModelConditionGroupViewModel(DataModelConditionGroup dataModelConditionGroup,
            bool isListGroup,
            IProfileEditorService profileEditorService,
            IDataModelConditionsVmFactory dataModelConditionsVmFactory)
            : base(dataModelConditionGroup)
        {
            IsListGroup = isListGroup;
            _profileEditorService = profileEditorService;
            _dataModelConditionsVmFactory = dataModelConditionsVmFactory;

            Items.CollectionChanged += (sender, args) => NotifyOfPropertyChange(nameof(DisplayBooleanOperator));

            Execute.PostToUIThread(async () =>
            {
                await Task.Delay(50);
                IsInitialized = true;
            });
        }

        public bool IsListGroup { get; }
        public DataModelConditionGroup DataModelConditionGroup => (DataModelConditionGroup) Model;

        public bool IsRootGroup
        {
            get => _isRootGroup;
            set => SetAndNotify(ref _isRootGroup, value);
        }

        public bool IsInitialized
        {
            get => _isInitialized;
            set => SetAndNotify(ref _isInitialized, value);
        }

        public bool DisplayBooleanOperator => Items.Count > 1;
        public string SelectedBooleanOperator => DataModelConditionGroup.BooleanOperator.Humanize();

        public void SelectBooleanOperator(string type)
        {
            BooleanOperator enumValue = Enum.Parse<BooleanOperator>(type);
            DataModelConditionGroup.BooleanOperator = enumValue;
            NotifyOfPropertyChange(nameof(SelectedBooleanOperator));

            _profileEditorService.UpdateSelectedProfileElement();
        }

        public void AddCondition()
        {
            if (!IsListGroup)
                DataModelConditionGroup.AddChild(new DataModelConditionPredicate(DataModelConditionGroup, ProfileRightSideType.Dynamic));
            else
                DataModelConditionGroup.AddChild(new DataModelConditionListPredicate(DataModelConditionGroup, ProfileRightSideType.Dynamic));

            Update();
            _profileEditorService.UpdateSelectedProfileElement();
        }

        public void AddGroup()
        {
            DataModelConditionGroup.AddChild(new DataModelConditionGroup(DataModelConditionGroup));

            Update();
            _profileEditorService.UpdateSelectedProfileElement();
        }

        public override void Update()
        {
            NotifyOfPropertyChange(nameof(SelectedBooleanOperator));

            // Remove VMs of effects no longer applied on the layer
            Items.RemoveRange(Items.Where(c => !DataModelConditionGroup.Children.Contains(c.Model)).ToList());

            List<DataModelConditionViewModel> viewModels = new List<DataModelConditionViewModel>();
            foreach (DataModelConditionPart childModel in Model.Children)
            {
                if (Items.Any(c => c.Model == childModel))
                    continue;

                switch (childModel)
                {
                    case DataModelConditionGroup dataModelConditionGroup:
                        viewModels.Add(_dataModelConditionsVmFactory.DataModelConditionGroupViewModel(dataModelConditionGroup, IsListGroup));
                        break;
                    case DataModelConditionList dataModelConditionList:
                        viewModels.Add(_dataModelConditionsVmFactory.DataModelConditionListViewModel(dataModelConditionList));
                        break;
                    case DataModelConditionEvent dataModelConditionEvent:
                        viewModels.Add(_dataModelConditionsVmFactory.DataModelConditionEventViewModel(dataModelConditionEvent));
                        break;
                    case DataModelConditionPredicate dataModelConditionPredicate:
                        if (!IsListGroup)
                            viewModels.Add(_dataModelConditionsVmFactory.DataModelConditionPredicateViewModel(dataModelConditionPredicate));
                        break;
                    case DataModelConditionListPredicate dataModelConditionListPredicate:
                        if (IsListGroup)
                            viewModels.Add(_dataModelConditionsVmFactory.DataModelConditionListPredicateViewModel(dataModelConditionListPredicate));
                        break;
                }
            }

            if (viewModels.Any())
                Items.AddRange(viewModels);

            // Ensure the items are in the same order as on the model
            ((BindableCollection<DataModelConditionViewModel>) Items).Sort(i => Model.Children.IndexOf(i.Model));

            foreach (DataModelConditionViewModel childViewModel in Items)
                childViewModel.Update();

            if (IsRootGroup && Parent is DisplayConditionsViewModel displayConditionsViewModel)
                displayConditionsViewModel.DisplayStartHint = !Items.Any();

            OnUpdated();
        }

        public void ConvertToConditionList(DataModelConditionViewModel predicateViewModel)
        {
            // Store the old index and remove the old predicate
            int index = DataModelConditionGroup.Children.IndexOf(predicateViewModel.Model);
            DataModelConditionGroup.RemoveChild(predicateViewModel.Model);

            // Insert a list in the same position
            DataModelConditionList list = new DataModelConditionList(DataModelConditionGroup);
            list.UpdateList(predicateViewModel.LeftSideSelectionViewModel.DataModelPath);
            DataModelConditionGroup.AddChild(list, index);

            // Update to switch the VMs
            Update();
        }

        public void ConvertToConditionEvent(DataModelConditionViewModel predicateViewModel)
        {
            // Remove the old child
            DataModelConditionGroup.RemoveChild(predicateViewModel.Model);

            DataModelConditionPart rootGroup = DataModelConditionGroup;
            while (rootGroup.Parent != null)
                rootGroup = rootGroup.Parent;

            // Insert an event at the start of the root group
            DataModelConditionEvent conditionEvent = new DataModelConditionEvent(rootGroup);
            conditionEvent.UpdateEvent(predicateViewModel.LeftSideSelectionViewModel.DataModelPath);
            rootGroup.AddChild(conditionEvent, 0);

            // Update to switch the VMs
            Update();
        }

        public void ConvertToPredicate(DataModelConditionViewModel listViewModel)
        {
            // Store the old index and remove the old predicate
            int index = DataModelConditionGroup.Children.IndexOf(listViewModel.Model);
            DataModelConditionGroup.RemoveChild(listViewModel.Model);

            // Insert a list in the same position
            DataModelConditionPredicate predicate = new DataModelConditionPredicate(DataModelConditionGroup, ProfileRightSideType.Dynamic);
            predicate.UpdateLeftSide(listViewModel.LeftSideSelectionViewModel.DataModelPath);
            DataModelConditionGroup.AddChild(predicate, index);

            // Update to switch the VMs
            Update();
        }

        public event EventHandler Updated;

        protected virtual void OnUpdated()
        {
            Updated?.Invoke(this, EventArgs.Empty);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.UI.Extensions;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.ProfileEditor.Conditions.Abstract;
using Artemis.UI.Shared.Services;
using Humanizer;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.Conditions
{
    public class DataModelConditionGroupViewModel : DataModelConditionViewModel
    {
        private readonly IDataModelConditionsVmFactory _dataModelConditionsVmFactory;
        private readonly List<Module> _modules;
        private readonly IProfileEditorService _profileEditorService;
        private bool _isEventGroup;
        private bool _isRootGroup;

        public DataModelConditionGroupViewModel(DataModelConditionGroup dataModelConditionGroup,
            ConditionGroupType groupType,
            List<Module> modules,
            IProfileEditorService profileEditorService,
            IDataModelConditionsVmFactory dataModelConditionsVmFactory)
            : base(dataModelConditionGroup)
        {
            GroupType = groupType;
            _modules = modules;
            _profileEditorService = profileEditorService;
            _dataModelConditionsVmFactory = dataModelConditionsVmFactory;

            Items.CollectionChanged += (_, _) => NotifyOfPropertyChange(nameof(DisplayBooleanOperator));
        }

        public ConditionGroupType GroupType { get; }
        public DataModelConditionGroup DataModelConditionGroup => (DataModelConditionGroup) Model;

        public bool IsRootGroup
        {
            get => _isRootGroup;
            set
            {
                if (!SetAndNotify(ref _isRootGroup, value)) return;
                NotifyOfPropertyChange(nameof(CanAddEventCondition));
            }
        }

        public bool CanAddEventCondition => IsRootGroup && GroupType == ConditionGroupType.General;

        public bool IsEventGroup
        {
            get => _isEventGroup;
            set
            {
                SetAndNotify(ref _isEventGroup, value);
                NotifyOfPropertyChange(nameof(DisplayEvaluationResult));
            }
        }

        public bool DisplayBooleanOperator => Items.Count > 1;
        public bool DisplayEvaluationResult => GroupType == ConditionGroupType.General && !IsEventGroup;
        public string SelectedBooleanOperator => DataModelConditionGroup.BooleanOperator.Humanize();

        public void SelectBooleanOperator(string type)
        {
            BooleanOperator enumValue = Enum.Parse<BooleanOperator>(type);
            DataModelConditionGroup.BooleanOperator = enumValue;
            NotifyOfPropertyChange(nameof(SelectedBooleanOperator));

            _profileEditorService.SaveSelectedProfileElement();
        }

        public void AddCondition()
        {
            switch (GroupType)
            {
                case ConditionGroupType.General:
                    DataModelConditionGroup.AddChild(new DataModelConditionGeneralPredicate(DataModelConditionGroup, ProfileRightSideType.Dynamic));
                    break;
                case ConditionGroupType.List:
                    DataModelConditionGroup.AddChild(new DataModelConditionListPredicate(DataModelConditionGroup, ProfileRightSideType.Dynamic));
                    break;
                case ConditionGroupType.Event:
                    DataModelConditionGroup.AddChild(new DataModelConditionEventPredicate(DataModelConditionGroup, ProfileRightSideType.Dynamic));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Update();
            _profileEditorService.SaveSelectedProfileElement();
        }

        public void AddEventCondition()
        {
            if (!CanAddEventCondition)
                return;

            // Find a good spot for the event, behind the last existing event
            int index = 0;
            DataModelConditionPart existing = DataModelConditionGroup.Children.LastOrDefault(c => c is DataModelConditionEvent);
            if (existing != null)
                index = DataModelConditionGroup.Children.IndexOf(existing) + 1;

            DataModelConditionGroup.AddChild(new DataModelConditionEvent(DataModelConditionGroup), index);

            Update();
            _profileEditorService.SaveSelectedProfileElement();
        }

        public void AddGroup()
        {
            DataModelConditionGroup.AddChild(new DataModelConditionGroup(DataModelConditionGroup));

            Update();
            _profileEditorService.SaveSelectedProfileElement();
        }

        public override void Update()
        {
            NotifyOfPropertyChange(nameof(SelectedBooleanOperator));
            // Remove VMs of effects no longer applied on the layer
            List<DataModelConditionViewModel> toRemove = Items.Where(c => !DataModelConditionGroup.Children.Contains(c.Model)).ToList();
            if (toRemove.Any())
                Items.RemoveRange(toRemove);

            foreach (DataModelConditionPart childModel in Model.Children)
            {
                if (Items.Any(c => c.Model == childModel))
                    continue;

                switch (childModel)
                {
                    case DataModelConditionGroup dataModelConditionGroup:
                        Items.Add(_dataModelConditionsVmFactory.DataModelConditionGroupViewModel(dataModelConditionGroup, GroupType, _modules));
                        break;
                    case DataModelConditionList dataModelConditionList:
                        Items.Add(_dataModelConditionsVmFactory.DataModelConditionListViewModel(dataModelConditionList, _modules));
                        break;
                    case DataModelConditionEvent dataModelConditionEvent:
                        Items.Add(_dataModelConditionsVmFactory.DataModelConditionEventViewModel(dataModelConditionEvent, _modules));
                        break;
                    case DataModelConditionGeneralPredicate dataModelConditionGeneralPredicate:
                        Items.Add(_dataModelConditionsVmFactory.DataModelConditionGeneralPredicateViewModel(dataModelConditionGeneralPredicate, _modules));
                        break;
                    case DataModelConditionListPredicate dataModelConditionListPredicate:
                        Items.Add(_dataModelConditionsVmFactory.DataModelConditionListPredicateViewModel(dataModelConditionListPredicate, _modules));
                        break;
                    case DataModelConditionEventPredicate dataModelConditionEventPredicate:
                        Items.Add(_dataModelConditionsVmFactory.DataModelConditionEventPredicateViewModel(dataModelConditionEventPredicate, _modules));
                        break;
                }
            }

            // Ensure the items are in the same order as on the model
            ((BindableCollection<DataModelConditionViewModel>) Items).Sort(i => Model.Children.IndexOf(i.Model));
            foreach (DataModelConditionViewModel childViewModel in Items)
                childViewModel.Update();

            IsEventGroup = Items.Any(i => i is DataModelConditionEventViewModel);
            if (IsEventGroup)
            {
                if (DataModelConditionGroup.BooleanOperator != BooleanOperator.And)
                    SelectBooleanOperator("And");
            }

            OnUpdated();
        }

        public override void Evaluate()
        {
            IsConditionMet = DataModelConditionGroup.Evaluate();
            foreach (DataModelConditionViewModel dataModelConditionViewModel in Items)
                dataModelConditionViewModel.Evaluate();
        }

        public override void UpdateModules()
        {
            foreach (DataModelConditionViewModel dataModelConditionViewModel in Items) 
                dataModelConditionViewModel.UpdateModules();
        }

        public void ConvertToConditionList(DataModelConditionViewModel predicateViewModel)
        {
            // Store the old index and remove the old predicate
            int index = DataModelConditionGroup.Children.IndexOf(predicateViewModel.Model);
            DataModelConditionGroup.RemoveChild(predicateViewModel.Model);

            // Insert a list in the same position
            DataModelConditionList list = new(DataModelConditionGroup);
            list.UpdateList(predicateViewModel.LeftSideSelectionViewModel.DataModelPath);
            DataModelConditionGroup.AddChild(list, index);

            // Update to switch the VMs
            Update();
        }

        public void ConvertToPredicate(DataModelConditionViewModel listViewModel)
        {
            // Store the old index and remove the old predicate
            int index = DataModelConditionGroup.Children.IndexOf(listViewModel.Model);
            DataModelConditionGroup.RemoveChild(listViewModel.Model);

            // Insert a list in the same position
            DataModelConditionGeneralPredicate predicate = new(DataModelConditionGroup, ProfileRightSideType.Dynamic);
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

    public enum ConditionGroupType
    {
        General,
        List,
        Event
    }
}
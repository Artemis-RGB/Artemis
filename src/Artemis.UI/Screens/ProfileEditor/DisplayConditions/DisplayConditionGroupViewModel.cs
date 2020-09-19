using System;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.ProfileEditor.DisplayConditions.Abstract;
using Artemis.UI.Shared.Services;
using Humanizer;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.DisplayConditions
{
    public class DisplayConditionGroupViewModel : DisplayConditionViewModel
    {
        private readonly IDisplayConditionsVmFactory _displayConditionsVmFactory;
        private readonly IProfileEditorService _profileEditorService;
        private bool _isInitialized;
        private bool _isRootGroup;

        public DisplayConditionGroupViewModel(DataModelConditionGroup dataModelConditionGroup,
            bool isListGroup,
            IProfileEditorService profileEditorService,
            IDisplayConditionsVmFactory displayConditionsVmFactory)
            : base(dataModelConditionGroup)
        {
            IsListGroup = isListGroup;
            _profileEditorService = profileEditorService;
            _displayConditionsVmFactory = displayConditionsVmFactory;

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
            var enumValue = Enum.Parse<BooleanOperator>(type);
            DataModelConditionGroup.BooleanOperator = enumValue;
            NotifyOfPropertyChange(nameof(SelectedBooleanOperator));

            _profileEditorService.UpdateSelectedProfileElement();
        }

        public void AddCondition(string type)
        {
            if (type == "Static")
            {
                if (!IsListGroup)
                    DataModelConditionGroup.AddChild(new DataModelConditionPredicate(DataModelConditionGroup, ProfileRightSideType.Static));
                else
                    DataModelConditionGroup.AddChild(new DataModelConditionListPredicate(DataModelConditionGroup, ProfileRightSideType.Static));
            }
            else if (type == "Dynamic")
            {
                if (!IsListGroup)
                    DataModelConditionGroup.AddChild(new DataModelConditionPredicate(DataModelConditionGroup, ProfileRightSideType.Dynamic));
                else
                    DataModelConditionGroup.AddChild(new DataModelConditionListPredicate(DataModelConditionGroup, ProfileRightSideType.Dynamic));
            }
            else if (type == "List" && !IsListGroup)
                DataModelConditionGroup.AddChild(new DataModelConditionList(DataModelConditionGroup));

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
            var toRemove = Items.Where(c => !DataModelConditionGroup.Children.Contains(c.Model)).ToList();
            // Using RemoveRange breaks our lovely animations
            foreach (var displayConditionViewModel in toRemove)
                Items.Remove(displayConditionViewModel);

            foreach (var childModel in Model.Children)
            {
                if (Items.Any(c => c.Model == childModel))
                    continue;

                switch (childModel)
                {
                    case DataModelConditionGroup displayConditionGroup:
                        Items.Add(_displayConditionsVmFactory.DisplayConditionGroupViewModel(displayConditionGroup, IsListGroup));
                        break;
                    case DataModelConditionList displayConditionListPredicate:
                        Items.Add(_displayConditionsVmFactory.DisplayConditionListViewModel(displayConditionListPredicate));
                        break;
                    case DataModelConditionPredicate displayConditionPredicate:
                        if (!IsListGroup)
                            Items.Add(_displayConditionsVmFactory.DisplayConditionPredicateViewModel(displayConditionPredicate));
                        break;
                    case DataModelConditionListPredicate displayConditionListPredicate:
                        if (IsListGroup)
                            Items.Add(_displayConditionsVmFactory.DisplayConditionListPredicateViewModel(displayConditionListPredicate));
                        break;
                }
            }

            foreach (var childViewModel in Items)
                childViewModel.Update();

            if (IsRootGroup) 
                ((DisplayConditionsViewModel) Parent).DisplayStartHint = !Items.Any();
        }
    }
}
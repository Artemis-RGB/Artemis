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

        public DisplayConditionGroupViewModel(DisplayConditionGroup displayConditionGroup,
            bool isListGroup,
            IProfileEditorService profileEditorService,
            IDisplayConditionsVmFactory displayConditionsVmFactory)
            : base(displayConditionGroup)
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

        public DisplayConditionGroup DisplayConditionGroup => (DisplayConditionGroup) Model;

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
        public string SelectedBooleanOperator => DisplayConditionGroup.BooleanOperator.Humanize();

        public void SelectBooleanOperator(string type)
        {
            var enumValue = Enum.Parse<BooleanOperator>(type);
            DisplayConditionGroup.BooleanOperator = enumValue;
            NotifyOfPropertyChange(nameof(SelectedBooleanOperator));

            _profileEditorService.UpdateSelectedProfileElement();
        }

        public void AddCondition(string type)
        {
            if (type == "Static")
            {
                if (!IsListGroup)
                    DisplayConditionGroup.AddChild(new DisplayConditionPredicate(DisplayConditionGroup, ProfileRightSideType.Static));
                else
                    DisplayConditionGroup.AddChild(new DisplayConditionListPredicate(DisplayConditionGroup, ProfileRightSideType.Static));
            }
            else if (type == "Dynamic")
            {
                if (!IsListGroup)
                    DisplayConditionGroup.AddChild(new DisplayConditionPredicate(DisplayConditionGroup, ProfileRightSideType.Dynamic));
                else
                    DisplayConditionGroup.AddChild(new DisplayConditionListPredicate(DisplayConditionGroup, ProfileRightSideType.Dynamic));
            }
            else if (type == "List" && !IsListGroup)
                DisplayConditionGroup.AddChild(new DisplayConditionList(DisplayConditionGroup));

            Update();
            _profileEditorService.UpdateSelectedProfileElement();
        }

        public void AddGroup()
        {
            DisplayConditionGroup.AddChild(new DisplayConditionGroup(DisplayConditionGroup));

            Update();
            _profileEditorService.UpdateSelectedProfileElement();
        }

        public override void Update()
        {
            NotifyOfPropertyChange(nameof(SelectedBooleanOperator));

            // Remove VMs of effects no longer applied on the layer
            var toRemove = Items.Where(c => !DisplayConditionGroup.Children.Contains(c.Model)).ToList();
            // Using RemoveRange breaks our lovely animations
            foreach (var displayConditionViewModel in toRemove)
                CloseItem(displayConditionViewModel);

            foreach (var childModel in Model.Children)
            {
                if (Items.Any(c => c.Model == childModel))
                    continue;

                switch (childModel)
                {
                    case DisplayConditionGroup displayConditionGroup:
                        ActivateItem(_displayConditionsVmFactory.DisplayConditionGroupViewModel(displayConditionGroup, IsListGroup));
                        break;
                    case DisplayConditionList displayConditionListPredicate:
                        ActivateItem(_displayConditionsVmFactory.DisplayConditionListViewModel(displayConditionListPredicate));
                        break;
                    case DisplayConditionPredicate displayConditionPredicate:
                        if (!IsListGroup)
                            ActivateItem(_displayConditionsVmFactory.DisplayConditionPredicateViewModel(displayConditionPredicate));
                        break;
                    case DisplayConditionListPredicate displayConditionListPredicate:
                        if (IsListGroup)
                            ActivateItem(_displayConditionsVmFactory.DisplayConditionListPredicateViewModel(displayConditionListPredicate));
                        break;
                }
            }

            foreach (var childViewModel in Items)
                childViewModel.Update();
        }
    }
}
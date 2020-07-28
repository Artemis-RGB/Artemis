using System;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core.Models.Profile.Conditions;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.Module.ProfileEditor.DisplayConditions.Abstract;
using Humanizer;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.DisplayConditions
{
    public class DisplayConditionGroupViewModel : DisplayConditionViewModel
    {
        private readonly IDisplayConditionsVmFactory _displayConditionsVmFactory;
        private bool _isRootGroup;
        private bool _isInitialized;

        public DisplayConditionGroupViewModel(DisplayConditionGroup displayConditionGroup, DisplayConditionViewModel parent, IDisplayConditionsVmFactory displayConditionsVmFactory) : base(
            displayConditionGroup, parent)
        {
            _displayConditionsVmFactory = displayConditionsVmFactory;
            Execute.PostToUIThread(async () =>
            {
                await Task.Delay(50);
                IsInitialized = true;
            });
        }

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

        public string SelectedBooleanOperator => DisplayConditionGroup.BooleanOperator.Humanize();

        public void SelectBooleanOperator(string type)
        {
            var enumValue = Enum.Parse<BooleanOperator>(type);
            DisplayConditionGroup.BooleanOperator = enumValue;
            NotifyOfPropertyChange(nameof(SelectedBooleanOperator));
        }

        public void AddCondition(string type)
        {
            if (type == "Static")
                DisplayConditionGroup.AddChild(new DisplayConditionPredicate(DisplayConditionGroup, PredicateType.Static));
            else if (type == "Dynamic")
                DisplayConditionGroup.AddChild(new DisplayConditionPredicate(DisplayConditionGroup, PredicateType.Dynamic));
            Update();
        }

        public void AddGroup()
        {
            DisplayConditionGroup.AddChild(new DisplayConditionGroup(DisplayConditionGroup));
            Update();
        }

        public override void Update()
        {
            NotifyOfPropertyChange(nameof(SelectedBooleanOperator));

            // Remove VMs of effects no longer applied on the layer
            var toRemove = Children.Where(c => !DisplayConditionGroup.Children.Contains(c.Model)).ToList();
            // Using RemoveRange breaks our lovely animations
            foreach (var displayConditionViewModel in toRemove)
                Children.Remove(displayConditionViewModel);

            foreach (var childModel in Model.Children)
            {
                if (Children.Any(c => c.Model == childModel))
                    continue;

                switch (childModel)
                {
                    case DisplayConditionGroup displayConditionGroup:
                        Children.Add(_displayConditionsVmFactory.DisplayConditionGroupViewModel(displayConditionGroup, this));
                        break;
                    case DisplayConditionListPredicate displayConditionListPredicate:
                        Children.Add(_displayConditionsVmFactory.DisplayConditionListPredicateViewModel(displayConditionListPredicate, this));
                        break;
                    case DisplayConditionPredicate displayConditionPredicate:
                        Children.Add(_displayConditionsVmFactory.DisplayConditionPredicateViewModel(displayConditionPredicate, this));
                        break;
                }
            }

            foreach (var childViewModel in Children)
                childViewModel.Update();
        }
    }
}
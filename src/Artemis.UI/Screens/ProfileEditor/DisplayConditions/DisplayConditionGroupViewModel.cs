using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Artemis.Core.Models.Profile.Conditions;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.ProfileEditor.DisplayConditions.Abstract;
using Artemis.UI.Shared.Services.Interfaces;
using Humanizer;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.DisplayConditions
{
    public class DisplayConditionGroupViewModel : DisplayConditionViewModel, IViewAware
    {
        private readonly IDisplayConditionsVmFactory _displayConditionsVmFactory;
        private readonly IProfileEditorService _profileEditorService;
        private bool _isInitialized;
        private bool _isRootGroup;

        public DisplayConditionGroupViewModel(DisplayConditionGroup displayConditionGroup, DisplayConditionViewModel parent,
            IProfileEditorService profileEditorService, IDisplayConditionsVmFactory displayConditionsVmFactory) : base(displayConditionGroup, parent)
        {
            _profileEditorService = profileEditorService;
            _displayConditionsVmFactory = displayConditionsVmFactory;

            Children.CollectionChanged += (sender, args) => NotifyOfPropertyChange(nameof(DisplayBooleanOperator));

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

        public bool DisplayBooleanOperator => Children.Count > 1;
        public string SelectedBooleanOperator => DisplayConditionGroup.BooleanOperator.Humanize();

        public void AttachView(UIElement view)
        {
            View = view;
        }

        public UIElement View { get; set; }

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
            else if (type == "List")
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
            var toRemove = Children.Where(c => !DisplayConditionGroup.Children.Contains(c.Model)).ToList();
            // Using RemoveRange breaks our lovely animations
            foreach (var displayConditionViewModel in toRemove)
            {
                Children.Remove(displayConditionViewModel);
                displayConditionViewModel.Dispose();
            }

            foreach (var childModel in Model.Children)
            {
                if (Children.Any(c => c.Model == childModel))
                    continue;

                switch (childModel)
                {
                    case DisplayConditionGroup displayConditionGroup:
                        Children.Add(_displayConditionsVmFactory.DisplayConditionGroupViewModel(displayConditionGroup, this));
                        break;
                    case DisplayConditionList displayConditionListPredicate:
                        Children.Add(_displayConditionsVmFactory.DisplayConditionListViewModel(displayConditionListPredicate, this));
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
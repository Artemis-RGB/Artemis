using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core;
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
            if (IsListGroup)
                DynamicListConditionSupported = !((DataModelConditionList) dataModelConditionGroup.Parent).IsPrimitiveList;
            else
                DynamicListConditionSupported = false;

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
        public bool DynamicListConditionSupported { get; }
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

        public void AddCondition(string type)
        {
            if (type == "Static")
            {
                if (!IsListGroup)
                    DataModelConditionGroup.AddChild(new DataModelConditionPredicate(DataModelConditionGroup, ProfileRightSideType.Static));
                else
                    DataModelConditionGroup.AddChild(new DataModelConditionListPredicate(DataModelConditionGroup, ListRightSideType.Static));
            }
            else if (type == "Dynamic")
            {
                if (!IsListGroup)
                    DataModelConditionGroup.AddChild(new DataModelConditionPredicate(DataModelConditionGroup, ProfileRightSideType.Dynamic));
                else
                    DataModelConditionGroup.AddChild(new DataModelConditionListPredicate(DataModelConditionGroup, ListRightSideType.Dynamic));
            }
            else if (type == "DynamicList" && IsListGroup)
                DataModelConditionGroup.AddChild(new DataModelConditionListPredicate(DataModelConditionGroup, ListRightSideType.DynamicList));
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
            Items.RemoveRange(Items.Where(c => !DataModelConditionGroup.Children.Contains(c.Model)).ToList());

            List<DataModelConditionViewModel> viewModels = new List<DataModelConditionViewModel>();
            foreach (DataModelConditionPart childModel in Model.Children)
            {
                if (Items.Any(c => c.Model == childModel))
                    continue;

                switch (childModel)
                {
                    case DataModelConditionGroup DataModelConditionGroup:
                        viewModels.Add(_dataModelConditionsVmFactory.DataModelConditionGroupViewModel(DataModelConditionGroup, IsListGroup));
                        break;
                    case DataModelConditionList DataModelConditionListPredicate:
                        viewModels.Add(_dataModelConditionsVmFactory.DataModelConditionListViewModel(DataModelConditionListPredicate));
                        break;
                    case DataModelConditionPredicate DataModelConditionPredicate:
                        if (!IsListGroup)
                            viewModels.Add(_dataModelConditionsVmFactory.DataModelConditionPredicateViewModel(DataModelConditionPredicate));
                        break;
                    case DataModelConditionListPredicate DataModelConditionListPredicate:
                        if (IsListGroup)
                            viewModels.Add(_dataModelConditionsVmFactory.DataModelConditionListPredicateViewModel(DataModelConditionListPredicate));
                        break;
                }
            }
            if (viewModels.Any())
                Items.AddRange(viewModels);

            foreach (DataModelConditionViewModel childViewModel in Items)
                childViewModel.Update();

            if (IsRootGroup && Parent is DisplayConditionsViewModel displayConditionsViewModel)
                displayConditionsViewModel.DisplayStartHint = !Items.Any();

            OnUpdated();
        }

        public event EventHandler Updated;

        protected virtual void OnUpdated()
        {
            Updated?.Invoke(this, EventArgs.Empty);
        }
    }
}
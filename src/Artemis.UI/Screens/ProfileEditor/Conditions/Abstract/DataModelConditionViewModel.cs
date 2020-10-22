using System;
using Artemis.Core;
using Artemis.UI.Shared.Input;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.Conditions.Abstract
{
    public abstract class DataModelConditionViewModel : Conductor<DataModelConditionViewModel>.Collection.AllActive
    {
        private DataModelDynamicViewModel _leftSideSelectionViewModel;

        protected DataModelConditionViewModel(DataModelConditionPart model)
        {
            Model = model;
        }

        public DataModelConditionPart Model { get; }

        public DataModelDynamicViewModel LeftSideSelectionViewModel
        {
            get => _leftSideSelectionViewModel;
            set => SetAndNotify(ref _leftSideSelectionViewModel, value);
        }

        public abstract void Update();

        public virtual void Delete()
        {
            Model.Parent.RemoveChild(Model);
            ((DataModelConditionViewModel) Parent).Update();
        }

        protected bool ConvertIfRequired(Type newType)
        {
            if (newType == null)
                return false;

            if (!(Parent is DataModelConditionGroupViewModel groupViewModel))
                return false;

            // List
            if (newType.IsGenericEnumerable())
            {
                if (this is DataModelConditionListViewModel)
                    return false;
                groupViewModel.ConvertToConditionList(this);
                return true;
            }

            // Event
            if (IsEvent(newType))
            {
                if (this is DataModelConditionEventViewModel)
                    return false;
                groupViewModel.ConvertToConditionEvent(this);
                return true;
            }

            // Predicate
            if (this is DataModelConditionPredicateViewModel)
                return false;
            groupViewModel.ConvertToPredicate(this);
            return true;
        }

        protected bool IsEvent(Type type)
        {
            return type == typeof(DataModelEvent) || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(DataModelEvent<>);
        }
    }
}
using System.Linq;
using Artemis.Core.Exceptions;
using Artemis.Core.Models.Profile.Conditions.Abstract;
using Artemis.Core.Plugins.Abstract.DataModels;
using Artemis.Core.Services.Interfaces;
using Artemis.Storage.Entities.Profile;
using Artemis.Storage.Entities.Profile.Abstract;

namespace Artemis.Core.Models.Profile.Conditions
{
    public class DisplayConditionListPredicate : DisplayConditionPart
    {
        public DisplayConditionListPredicate(DisplayConditionPart parent)
        {
            Parent = parent;
            DisplayConditionListPredicateEntity = new DisplayConditionListPredicateEntity();
        }

        public DisplayConditionListPredicate(DisplayConditionPart parent, DisplayConditionListPredicateEntity entity)
        {
            Parent = parent;
            DisplayConditionListPredicateEntity = entity;
            ListOperator = (ListOperator) entity.ListOperator;

            foreach (var childEntity in DisplayConditionListPredicateEntity.Children)
            {
                if (childEntity is DisplayConditionGroupEntity groupEntity)
                    AddChild(new DisplayConditionGroup(this, groupEntity));
                else if (childEntity is DisplayConditionPredicateEntity predicateEntity)
                    AddChild(new DisplayConditionPredicate(this, predicateEntity));
                else if (childEntity is DisplayConditionListPredicateEntity listPredicateEntity)
                    AddChild(new DisplayConditionListPredicate(this, listPredicateEntity));
            }
        }

        public DisplayConditionListPredicateEntity DisplayConditionListPredicateEntity { get; set; }

        public ListOperator ListOperator { get; set; }
        public DataModel ListDataModel { get; private set; }
        public string ListPropertyPath { get; private set; }

        public override bool Evaluate()
        {
            return true;
        }

        internal override void ApplyToEntity()
        {
            // Target list
            DisplayConditionListPredicateEntity.ListDataModelGuid = ListDataModel?.PluginInfo?.Guid;
            DisplayConditionListPredicateEntity.ListPropertyPath = ListPropertyPath;

            // Operator
            DisplayConditionListPredicateEntity.ListOperator = (int) ListOperator;

            // Children
            DisplayConditionListPredicateEntity.Children.Clear();
            DisplayConditionListPredicateEntity.Children.AddRange(Children.Select(c => c.GetEntity()));
            foreach (var child in Children)
                child.ApplyToEntity();
        }

        internal override DisplayConditionPartEntity GetEntity()
        {
            return DisplayConditionListPredicateEntity;
        }

        internal override void Initialize(IDataModelService dataModelService)
        {
            // Target list
            if (DisplayConditionListPredicateEntity.ListDataModelGuid != null)
            {
                var dataModel = dataModelService.GetPluginDataModelByGuid(DisplayConditionListPredicateEntity.ListDataModelGuid.Value);
                if (dataModel != null && dataModel.ContainsPath(DisplayConditionListPredicateEntity.ListPropertyPath))
                    UpdateList(dataModel, DisplayConditionListPredicateEntity.ListPropertyPath);
            }

            // Children
            foreach (var child in Children)
                child.Initialize(dataModelService);
        }

        public void UpdateList(DataModel dataModel, string path)
        {
            if (dataModel != null && path == null)
                throw new ArtemisCoreException("If a data model is provided, a path is also required");
            if (dataModel == null && path != null)
                throw new ArtemisCoreException("If path is provided, a data model is also required");

            if (dataModel != null)
            {
                if (!dataModel.ContainsPath(path))
                    throw new ArtemisCoreException($"Data model of type {dataModel.GetType().Name} does not contain a property at path '{path}'");
            }

            ListDataModel = dataModel;
            ListPropertyPath = path;
        }
    }

    public enum ListOperator
    {
        Any,
        All,
        None,
        Count
    }
}
using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using Artemis.Core.Exceptions;
using Artemis.Core.Models.Profile.Conditions.Abstract;
using Artemis.Core.Plugins.Abstract.DataModels;
using Artemis.Core.Services.Interfaces;
using Artemis.Storage.Entities.Profile;
using Artemis.Storage.Entities.Profile.Abstract;

namespace Artemis.Core.Models.Profile.Conditions
{
    public class DisplayConditionList : DisplayConditionPart
    {
        public DisplayConditionList(DisplayConditionPart parent)
        {
            Parent = parent;
            Entity = new DisplayConditionListEntity();

            // There is always a child root group, add it
            AddChild(new DisplayConditionGroup(this));
        }

        public DisplayConditionList(DisplayConditionPart parent, DisplayConditionListEntity entity)
        {
            Parent = parent;
            Entity = entity;
            ListOperator = (ListOperator) entity.ListOperator;

            // There should only be one child and it should be a group
            var rootGroup = Entity.Children.SingleOrDefault() as DisplayConditionGroupEntity;
            if (rootGroup == null)
            {
                Entity.Children.Clear();
                AddChild(new DisplayConditionGroup(this));
            }
            else
                AddChild(new DisplayConditionGroup(this, rootGroup));
        }

        public DisplayConditionListEntity Entity { get; set; }

        public ListOperator ListOperator { get; set; }
        public DataModel ListDataModel { get; private set; }
        public string ListPropertyPath { get; private set; }

        public override bool Evaluate()
        {
            if (CompiledListAccessor == null)
                return false;

            return EvaluateObject(CompiledListAccessor(ListDataModel));
        }

        public override bool EvaluateObject(object target)
        {
            if (!(target is IList list))
                return false;

            var objectList = list.Cast<object>();
            return ListOperator switch
            {
                ListOperator.Any => objectList.Any(o => Children[0].EvaluateObject(o)),
                ListOperator.All => objectList.All(o => Children[0].EvaluateObject(o)),
                ListOperator.None => objectList.Any(o => !Children[0].EvaluateObject(o)),
                ListOperator.Count => false,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        internal override void ApplyToEntity()
        {
            // Target list
            Entity.ListDataModelGuid = ListDataModel?.PluginInfo?.Guid;
            Entity.ListPropertyPath = ListPropertyPath;

            // Operator
            Entity.ListOperator = (int) ListOperator;

            // Children
            Entity.Children.Clear();
            Entity.Children.AddRange(Children.Select(c => c.GetEntity()));
            foreach (var child in Children)
                child.ApplyToEntity();
        }

        internal override DisplayConditionPartEntity GetEntity()
        {
            return Entity;
        }

        internal override void Initialize(IDataModelService dataModelService)
        {
            // Target list
            if (Entity.ListDataModelGuid != null)
            {
                var dataModel = dataModelService.GetPluginDataModelByGuid(Entity.ListDataModelGuid.Value);
                if (dataModel != null && dataModel.ContainsPath(Entity.ListPropertyPath))
                    UpdateList(dataModel, Entity.ListPropertyPath);
            }

            // Children
            var rootGroup = (DisplayConditionGroup) Children.Single();
            rootGroup.Initialize(dataModelService);
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
                if (dataModel.GetListTypeAtPath(path) == null)
                    throw new ArtemisCoreException($"The path '{path}' does not contain a list");
            }

            ListDataModel = dataModel;
            ListPropertyPath = path;

            if (dataModel != null)
            {
                var parameter = Expression.Parameter(typeof(object), "listDataModel");
                var accessor = path.Split('.').Aggregate<string, Expression>(
                    Expression.Convert(parameter, dataModel.GetType()),
                    (expression, s) => Expression.Convert(Expression.Property(expression, s), typeof(IList)));

                var lambda = Expression.Lambda<Func<object, IList>>(accessor, parameter);
                CompiledListAccessor = lambda.Compile();
            }
        }

        public Func<object, IList> CompiledListAccessor { get; set; }
    }

    public enum ListOperator
    {
        Any,
        All,
        None,
        Count
    }
}
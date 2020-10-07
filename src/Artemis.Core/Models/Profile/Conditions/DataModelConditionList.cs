using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.DataModelExpansions;
using Artemis.Storage.Entities.Profile.Abstract;
using Artemis.Storage.Entities.Profile.Conditions;

namespace Artemis.Core
{
    /// <summary>
    ///     A condition that evaluates one or more predicates inside a list
    /// </summary>
    public class DataModelConditionList : DataModelConditionPart
    {
        private bool _disposed;

        /// <summary>
        ///     Creates a new instance of the <see cref="DataModelConditionList" /> class
        /// </summary>
        /// <param name="parent"></param>
        public DataModelConditionList(DataModelConditionPart parent)
        {
            Parent = parent;
            Entity = new DataModelConditionListEntity();

            Initialize();
        }

        internal DataModelConditionList(DataModelConditionPart parent, DataModelConditionListEntity entity)
        {
            Parent = parent;
            Entity = entity;
            ListOperator = (ListOperator) entity.ListOperator;

            Initialize();
        }

        /// <summary>
        ///     Gets or sets the list operator
        /// </summary>
        public ListOperator ListOperator { get; set; }

        /// <summary>
        ///     Gets the path of the list property
        /// </summary>
        public DataModelPath? ListPath { get; set; }

        /// <summary>
        ///     Gets the type of the content of the list this predicate is evaluated on
        /// </summary>
        public Type? ListType { get; set; }

        /// <summary>
        ///     Gets whether the list contains primitives
        /// </summary>
        public bool IsPrimitiveList { get; set; }

        internal DataModelConditionListEntity Entity { get; set; }

        /// <inheritdoc />
        public override bool Evaluate()
        {
            if (_disposed)
                throw new ObjectDisposedException("DataModelConditionList");

            if (ListPath == null || !ListPath.IsValid)
                return false;

            return EvaluateObject(ListPath.GetValue());
        }

        /// <summary>
        ///     Updates the list the predicate is evaluated on
        /// </summary>
        /// <param name="dataModel">The data model of the list</param>
        /// <param name="path">The path pointing to the list inside the list</param>
        public void UpdateList(DataModel? dataModel, string? path)
        {
            if (_disposed)
                throw new ObjectDisposedException("DataModelConditionList");

            if (dataModel != null && path == null)
                throw new ArtemisCoreException("If a data model is provided, a path is also required");
            if (dataModel == null && path != null)
                throw new ArtemisCoreException("If path is provided, a data model is also required");

            ListPath?.Dispose();
            if (dataModel != null && path != null)
            {
                DataModelPath newPath = new DataModelPath(dataModel, path);
                if (!newPath.IsValid)
                    throw new ArtemisCoreException($"New left path '{newPath}' is invalid");
                Type listType = newPath.GetPropertyType()!;
                if (!typeof(IList).IsAssignableFrom(listType))
                    throw new ArtemisCoreException($"Data model of type {dataModel.GetType().Name} does not contain a list at path '{newPath}'");

                ListPath = newPath;
                ListType = listType;
                IsPrimitiveList = listType.IsPrimitive || listType.IsEnum || listType == typeof(string);
            }
            else
            {
                ListPath = null;
                ListType = null;
            }

            // Remove the old root group that was tied to the old data model
            while (Children.Any())
                RemoveChild(Children[0]);

            if (dataModel == null)
                return;

            // Create a new root group
            AddChild(new DataModelConditionGroup(this));
        }

        #region IDisposable

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            _disposed = true;

            ListPath?.Dispose();

            foreach (DataModelConditionPart child in Children)
                child.Dispose();

            base.Dispose(disposing);
        }

        #endregion

        internal override bool EvaluateObject(object? target)
        {
            if (_disposed)
                throw new ObjectDisposedException("DataModelConditionList");

            if (!Children.Any())
                return false;
            if (!(target is IList list))
                return false;

            IEnumerable<object> objectList = list.Cast<object>();
            return ListOperator switch
            {
                ListOperator.Any => objectList.Any(o => Children[0].EvaluateObject(o)),
                ListOperator.All => objectList.All(o => Children[0].EvaluateObject(o)),
                ListOperator.None => objectList.Any(o => !Children[0].EvaluateObject(o)),
                ListOperator.Count => false,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        internal override void Save()
        {
            // Target list
            ListPath?.Save();
            Entity.ListPath = ListPath?.Entity;

            // Operator
            Entity.ListOperator = (int) ListOperator;

            // Children
            Entity.Children.Clear();
            Entity.Children.AddRange(Children.Select(c => c.GetEntity()));
            foreach (DataModelConditionPart child in Children)
                child.Save();
        }

        internal override DataModelConditionPartEntity GetEntity()
        {
            return Entity;
        }

        internal void Initialize()
        {
            if (Entity.ListPath == null)
                return;

            // Ensure the list path is valid and points to a list
            DataModelPath listPath = new DataModelPath(null, Entity.ListPath);
            if (!listPath.IsValid || !typeof(IList).IsAssignableFrom(listPath.GetPropertyType()))
                return;

            ListPath = listPath;

            // There should only be one child and it should be a group
            if (Entity.Children.SingleOrDefault() is DataModelConditionGroupEntity rootGroup)
            {
                AddChild(new DataModelConditionGroup(this, rootGroup));
            }
            else
            {
                Entity.Children.Clear();
                AddChild(new DataModelConditionGroup(this));
            }
        }
    }

    /// <summary>
    ///     Represents a list operator
    /// </summary>
    public enum ListOperator
    {
        /// <summary>
        ///     Any of the list items should evaluate to true
        /// </summary>
        Any,

        /// <summary>
        ///     All of the list items should evaluate to true
        /// </summary>
        All,

        /// <summary>
        ///     None of the list items should evaluate to true
        /// </summary>
        None,

        /// <summary>
        ///     A specific amount of the list items should evaluate to true
        /// </summary>
        Count
    }
}
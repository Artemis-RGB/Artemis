using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        private bool _reinitializing;

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
        public DataModelPath? ListPath { get; private set; }

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
        /// <param name="path">The path pointing to the list inside the list</param>
        public void UpdateList(DataModelPath? path)
        {
            if (_disposed)
                throw new ObjectDisposedException("DataModelConditionList");

            if (path != null && !path.IsValid)
                throw new ArtemisCoreException("Cannot update list to an invalid path");

            ListPath?.Dispose();
            ListPath = path != null ? new DataModelPath(path) : null;
            SubscribeToListPath();

            // Remove the old root group that was tied to the old data model
            while (Children.Any())
                RemoveChild(Children[0]);

            if (ListPath != null)
            {
                Type listType = ListPath.GetPropertyType()!;
                ListType = listType.GetGenericEnumerableType();
                IsPrimitiveList = ListType.IsPrimitive || ListType.IsEnum || ListType == typeof(string);

                // Create a new root group
                AddChild(new DataModelConditionGroup(this));
            }
            else
            {
                ListType = null;
            }
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
            if (!(target is IEnumerable enumerable))
                return false;

            IEnumerable<object> objectList = enumerable.Cast<object>();
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
            // Don't save an invalid state
            if (ListPath != null && !ListPath.IsValid)
                return;

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
            while (Children.Any())
                RemoveChild(Children[0]);

            if (Entity.ListPath == null)
                return;

            // Ensure the list path is valid and points to a list
            DataModelPath listPath = new DataModelPath(null, Entity.ListPath);
            Type listType = listPath.GetPropertyType()!;
            // Can't check this on an invalid list, if it becomes valid later lets hope for the best
            if (listPath.IsValid && !PointsToList(listPath))
                return;

            ListPath = listPath;
            SubscribeToListPath();
            if (ListPath.IsValid)
            {
                ListType = listType.GetGenericEnumerableType();
                IsPrimitiveList = ListType.IsPrimitive || ListType.IsEnum || ListType == typeof(string);
            }
            else
            {
                ListType = null;
                IsPrimitiveList = false;
            }

            // There should only be one child and it should be a group
            if (Entity.Children.FirstOrDefault() is DataModelConditionGroupEntity rootGroup)
            {
                AddChild(new DataModelConditionGroup(this, rootGroup));
            }
            else
            {
                Entity.Children.Clear();
                AddChild(new DataModelConditionGroup(this));
            }
        }

        private bool PointsToList(DataModelPath dataModelPath)
        {
            Type? type = dataModelPath.GetPropertyType();
            return type?.IsGenericEnumerable() ?? false;
        }

        private void SubscribeToListPath()
        {
            if (ListPath == null) return;
            ListPath.PathValidated += ListPathOnPathValidated;
            ListPath.PathInvalidated += ListPathOnPathInvalidated;
        }

        #region Event handlers

        private void ListPathOnPathValidated(object? sender, EventArgs e)
        {
            if (_reinitializing)
                return;

            _reinitializing = true;
            ListPath?.Dispose();
            Initialize();
            _reinitializing = false;
        }

        private void ListPathOnPathInvalidated(object? sender, EventArgs e)
        {
            if (_reinitializing)
                return;

            _reinitializing = true;
            ListPath?.Dispose();
            Initialize();
            _reinitializing = false;
        }

        #endregion
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
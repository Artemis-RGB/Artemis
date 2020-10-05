using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        internal DataModelConditionListEntity Entity { get; set; }

        /// <summary>
        ///     Gets or sets the list operator
        /// </summary>
        public ListOperator ListOperator { get; set; }

        /// <summary>
        ///     Gets the type of the content of the list this predicate is evaluated on
        /// </summary>
        public Type ListType { get; set; }

        /// <summary>
        ///     Gets whether the list contains primitives
        /// </summary>
        public bool IsPrimitiveList { get; set; }

        /// <summary>
        ///     Gets the currently used instance of the list data model
        /// </summary>
        public DataModel ListDataModel { get; private set; }

        /// <summary>
        ///     Gets the path of the list property in the <see cref="ListDataModel" />
        /// </summary>
        public string ListPropertyPath { get; private set; }

        /// <summary>
        ///     Gets the compiled function that accesses the list this condition evaluates on
        /// </summary>
        public Func<object, IList> CompiledListAccessor { get; private set; }

        /// <inheritdoc />
        public override bool Evaluate()
        {
            if (_disposed)
                throw new ObjectDisposedException("DataModelConditionList");

            if (CompiledListAccessor == null)
                return false;

            return EvaluateObject(CompiledListAccessor(ListDataModel));
        }

        /// <summary>
        ///     Updates the list the predicate is evaluated on
        /// </summary>
        /// <param name="dataModel">The data model of the list</param>
        /// <param name="path">The path pointing to the list inside the list</param>
        public void UpdateList(DataModel dataModel, string path)
        {
            if (_disposed)
                throw new ObjectDisposedException("DataModelConditionList");

            if (dataModel != null && path == null)
                throw new ArtemisCoreException("If a data model is provided, a path is also required");
            if (dataModel == null && path != null)
                throw new ArtemisCoreException("If path is provided, a data model is also required");

            if (dataModel != null)
            {
                Type listType = dataModel.GetListTypeAtPath(path);
                if (listType == null)
                    throw new ArtemisCoreException($"Data model of type {dataModel.GetType().Name} does not contain a list at path '{path}'");

                ListType = listType;
                IsPrimitiveList = listType.IsPrimitive || listType.IsEnum || listType == typeof(string);

                ListDataModel = dataModel;
                ListPropertyPath = path;
            }

            // Remove the old root group that was tied to the old data model
            while (Children.Any())
                RemoveChild(Children[0]);

            if (dataModel == null)
                return;

            // Create a new root group
            AddChild(new DataModelConditionGroup(this));
            CreateExpression();
        }

        #region IDisposable

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            _disposed = true;

            DataModelStore.DataModelAdded -= DataModelStoreOnDataModelAdded;
            DataModelStore.DataModelRemoved -= DataModelStoreOnDataModelRemoved;

            foreach (DataModelConditionPart child in Children)
                child.Dispose();

            base.Dispose(disposing);
        }

        #endregion

        internal override bool EvaluateObject(object target)
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
            if (ListDataModel != null)
            {
                Entity.ListDataModelGuid = ListDataModel.PluginInfo.Guid;
                Entity.ListPropertyPath = ListPropertyPath;
            }

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
            DataModelStore.DataModelAdded += DataModelStoreOnDataModelAdded;
            DataModelStore.DataModelRemoved += DataModelStoreOnDataModelRemoved;
            if (Entity.ListDataModelGuid == null)
                return;

            // Get the data model ...
            DataModel dataModel = DataModelStore.Get(Entity.ListDataModelGuid.Value)?.DataModel;
            if (dataModel == null)
                return;
            // ... and ensure the path is valid
            Type listType = dataModel.GetListTypeAtPath(Entity.ListPropertyPath);
            if (listType == null)
                return;

            ListType = listType;
            IsPrimitiveList = listType.IsPrimitive || listType.IsEnum || listType == typeof(string);
            ListDataModel = dataModel;
            ListPropertyPath = Entity.ListPropertyPath;

            CreateExpression();

            if (ListDataModel == null)
                return;

            // There should only be one child and it should be a group
            if (Entity.Children.SingleOrDefault() is DataModelConditionGroupEntity rootGroup)
                AddChild(new DataModelConditionGroup(this, rootGroup));
            else
            {
                Entity.Children.Clear();
                AddChild(new DataModelConditionGroup(this));
            }
        }

        private void CreateExpression()
        {
            if (_disposed)
                throw new ObjectDisposedException("DataModelConditionList");

            ParameterExpression parameter = Expression.Parameter(typeof(object), "listDataModel");
            Expression accessor = ListPropertyPath.Split('.').Aggregate<string, Expression>(
                Expression.Convert(parameter, ListDataModel.GetType()),
                Expression.Property
            );
            accessor = Expression.Convert(accessor, typeof(IList));

            Expression<Func<object, IList>> lambda = Expression.Lambda<Func<object, IList>>(accessor, parameter);
            CompiledListAccessor = lambda.Compile();
        }


        #region Event handlers

        private void DataModelStoreOnDataModelAdded(object sender, DataModelStoreEvent e)
        {
            DataModel dataModel = e.Registration.DataModel;
            if (dataModel.PluginInfo.Guid == Entity.ListDataModelGuid && dataModel.ContainsPath(Entity.ListPropertyPath))
            {
                ListDataModel = dataModel;
                ListPropertyPath = Entity.ListPropertyPath;
                CreateExpression();
            }
        }

        private void DataModelStoreOnDataModelRemoved(object sender, DataModelStoreEvent e)
        {
            if (ListDataModel != e.Registration.DataModel)
                return;

            ListDataModel = null;
            CompiledListAccessor = null;
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
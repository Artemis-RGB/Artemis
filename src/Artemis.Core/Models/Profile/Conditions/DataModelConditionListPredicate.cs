using System;
using System.Linq;
using System.Linq.Expressions;
using Artemis.Core.DataModelExpansions;
using Artemis.Storage.Entities.Profile.Abstract;
using Artemis.Storage.Entities.Profile.Conditions;
using Newtonsoft.Json;

namespace Artemis.Core
{
    /// <summary>
    ///     A predicate like evaluated inside a <see cref="DataModelConditionList" />
    /// </summary>
    public class DataModelConditionListPredicate : DataModelConditionPart
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="DataModelConditionListPredicate" /> class
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="predicateType"></param>
        public DataModelConditionListPredicate(DataModelConditionPart parent, ListRightSideType predicateType)
        {
            Parent = parent;
            PredicateType = predicateType;
            Entity = new DataModelConditionListPredicateEntity();

            ApplyParentList();
            Initialize();
        }

        internal DataModelConditionListPredicate(DataModelConditionPart parent, DataModelConditionListPredicateEntity entity)
        {
            Parent = parent;
            Entity = entity;
            PredicateType = (ListRightSideType) entity.PredicateType;

            ApplyParentList();
            Initialize();
        }

        internal DataModelConditionListPredicateEntity Entity { get; set; }

        /// <summary>
        ///     Gets or sets the predicate type
        /// </summary>
        public ListRightSideType PredicateType { get; set; }

        /// <summary>
        ///     Gets the operator
        /// </summary>
        public ConditionOperator Operator { get; private set; }

        /// <summary>
        ///     Gets the data model condition list this predicate belongs to
        /// </summary>
        public DataModelConditionList DataModelConditionList { get; private set; }

        /// <summary>
        ///     Gets the path of the left property in the <see cref="ListType" />
        /// </summary>
        public string LeftPropertyPath { get; private set; }

        /// <summary>
        ///     Gets the currently used instance of the right side data model
        ///     <para>Note: This is null when using a path inside the list</para>
        /// </summary>
        public DataModel RightDataModel { get; set; }

        /// <summary>
        ///     Gets the path of the right property in the <see cref="ListType" />
        /// </summary>
        public string RightPropertyPath { get; private set; }

        /// <summary>
        ///     Gets the right static value, only used it <see cref="PredicateType" /> is
        ///     <see cref="ListRightSideType.Static" />
        /// </summary>
        public object RightStaticValue { get; private set; }

        public Func<object, object> LeftSideAccessor { get; set; }
        public Func<object, object> RightSideAccessor { get; set; }

        /// <summary>
        ///     Updates the left side of the predicate
        /// </summary>
        /// <param name="path">The path pointing to the left side value inside the list</param>
        public void UpdateLeftSide(string path)
        {
            if (DataModelConditionList.IsPrimitiveList)
                throw new ArtemisCoreException("Cannot apply a left side to a predicate inside a primitive list");
            if (!ListContainsInnerPath(path))
                throw new ArtemisCoreException($"List type {DataModelConditionList.ListType.Name} does not contain path {path}");

            LeftPropertyPath = path;

            ValidateOperator();
            ValidateRightSide();
            CreateExpression();
        }

        /// <summary>
        ///     Updates the right side of the predicate using a path to a value inside the list item, makes the predicate dynamic
        ///     and re-compiles the expression
        /// </summary>
        /// <param name="path">The path pointing to the right side value inside the list</param>
        public void UpdateRightSideDynamic(string path)
        {
            if (!ListContainsInnerPath(path))
                throw new ArtemisCoreException($"List type {DataModelConditionList.ListType.Name} does not contain path {path}");

            PredicateType = ListRightSideType.DynamicList;
            RightPropertyPath = path;

            CreateExpression();
        }

        /// <summary>
        ///     Updates the right side of the predicate using path to a value in a data model, makes the predicate dynamic and
        ///     re-compiles the expression
        /// </summary>
        /// <param name="dataModel"></param>
        /// <param name="path">The path pointing to the right side value inside the list</param>
        public void UpdateRightSideDynamic(DataModel dataModel, string path)
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

            PredicateType = ListRightSideType.Dynamic;
            RightDataModel = dataModel;
            RightPropertyPath = path;

            CreateExpression();
        }

        /// <summary>
        ///     Updates the right side of the predicate, makes the predicate static and re-compiles the expression
        /// </summary>
        /// <param name="staticValue">The right side value to use</param>
        public void UpdateRightSideStatic(object staticValue)
        {
            PredicateType = ListRightSideType.Static;
            RightPropertyPath = null;

            SetStaticValue(staticValue);
            CreateExpression();
        }

        /// <summary>
        ///     Updates the operator of the predicate and re-compiles the expression
        /// </summary>
        /// <param name="conditionOperator"></param>
        public void UpdateOperator(ConditionOperator conditionOperator)
        {
            if (conditionOperator == null)
            {
                Operator = null;
                return;
            }

            if (LeftPropertyPath == null)
            {
                Operator = conditionOperator;
                return;
            }

            var leftType = GetTypeAtInnerPath(LeftPropertyPath);
            if (conditionOperator.SupportsType(leftType))
                Operator = conditionOperator;

            CreateExpression();
        }

        /// <summary>
        ///     Not supported for list predicates, always returns <c>false</c>
        /// </summary>
        public override bool Evaluate()
        {
            return false;
        }

        /// <summary>
        ///     Determines whether the provided path is contained inside the list
        /// </summary>
        /// <param name="path">The path to evaluate</param>
        public bool ListContainsInnerPath(string path)
        {
            if (DataModelConditionList.ListType == null)
                return false;

            var parts = path.Split('.');
            var current = DataModelConditionList.ListType;
            foreach (var part in parts)
            {
                var property = current.GetProperty(part);
                current = property?.PropertyType;

                if (property == null)
                    return false;
            }

            return true;
        }
        
        internal override bool EvaluateObject(object target)
        {
            if (Operator == null || LeftSideAccessor == null || PredicateType != ListRightSideType.Static && RightSideAccessor == null)
                return false;

            // Compare with a static value
            if (PredicateType == ListRightSideType.Static)
            {
                if (!DataModelConditionList.ListType.IsValueType && RightStaticValue == null)
                    return false;

                return Operator.Evaluate(LeftSideAccessor(target), RightStaticValue);
            }

            // Compare with dynamic values
            if (PredicateType == ListRightSideType.Dynamic)
                return Operator.Evaluate(LeftSideAccessor(target), RightSideAccessor(RightDataModel));
            if (PredicateType == ListRightSideType.DynamicList)
                return Operator.Evaluate(LeftSideAccessor(target), RightSideAccessor(target));

            return false;
        }

        internal Type GetTypeAtInnerPath(string path)
        {
            if (!ListContainsInnerPath(path))
                return null;

            var parts = path.Split('.');
            var current = DataModelConditionList.ListType;

            Type result = null;
            foreach (var part in parts)
            {
                var property = current.GetProperty(part);
                current = property.PropertyType;
                result = property.PropertyType;
            }

            return result;
        }

        internal override void Save()
        {
            Entity.PredicateType = (int) PredicateType;
            Entity.LeftPropertyPath = LeftPropertyPath;

            Entity.RightDataModelGuid = RightDataModel?.PluginInfo?.Guid;
            Entity.RightPropertyPath = RightPropertyPath;
            Entity.RightStaticValue = JsonConvert.SerializeObject(RightStaticValue);

            if (Operator != null)
            {
                Entity.OperatorPluginGuid = Operator.PluginInfo.Guid;
                Entity.OperatorType = Operator.GetType().Name;
            }
        }

        internal override DataModelConditionPartEntity GetEntity()
        {
            return Entity;
        }

        private void ApplyParentList()
        {
            var current = Parent;

            while (current != null)
            {
                if (current is DataModelConditionList parentList)
                {
                    DataModelConditionList = parentList;

                    if (LeftPropertyPath != null && !ListContainsInnerPath(LeftPropertyPath))
                        LeftPropertyPath = null;
                    if (RightPropertyPath != null && !ListContainsInnerPath(RightPropertyPath))
                        RightPropertyPath = null;

                    return;
                }

                current = current.Parent;
            }

            if (DataModelConditionList == null)
                throw new ArtemisCoreException("This data model condition list predicate does not belong to a data model condition list");
        }

        private void Initialize()
        {
            DataModelStore.DataModelAdded += DataModelStoreOnDataModelAdded;
            DataModelStore.DataModelRemoved += DataModelStoreOnDataModelRemoved;
            ConditionOperatorStore.ConditionOperatorAdded += ConditionOperatorStoreOnConditionOperatorAdded;
            ConditionOperatorStore.ConditionOperatorRemoved += ConditionOperatorStoreOnConditionOperatorRemoved;

            // Left side
            if (Entity.LeftPropertyPath != null && ListContainsInnerPath(Entity.LeftPropertyPath))
                UpdateLeftSide(Entity.LeftPropertyPath);

            // Operator
            if (Entity.OperatorPluginGuid != null)
            {
                var conditionOperator = ConditionOperatorStore.Get(Entity.OperatorPluginGuid.Value, Entity.OperatorType)?.ConditionOperator;
                if (conditionOperator != null)
                    UpdateOperator(conditionOperator);
            }

            // Right side dynamic
            if (PredicateType == ListRightSideType.Dynamic && Entity.RightDataModelGuid != null && Entity.RightPropertyPath != null)
            {
                var dataModel = DataModelStore.Get(Entity.RightDataModelGuid.Value)?.DataModel;
                if (dataModel != null && dataModel.ContainsPath(Entity.RightPropertyPath))
                    UpdateRightSideDynamic(dataModel, Entity.RightPropertyPath);
            }
            // Right side dynamic inside the list
            else if (PredicateType == ListRightSideType.DynamicList && Entity.RightPropertyPath != null)
            {
                if (ListContainsInnerPath(Entity.RightPropertyPath))
                    UpdateRightSideDynamic(Entity.RightPropertyPath);
            }
            // Right side static
            else if (PredicateType == ListRightSideType.Static && Entity.RightStaticValue != null)
            {
                try
                {
                    if (LeftPropertyPath != null)
                    {
                        // Use the left side type so JSON.NET has a better idea what to do
                        var leftSideType = GetTypeAtInnerPath(LeftPropertyPath);
                        object rightSideValue;

                        try
                        {
                            rightSideValue = JsonConvert.DeserializeObject(Entity.RightStaticValue, leftSideType);
                        }
                        // If deserialization fails, use the type's default
                        catch (JsonSerializationException e)
                        {
                            DeserializationLogger.LogListPredicateDeserializationFailure(this, e);
                            rightSideValue = Activator.CreateInstance(leftSideType);
                        }

                        UpdateRightSideStatic(rightSideValue);
                    }
                    else
                    {
                        // Hope for the best... we must infer the type from JSON now
                        UpdateRightSideStatic(JsonConvert.DeserializeObject(Entity.RightStaticValue));
                    }
                }
                catch (JsonException e)
                {
                    DeserializationLogger.LogListPredicateDeserializationFailure(this, e);
                }
            }
        }

        private void CreateExpression()
        {
            if (Operator == null)
                return;

            // If the operator does not support a right side, create a static expression because the right side will simply be null
            if (PredicateType == ListRightSideType.DynamicList && Operator.SupportsRightSide)
                CreateDynamicListAccessors();
            else if (PredicateType == ListRightSideType.Dynamic && Operator.SupportsRightSide)
                CreateDynamicAccessors();
            else
                CreateStaticAccessors();
        }

        private void ValidateOperator()
        {
            if (LeftPropertyPath == null || Operator == null)
                return;

            var leftSideType = GetTypeAtInnerPath(LeftPropertyPath);
            if (!Operator.SupportsType(leftSideType))
                Operator = null;
        }

        private void ValidateRightSide()
        {
            var leftSideType = GetTypeAtInnerPath(LeftPropertyPath);
            if (PredicateType == ListRightSideType.Dynamic)
            {
                if (RightDataModel == null)
                    return;

                var rightSideType = RightDataModel.GetTypeAtPath(RightPropertyPath);
                if (!leftSideType.IsCastableFrom(rightSideType))
                    UpdateRightSideDynamic(null, null);
            }
            else if (PredicateType == ListRightSideType.DynamicList)
            {
                if (RightPropertyPath == null)
                    return;

                var rightSideType = GetTypeAtInnerPath(RightPropertyPath);
                if (!leftSideType.IsCastableFrom(rightSideType))
                    UpdateRightSideDynamic(null);
            }
            else
            {
                if (RightStaticValue != null && leftSideType.IsCastableFrom(RightStaticValue.GetType()))
                    UpdateRightSideStatic(RightStaticValue);
                else
                    UpdateRightSideStatic(null);
            }
        }

        private void SetStaticValue(object staticValue)
        {
            // If the left side is empty simply apply the value, any validation will wait
            if (LeftPropertyPath == null)
            {
                RightStaticValue = staticValue;
                return;
            }

            var leftSideType = GetTypeAtInnerPath(LeftPropertyPath);

            // If not null ensure the types match and if not, convert it
            if (staticValue != null && staticValue.GetType() == leftSideType)
                RightStaticValue = staticValue;
            else if (staticValue != null)
                RightStaticValue = Convert.ChangeType(staticValue, leftSideType);
            // If null create a default instance for value types or simply make it null for reference types
            else if (leftSideType.IsValueType)
                RightStaticValue = Activator.CreateInstance(leftSideType);
            else
                RightStaticValue = null;
        }

        private void CreateDynamicListAccessors()
        {
            if (LeftPropertyPath == null || RightPropertyPath == null || Operator == null)
                return;

            // List accessors share the same parameter because a list always contains one item per entry
            var leftSideParameter = Expression.Parameter(typeof(object), "listItem");
            var leftSideAccessor = CreateListAccessor(LeftPropertyPath, leftSideParameter);
            var rightSideAccessor = CreateListAccessor(RightPropertyPath, leftSideParameter);

            // A conversion may be required if the types differ
            // This can cause issues if the DataModelConditionOperator wasn't accurate in it's supported types but that is not a concern here
            if (rightSideAccessor.Type != leftSideAccessor.Type)
                rightSideAccessor = Expression.Convert(rightSideAccessor, leftSideAccessor.Type);

            LeftSideAccessor = Expression.Lambda<Func<object, object>>(leftSideAccessor, leftSideParameter).Compile();
            RightSideAccessor = Expression.Lambda<Func<object, object>>(rightSideAccessor, leftSideParameter).Compile();
        }

        private void CreateDynamicAccessors()
        {
            if (LeftPropertyPath == null || RightPropertyPath == null || RightDataModel == null || Operator == null)
                return;

            // List accessors share the same parameter because a list always contains one item per entry
            var leftSideParameter = Expression.Parameter(typeof(object), "listItem");
            var leftSideAccessor = CreateListAccessor(LeftPropertyPath, leftSideParameter);
            var rightSideAccessor = ExpressionUtilities.CreateDataModelAccessor(RightDataModel, RightPropertyPath, "right", out var rightSideParameter);

            // A conversion may be required if the types differ
            // This can cause issues if the DataModelConditionOperator wasn't accurate in it's supported types but that is not a concern here
            if (rightSideAccessor.Type != leftSideAccessor.Type)
                rightSideAccessor = Expression.Convert(rightSideAccessor, leftSideAccessor.Type);

            LeftSideAccessor = Expression.Lambda<Func<object, object>>(leftSideAccessor, leftSideParameter).Compile();
            RightSideAccessor = Expression.Lambda<Func<object, object>>(rightSideAccessor, rightSideParameter).Compile();
        }

        private void CreateStaticAccessors()
        {
            if (!DataModelConditionList.IsPrimitiveList && LeftPropertyPath == null || Operator == null)
                return;

            // List accessors share the same parameter because a list always contains one item per entry
            var leftSideParameter = Expression.Parameter(typeof(object), "listItem");
            var leftSideAccessor = DataModelConditionList.IsPrimitiveList
                ? Expression.Convert(leftSideParameter, DataModelConditionList.ListType)
                : CreateListAccessor(LeftPropertyPath, leftSideParameter);

            LeftSideAccessor = Expression.Lambda<Func<object, object>>(leftSideAccessor, leftSideParameter).Compile();
            RightSideAccessor = null;
        }

        private Expression CreateListAccessor(string path, ParameterExpression listParameter)
        {
            // Create an expression that checks every part of the path for null
            // In the same iteration, create the accessor
            Expression source = Expression.Convert(listParameter, DataModelConditionList.ListType);
            return ExpressionUtilities.CreateNullCheckedAccessor(source, path);
        }

        #region IDisposable

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            DataModelStore.DataModelAdded -= DataModelStoreOnDataModelAdded;
            DataModelStore.DataModelRemoved -= DataModelStoreOnDataModelRemoved;
            ConditionOperatorStore.ConditionOperatorAdded -= ConditionOperatorStoreOnConditionOperatorAdded;
            ConditionOperatorStore.ConditionOperatorRemoved -= ConditionOperatorStoreOnConditionOperatorRemoved;

            base.Dispose(disposing);
        }

        #endregion

        #region Event handlers

        private void DataModelStoreOnDataModelAdded(object sender, DataModelStoreEvent e)
        {
            var dataModel = e.Registration.DataModel;
            if (dataModel.PluginInfo.Guid == Entity.RightDataModelGuid && dataModel.ContainsPath(Entity.RightPropertyPath))
                UpdateRightSideDynamic(dataModel, Entity.RightPropertyPath);
        }

        private void DataModelStoreOnDataModelRemoved(object sender, DataModelStoreEvent e)
        {
            if (RightDataModel == e.Registration.DataModel)
            {
                RightSideAccessor = null;
                RightDataModel = null;
            }
        }

        private void ConditionOperatorStoreOnConditionOperatorAdded(object sender, ConditionOperatorStoreEvent e)
        {
            var conditionOperator = e.Registration.ConditionOperator;
            if (Entity.OperatorPluginGuid == conditionOperator.PluginInfo.Guid && Entity.OperatorType == conditionOperator.GetType().Name)
                UpdateOperator(conditionOperator);
        }

        private void ConditionOperatorStoreOnConditionOperatorRemoved(object sender, ConditionOperatorStoreEvent e)
        {
            if (e.Registration.ConditionOperator != Operator)
                return;
            Operator = null;
        }

        #endregion
    }

    /// <summary>
    ///     An enum defining the right side type of a profile entity
    /// </summary>
    public enum ListRightSideType
    {
        /// <summary>
        ///     A static right side value
        /// </summary>
        Static,

        /// <summary>
        ///     A dynamic right side value based on a path in a data model
        /// </summary>
        Dynamic,

        /// <summary>
        ///     A dynamic right side value based on a path in the list
        /// </summary>
        DynamicList
    }
}
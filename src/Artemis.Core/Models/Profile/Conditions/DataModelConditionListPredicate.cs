using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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

            DataModelConditionList = null!;
            ApplyParentList();
            Initialize();
        }

        internal DataModelConditionListPredicate(DataModelConditionPart parent, DataModelConditionListPredicateEntity entity)
        {
            Parent = parent;
            Entity = entity;
            PredicateType = (ListRightSideType) entity.PredicateType;

            DataModelConditionList = null!;
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
        public ConditionOperator? Operator { get; private set; }

        /// <summary>
        ///     Gets the data model condition list this predicate belongs to
        /// </summary>
        public DataModelConditionList DataModelConditionList { get; private set; }

        public DataModelPath? LeftPath { get; set; }

        public DataModelPath? RightPath { get; set; }

        /// <summary>
        ///     Gets the right static value, only used it <see cref="PredicateType" /> is
        ///     <see cref="ListRightSideType.Static" />
        /// </summary>
        public object? RightStaticValue { get; private set; }

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

            LeftPath?.Dispose();
            LeftPath = new DataModelPath(new ListPredicateWrapperDataModel(), path);

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
            RightPath?.Dispose();
            RightPath = new DataModelPath(new ListPredicateWrapperDataModel(), path);

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
            RightPath?.Dispose();
            RightPath = new DataModelPath(dataModel, path);

            CreateExpression();
        }

        /// <summary>
        ///     Updates the right side of the predicate, makes the predicate static and re-compiles the expression
        /// </summary>
        /// <param name="staticValue">The right side value to use</param>
        public void UpdateRightSideStatic(object staticValue)
        {
            PredicateType = ListRightSideType.Static;
            RightPath?.Dispose();
            RightPath = null;

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

            // No need to clear compiled expressions, without a left data model they are already null
            if (LeftPath == null || !LeftPath.IsValid)
            {
                Operator = conditionOperator;
                return;
            }

            Type leftType = LeftPath.GetPropertyType()!;
            if (!conditionOperator.SupportsType(leftType))
                throw new ArtemisCoreException($"Cannot apply operator {conditionOperator.GetType().Name} to this predicate because " +
                                               $"it does not support left side type {leftType.Name}");

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

            string[] parts = path.Split('.');
            Type current = DataModelConditionList.ListType;
            foreach (string part in parts)
            {
                PropertyInfo? property = current.GetProperty(part);
                current = property?.PropertyType;

                if (property == null)
                    return false;
            }

            return true;
        }

        internal override bool EvaluateObject(object target)
        {
            if (Operator == null || LeftPath == null || !LeftPath.IsValid)
                return false;

            // Compare with a static value
            if (PredicateType == ListRightSideType.Static)
            {
                object? leftSideValue = GetListPathValue(LeftPath, target);
                if (leftSideValue != null && leftSideValue.GetType().IsValueType && RightStaticValue == null)
                    return false;

                return Operator.Evaluate(leftSideValue, RightStaticValue);
            }

            if (RightPath == null || !RightPath.IsValid)
                return false;

            // Compare with dynamic values
            if (PredicateType == ListRightSideType.Dynamic)
                return Operator.Evaluate(GetListPathValue(LeftPath, target), RightPath.GetValue());
            if (PredicateType == ListRightSideType.DynamicList)
                return Operator.Evaluate(GetListPathValue(LeftPath, target), GetListPathValue(RightPath, target));

            return false;
        }

        internal Type GetTypeAtInnerPath(string path)
        {
            if (!ListContainsInnerPath(path))
                return null;

            string[] parts = path.Split('.');
            Type current = DataModelConditionList.ListType;

            Type result = null;
            foreach (string part in parts)
            {
                PropertyInfo? property = current.GetProperty(part);
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
            DataModelConditionPart current = Parent;

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
                ConditionOperator conditionOperator = ConditionOperatorStore.Get(Entity.OperatorPluginGuid.Value, Entity.OperatorType)?.ConditionOperator;
                if (conditionOperator != null)
                    UpdateOperator(conditionOperator);
            }

            // Right side dynamic
            if (PredicateType == ListRightSideType.Dynamic && Entity.RightDataModelGuid != null && Entity.RightPropertyPath != null)
            {
                DataModel dataModel = DataModelStore.Get(Entity.RightDataModelGuid.Value)?.DataModel;
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
                        Type leftSideType = GetTypeAtInnerPath(LeftPropertyPath);
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

            Type leftSideType = GetTypeAtInnerPath(LeftPropertyPath);
            if (!Operator.SupportsType(leftSideType))
                Operator = null;
        }

        private void ValidateRightSide()
        {
            Type leftSideType = GetTypeAtInnerPath(LeftPropertyPath);
            if (PredicateType == ListRightSideType.Dynamic)
            {
                if (RightDataModel == null)
                    return;

                Type rightSideType = RightDataModel.GetTypeAtPath(RightPropertyPath);
                if (!leftSideType.IsCastableFrom(rightSideType))
                    UpdateRightSideDynamic(null, null);
            }
            else if (PredicateType == ListRightSideType.DynamicList)
            {
                if (RightPropertyPath == null)
                    return;

                Type rightSideType = GetTypeAtInnerPath(RightPropertyPath);
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

            Type leftSideType = GetTypeAtInnerPath(LeftPropertyPath);

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

        private object? GetListPathValue(DataModelPath path, object target)
        {
            if (!(path.Target is ListPredicateWrapperDataModel wrapper))
                throw new ArtemisCoreException("Data model condition list predicate has a path with an invalid target");

            wrapper.Value = target;
            return path.GetValue();
        }

        private void CreateDynamicListAccessors()
        {
            if (LeftPropertyPath == null || RightPropertyPath == null || Operator == null)
                return;

            // List accessors share the same parameter because a list always contains one item per entry
            ParameterExpression leftSideParameter = Expression.Parameter(typeof(object), "listItem");
            Expression leftSideAccessor = CreateListAccessor(LeftPropertyPath, leftSideParameter);
            Expression rightSideAccessor = CreateListAccessor(RightPropertyPath, leftSideParameter);

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
            ParameterExpression leftSideParameter = Expression.Parameter(typeof(object), "listItem");
            Expression leftSideAccessor = CreateListAccessor(LeftPropertyPath, leftSideParameter);
            Expression rightSideAccessor = ExpressionUtilities.CreateDataModelAccessor(RightDataModel, RightPropertyPath, "right", out ParameterExpression rightSideParameter);

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
            ParameterExpression leftSideParameter = Expression.Parameter(typeof(object), "listItem");
            Expression leftSideAccessor = DataModelConditionList.IsPrimitiveList
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

            LeftPath?.Dispose();
            RightPath?.Dispose();

            base.Dispose(disposing);
        }

        #endregion

        #region Event handlers

        private void DataModelStoreOnDataModelAdded(object sender, DataModelStoreEvent e)
        {
            DataModel dataModel = e.Registration.DataModel;
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
            ConditionOperator conditionOperator = e.Registration.ConditionOperator;
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
using System;
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

        /// <summary>
        ///     Gets the path of the left property
        /// </summary>
        public DataModelPath? LeftPath { get; private set; }

        /// <summary>
        ///     Gets the path of the right property
        /// </summary>
        public DataModelPath? RightPath { get; private set; }

        /// <summary>
        ///     Gets the right static value, only used it <see cref="PredicateType" /> is
        ///     <see cref="ListRightSideType.Static" />
        /// </summary>
        public object? RightStaticValue { get; private set; }

        internal DataModelConditionListPredicateEntity Entity { get; set; }

        /// <summary>
        ///     Updates the left side of the predicate
        /// </summary>
        /// <param name="path">The path pointing to the left side value inside the list</param>
        public void UpdateLeftSide(DataModelPath? path)
        {
            if (DataModelConditionList.IsPrimitiveList)
                throw new ArtemisCoreException("Cannot apply a left side to a predicate inside a primitive list");

            if (path != null && !path.IsValid)
                throw new ArtemisCoreException("Cannot update left side of predicate to an invalid path");

            LeftPath?.Dispose();
            LeftPath = path != null ? new DataModelPath(path) : null;

            ValidateOperator();
            ValidateRightSide();
        }

        /// <summary>
        ///     Updates the right side of the predicate using a path to a value inside the list item, makes the predicate dynamic
        ///     and re-compiles the expression
        /// </summary>
        /// <param name="path">The path pointing to the right side value inside the list</param>
        public void UpdateRightSideDynamicList(DataModelPath? path)
        {
            if (path != null && !path.IsValid)
                throw new ArtemisCoreException("Cannot update right side of predicate to an invalid path");

            RightPath?.Dispose();
            RightPath = path != null ? new DataModelPath(path) : null;

            PredicateType = ListRightSideType.DynamicList;
        }

        /// <summary>
        ///     Updates the right side of the predicate using path to a value in a data model, makes the predicate dynamic and
        ///     re-compiles the expression
        /// </summary>
        /// <param name="path">The path pointing to the right side value inside the list</param>
        public void UpdateRightSideDynamic(DataModelPath? path)
        {
            RightPath?.Dispose();
            RightPath = path;

            PredicateType = ListRightSideType.Dynamic;
        }

        /// <summary>
        ///     Updates the right side of the predicate, makes the predicate static and re-compiles the expression
        /// </summary>
        /// <param name="staticValue">The right side value to use</param>
        public void UpdateRightSideStatic(object? staticValue)
        {
            PredicateType = ListRightSideType.Static;
            RightPath?.Dispose();
            RightPath = null;

            SetStaticValue(staticValue);
        }

        /// <summary>
        ///     Updates the operator of the predicate and re-compiles the expression
        /// </summary>
        /// <param name="conditionOperator"></param>
        public void UpdateOperator(ConditionOperator? conditionOperator)
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

        #region IDisposable

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            ConditionOperatorStore.ConditionOperatorAdded -= ConditionOperatorStoreOnConditionOperatorAdded;
            ConditionOperatorStore.ConditionOperatorRemoved -= ConditionOperatorStoreOnConditionOperatorRemoved;

            LeftPath?.Dispose();
            RightPath?.Dispose();

            base.Dispose(disposing);
        }

        #endregion

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

            LeftPath?.Save();
            Entity.LeftPath = LeftPath?.Entity;
            RightPath?.Save();
            Entity.RightPath = RightPath?.Entity;

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
            DataModelConditionPart? current = Parent;
            while (current != null)
            {
                if (current is DataModelConditionList parentList)
                {
                    DataModelConditionList = parentList;
                    return;
                }

                current = current.Parent;
            }

            if (DataModelConditionList == null)
                throw new ArtemisCoreException("This data model condition list predicate does not belong to a data model condition list");
        }

        private void Initialize()
        {
            ConditionOperatorStore.ConditionOperatorAdded += ConditionOperatorStoreOnConditionOperatorAdded;
            ConditionOperatorStore.ConditionOperatorRemoved += ConditionOperatorStoreOnConditionOperatorRemoved;

            // Left side
            if (Entity.LeftPath != null)
            {
                LeftPath = DataModelConditionList.ListType != null
                    ? new DataModelPath(ListPredicateWrapperDataModel.Create(DataModelConditionList.ListType), Entity.LeftPath)
                    : null;
            }

            // Operator
            if (Entity.OperatorPluginGuid != null)
            {
                ConditionOperator? conditionOperator = ConditionOperatorStore.Get(Entity.OperatorPluginGuid.Value, Entity.OperatorType)?.ConditionOperator;
                if (conditionOperator != null)
                    UpdateOperator(conditionOperator);
            }

            // Right side dynamic
            if (PredicateType == ListRightSideType.Dynamic && Entity.RightPath != null)
                RightPath = new DataModelPath(null, Entity.RightPath);
            // Right side dynamic inside the list
            else if (PredicateType == ListRightSideType.DynamicList && Entity.RightPath != null)
            {
                RightPath = DataModelConditionList.ListType != null
                    ? new DataModelPath(ListPredicateWrapperDataModel.Create(DataModelConditionList.ListType), Entity.RightPath)
                    : null;
            }
            // Right side static
            else if (PredicateType == ListRightSideType.Static && Entity.RightStaticValue != null)
                try
                {
                    if (LeftPath != null && LeftPath.IsValid)
                    {
                        // Use the left side type so JSON.NET has a better idea what to do
                        Type leftSideType = LeftPath.GetPropertyType()!;
                        object? rightSideValue;

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

        private void ValidateOperator()
        {
            if (LeftPath == null || !LeftPath.IsValid || Operator == null)
                return;

            Type leftType = LeftPath.GetPropertyType()!;
            if (!Operator.SupportsType(leftType))
                Operator = null;
        }

        private void ValidateRightSide()
        {
            Type? leftType = LeftPath?.GetPropertyType();
            if (PredicateType == ListRightSideType.Dynamic)
            {
                if (RightPath == null || !RightPath.IsValid)
                    return;

                Type rightSideType = RightPath.GetPropertyType()!;
                if (leftType != null && !leftType.IsCastableFrom(rightSideType))
                    UpdateRightSideDynamic(null);
            }
            else if (PredicateType == ListRightSideType.DynamicList)
            {
                if (RightPath == null || !RightPath.IsValid)
                    return;

                Type rightSideType = RightPath.GetPropertyType()!;
                if (leftType != null && !leftType.IsCastableFrom(rightSideType))
                    UpdateRightSideDynamicList(null);
            }
            else
            {
                if (RightStaticValue != null && (leftType == null || leftType.IsCastableFrom(RightStaticValue.GetType())))
                    UpdateRightSideStatic(RightStaticValue);
                else
                    UpdateRightSideStatic(null);
            }
        }

        private void SetStaticValue(object? staticValue)
        {
            RightPath?.Dispose();
            RightPath = null;

            // If the left side is empty simply apply the value, any validation will wait
            if (LeftPath == null || !LeftPath.IsValid)
            {
                RightStaticValue = staticValue;
                return;
            }

            // If the left path is valid we can expect a type
            Type leftSideType = LeftPath.GetPropertyType()!;

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

            wrapper.UntypedValue = target;
            return path.GetValue();
        }

        #region Event handlers

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
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
        public DataModelConditionListPredicate(DataModelConditionPart parent, ProfileRightSideType predicateType)
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
            PredicateType = (ProfileRightSideType) entity.PredicateType;

            DataModelConditionList = null!;
            ApplyParentList();
            Initialize();
        }

        /// <summary>
        ///     Gets or sets the predicate type
        /// </summary>
        public ProfileRightSideType PredicateType { get; set; }

        /// <summary>
        ///     Gets the operator
        /// </summary>
        public BaseConditionOperator? Operator { get; private set; }

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
        ///     <see cref="ProfileRightSideType.Static" />
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
        ///     Updates the right side of the predicate using a path and makes the predicate dynamic
        /// </summary>
        /// <param name="path">The path pointing to the right side value</param>
        public void UpdateRightSideDynamic(DataModelPath? path)
        {
            if (path != null && !path.IsValid)
                throw new ArtemisCoreException("Cannot update right side of predicate to an invalid path");
            if (Operator != null && path != null && !Operator.SupportsType(path.GetPropertyType()!, ConditionParameterSide.Right))
                throw new ArtemisCoreException($"Selected operator does not support right side of type {path.GetPropertyType()!.Name}");

            RightPath?.Dispose();
            RightPath = path != null ? new DataModelPath(path) : null;

            PredicateType = ProfileRightSideType.Dynamic;
        }

        /// <summary>
        ///     Updates the right side of the predicate, makes the predicate static and re-compiles the expression
        /// </summary>
        /// <param name="staticValue">The right side value to use</param>
        public void UpdateRightSideStatic(object? staticValue)
        {
            PredicateType = ProfileRightSideType.Static;
            RightPath?.Dispose();
            RightPath = null;

            SetStaticValue(staticValue);
        }

        /// <summary>
        ///     Updates the operator of the predicate and re-compiles the expression
        /// </summary>
        /// <param name="conditionOperator"></param>
        public void UpdateOperator(BaseConditionOperator? conditionOperator)
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
            // Left side can't go empty so enforce a match
            if (!conditionOperator.SupportsType(leftType, ConditionParameterSide.Left))
                throw new ArtemisCoreException($"Cannot apply operator {conditionOperator.GetType().Name} to this predicate because " +
                                               $"it does not support left side type {leftType.Name}");

            Operator = conditionOperator;
            ValidateRightSide();
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
            if (PredicateType == ProfileRightSideType.Static)
            {
                object? leftSideValue = GetListPathValue(LeftPath, target);
                if (leftSideValue != null && leftSideValue.GetType().IsValueType && RightStaticValue == null)
                    return false;

                return Operator.InternalEvaluate(leftSideValue, RightStaticValue);
            }

            if (RightPath == null || !RightPath.IsValid)
                return false;

            // Compare with dynamic values
            if (PredicateType == ProfileRightSideType.Dynamic)
            {
                // If the path targets a property inside the list, evaluate on the list path value instead of the right path value
                if (RightPath.Target is ListPredicateWrapperDataModel)
                    return Operator.InternalEvaluate(GetListPathValue(LeftPath, target), GetListPathValue(RightPath, target));
                return Operator.InternalEvaluate(GetListPathValue(LeftPath, target), RightPath.GetValue());
            }

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
                BaseConditionOperator? conditionOperator = ConditionOperatorStore.Get(Entity.OperatorPluginGuid.Value, Entity.OperatorType)?.ConditionOperator;
                if (conditionOperator != null)
                    UpdateOperator(conditionOperator);
            }

            // Right side dynamic
            if (PredicateType == ProfileRightSideType.Dynamic && Entity.RightPath != null)
            {
                // Right side dynamic inside the list
                // TODO: Come up with a general wrapper solution because this will clash with events
                if (Entity.RightPath.DataModelGuid == Constants.CorePluginInfo.Guid)
                {
                    RightPath = DataModelConditionList.ListType != null
                        ? new DataModelPath(ListPredicateWrapperDataModel.Create(DataModelConditionList.ListType), Entity.RightPath)
                        : null;
                }
                // Right side dynamic
                else
                    RightPath = new DataModelPath(null, Entity.RightPath);
            }
            // Right side static
            else if (PredicateType == ProfileRightSideType.Static && Entity.RightStaticValue != null)
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
            if (!Operator.SupportsType(leftType, ConditionParameterSide.Left))
                Operator = null;
        }

        private void ValidateRightSide()
        {
            if (Operator == null)
                return;

            if (PredicateType == ProfileRightSideType.Dynamic)
            {
                if (RightPath == null || !RightPath.IsValid)
                    return;

                Type rightSideType = RightPath.GetPropertyType()!;
                if (!Operator.SupportsType(rightSideType, ConditionParameterSide.Right))
                    UpdateRightSideDynamic(null);
            }
            else
            {
                if (RightStaticValue == null)
                    return;

                if (!Operator.SupportsType(RightStaticValue.GetType(), ConditionParameterSide.Right))
                    UpdateRightSideStatic(null);
            }
        }

        private void SetStaticValue(object? staticValue)
        {
            RightPath?.Dispose();
            RightPath = null;

            // If the operator is null simply apply the value, any validation will wait
            if (Operator == null)
            {
                RightStaticValue = staticValue;
                return;
            }
            // If the operator does not support a right side, always set it to null
            if (Operator.RightSideType == null)
            {
                RightStaticValue = null;
                return;
            }

            // If not null ensure the types match and if not, convert it
            if (staticValue != null && staticValue.GetType() == Operator.RightSideType)
                RightStaticValue = staticValue;
            else if (staticValue != null)
                RightStaticValue = Convert.ChangeType(staticValue, Operator.RightSideType);
            // If null create a default instance for value types or simply make it null for reference types
            else if (Operator.RightSideType.IsValueType)
                RightStaticValue = Activator.CreateInstance(Operator.RightSideType);
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
            BaseConditionOperator conditionOperator = e.Registration.ConditionOperator;
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
}
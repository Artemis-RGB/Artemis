using System;
using Artemis.Storage.Entities.Profile.Abstract;
using Artemis.Storage.Entities.Profile.Conditions;
using Newtonsoft.Json;

namespace Artemis.Core
{
    /// <summary>
    ///     A predicate in a data model condition using either two data model values or one data model value and a
    ///     static value
    /// </summary>
    public abstract class DataModelConditionPredicate : DataModelConditionPart
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="DataModelConditionPredicate" /> class
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="predicateType"></param>
        /// <param name="entity">A new empty entity</param>
        protected DataModelConditionPredicate(DataModelConditionPart parent, ProfileRightSideType predicateType, DataModelConditionPredicateEntity entity)
        {
            Parent = parent;
            Entity = entity;
            PredicateType = predicateType;
        }

        internal DataModelConditionPredicate(DataModelConditionPart parent, DataModelConditionPredicateEntity entity)
        {
            Parent = parent;
            Entity = entity;
            PredicateType = (ProfileRightSideType) entity.PredicateType;
        }

        /// <summary>
        ///     Gets or sets the predicate type
        /// </summary>
        public ProfileRightSideType PredicateType { get; set; }

        /// <summary>
        ///     Gets the operator
        /// </summary>
        public BaseConditionOperator? Operator { get; protected set; }

        /// <summary>
        ///     Gets the path of the left property
        /// </summary>
        public DataModelPath? LeftPath { get; protected set; }

        /// <summary>
        ///     Gets the path of the right property
        /// </summary>
        public DataModelPath? RightPath { get; protected set; }

        /// <summary>
        ///     Gets the right static value, only used it <see cref="PredicateType" /> is
        ///     <see cref="ProfileRightSideType.Static" />
        /// </summary>
        public object? RightStaticValue { get; protected set; }

        internal DataModelConditionPredicateEntity Entity { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            if (PredicateType == ProfileRightSideType.Dynamic)
                return $"[Dynamic] {LeftPath} {Operator?.Description} {RightPath}";
            return $"[Static] {LeftPath} {Operator?.Description} {RightStaticValue}";
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

        #region Initialization

        internal void Initialize()
        {
            ConditionOperatorStore.ConditionOperatorAdded += ConditionOperatorStoreOnConditionOperatorAdded;
            ConditionOperatorStore.ConditionOperatorRemoved += ConditionOperatorStoreOnConditionOperatorRemoved;

            InitializeLeftPath();

            // Operator
            if (Entity.OperatorPluginGuid != null)
            {
                BaseConditionOperator? conditionOperator = ConditionOperatorStore.Get(Entity.OperatorPluginGuid.Value, Entity.OperatorType)?.ConditionOperator;
                if (conditionOperator != null)
                    UpdateOperator(conditionOperator);
            }

            // Right side dynamic
            if (PredicateType == ProfileRightSideType.Dynamic)
                InitializeRightPath();
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
                            rightSideValue = JsonConvert.DeserializeObject(Entity.RightStaticValue, leftSideType, Constants.JsonConvertSettings);
                        }
                        // If deserialization fails, use the type's default
                        catch (JsonSerializationException e)
                        {
                            DeserializationLogger.LogPredicateDeserializationFailure(this, e);
                            rightSideValue = Activator.CreateInstance(leftSideType);
                        }

                        UpdateRightSideStatic(rightSideValue);
                    }
                    else
                    {
                        // Hope for the best...
                        UpdateRightSideStatic(JsonConvert.DeserializeObject(Entity.RightStaticValue, Constants.JsonConvertSettings));
                    }
                }
                catch (JsonReaderException e)
                {
                    DeserializationLogger.LogPredicateDeserializationFailure(this, e);
                }
        }

        /// <summary>
        ///     Initializes the left path of this condition predicate
        /// </summary>
        protected abstract void InitializeLeftPath();

        /// <summary>
        ///     Initializes the right path of this condition predicate
        /// </summary>
        protected abstract void InitializeRightPath();

        #endregion

        #region Modification

        /// <summary>
        ///     Updates the left side of the predicate
        /// </summary>
        /// <param name="path">The path pointing to the left side value inside the data model</param>
        public virtual void UpdateLeftSide(DataModelPath? path)
        {
            if (path != null && !path.IsValid)
                throw new ArtemisCoreException("Cannot update left side of predicate to an invalid path");

            LeftPath?.Dispose();
            LeftPath = path != null ? new DataModelPath(path) : null;

            ValidateOperator();
            ValidateRightSide();
        }

        /// <summary>
        ///     Updates the right side of the predicate, makes the predicate dynamic and re-compiles the expression
        /// </summary>
        /// <param name="path">The path pointing to the right side value inside the data model</param>
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
            Type? preferredType = GetPreferredRightSideType();
            if (staticValue != null && staticValue.GetType() == preferredType || preferredType == null)
                RightStaticValue = staticValue;
            else if (staticValue != null)
                RightStaticValue = Convert.ChangeType(staticValue, preferredType);
            // If null create a default instance for value types or simply make it null for reference types
            else if (preferredType.IsValueType)
                RightStaticValue = Activator.CreateInstance(preferredType);
            else
                RightStaticValue = null;
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

            // No need to check for compatibility without a left side, when left site does get set it will be checked again
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
        ///     Determines the best type to use for the right side op this predicate
        /// </summary>
        public abstract Type? GetPreferredRightSideType();

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

        #endregion

        #region Evaluation

        /// <inheritdoc />
        public override bool Evaluate()
        {
            if (Operator == null || LeftPath == null || !LeftPath.IsValid)
                return false;

            // If the operator does not support a right side, immediately evaluate with null
            if (Operator.RightSideType == null)
                return Operator.InternalEvaluate(LeftPath.GetValue(), null);

            // Compare with a static value
            if (PredicateType == ProfileRightSideType.Static)
            {
                object? leftSideValue = LeftPath.GetValue();
                if (leftSideValue != null && leftSideValue.GetType().IsValueType && RightStaticValue == null)
                    return false;

                return Operator.InternalEvaluate(leftSideValue, RightStaticValue);
            }

            if (RightPath == null || !RightPath.IsValid)
                return false;

            // Compare with dynamic values
            return Operator.InternalEvaluate(LeftPath.GetValue(), RightPath.GetValue());
        }

        /// <inheritdoc />
        internal override bool EvaluateObject(object? target)
        {
            return false;
        }

        #endregion

        #region Storage

        internal override DataModelConditionPartEntity GetEntity()
        {
            return Entity;
        }

        internal override void Save()
        {
            // Don't save an invalid state
            if (LeftPath != null && !LeftPath.IsValid || RightPath != null && !RightPath.IsValid)
                return;

            Entity.PredicateType = (int) PredicateType;

            LeftPath?.Save();
            Entity.LeftPath = LeftPath?.Entity;
            RightPath?.Save();
            Entity.RightPath = RightPath?.Entity;

            Entity.RightStaticValue = JsonConvert.SerializeObject(RightStaticValue, Constants.JsonConvertSettings);

            if (Operator?.Plugin != null)
            {
                Entity.OperatorPluginGuid = Operator.Plugin.Guid;
                Entity.OperatorType = Operator.GetType().Name;
            }
        }

        #endregion

        #region Event handlers

        private void ConditionOperatorStoreOnConditionOperatorAdded(object? sender, ConditionOperatorStoreEvent e)
        {
            BaseConditionOperator conditionOperator = e.Registration.ConditionOperator;
            if (Entity.OperatorPluginGuid == conditionOperator.Plugin!.Guid && Entity.OperatorType == conditionOperator.GetType().Name)
                UpdateOperator(conditionOperator);
        }

        private void ConditionOperatorStoreOnConditionOperatorRemoved(object? sender, ConditionOperatorStoreEvent e)
        {
            if (e.Registration.ConditionOperator != Operator)
                return;

            Operator = null;
        }

        #endregion
    }
}
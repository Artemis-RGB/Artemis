using System;
using Artemis.Storage.Entities.Profile;
using Artemis.Storage.Entities.Profile.Conditions;

namespace Artemis.Core
{
    /// <summary>
    ///     A predicate like evaluated inside a <see cref="DataModelConditionEvent" />
    /// </summary>
    public class DataModelConditionEventPredicate : DataModelConditionPredicate
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="DataModelConditionEventPredicate" /> class
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="predicateType"></param>
        public DataModelConditionEventPredicate(DataModelConditionPart parent, ProfileRightSideType predicateType)
            : base(parent, predicateType, new DataModelConditionEventPredicateEntity())
        {
            DataModelConditionEvent = null!;
            ApplyParentEvent();
            Initialize();
        }

        internal DataModelConditionEventPredicate(DataModelConditionPart parent, DataModelConditionEventPredicateEntity entity)
            : base(parent, entity)
        {
            DataModelConditionEvent = null!;
            ApplyParentEvent();
            Initialize();
        }

        /// <summary>
        ///     Gets the data model condition event this predicate belongs to
        /// </summary>
        public DataModelConditionEvent DataModelConditionEvent { get; private set; }

        private void ApplyParentEvent()
        {
            DataModelConditionPart? current = Parent;
            while (current != null)
            {
                if (current is DataModelConditionEvent parentEvent)
                {
                    DataModelConditionEvent = parentEvent;
                    return;
                }

                current = current.Parent;
            }

            if (DataModelConditionEvent == null)
                throw new ArtemisCoreException("This data model condition event predicate does not belong to a data model condition event");
        }

        private object? GetEventPathValue(DataModelPath path, object target)
        {
            if (!(path.Target is EventPredicateWrapperDataModel wrapper))
                throw new ArtemisCoreException("Data model condition event predicate has a path with an invalid target");

            wrapper.UntypedArguments = target;
            return path.GetValue();
        }

        #region Initialization

        protected override void InitializeLeftPath()
        {
            if (Entity.LeftPath != null)
                LeftPath = DataModelConditionEvent.EventArgumentType != null
                    ? new DataModelPath(EventPredicateWrapperDataModel.Create(DataModelConditionEvent.EventArgumentType), Entity.LeftPath)
                    : null;
        }

        protected override void InitializeRightPath()
        {
            if (PredicateType == ProfileRightSideType.Dynamic && Entity.RightPath != null)
            {
                // Right side dynamic using event arguments
                if (Entity.RightPath.WrapperType == PathWrapperType.Event)
                {
                    RightPath = DataModelConditionEvent.EventArgumentType != null
                        ? new DataModelPath(EventPredicateWrapperDataModel.Create(DataModelConditionEvent.EventArgumentType), Entity.RightPath)
                        : null;
                }
                // Right side dynamic
                else
                    RightPath = new DataModelPath(null, Entity.RightPath);
            }
        }

        #endregion

        #region Modification

        /// <inheritdoc />
        public override Type? GetPreferredRightSideType()
        {
            Type? preferredType = Operator?.RightSideType;
            Type? leftSideType = LeftPath?.GetPropertyType();
            if (preferredType == null)
                return null;

            if (leftSideType != null && preferredType.IsAssignableFrom(leftSideType))
                preferredType = leftSideType;

            return preferredType;
        }

        #endregion

        #region Evaluation

        /// <summary>
        ///     Not supported for event predicates, always returns <c>false</c>
        /// </summary>
        public override bool Evaluate()
        {
            return false;
        }

        internal override bool EvaluateObject(object target)
        {
            if (Operator == null || LeftPath == null || !LeftPath.IsValid)
                return false;

            // Compare with a static value
            if (PredicateType == ProfileRightSideType.Static)
            {
                object? leftSideValue = GetEventPathValue(LeftPath, target);
                if (leftSideValue != null && leftSideValue.GetType().IsValueType && RightStaticValue == null)
                    return false;

                return Operator.InternalEvaluate(leftSideValue, RightStaticValue);
            }

            if (RightPath == null || !RightPath.IsValid)
                return false;

            // Compare with dynamic values
            if (PredicateType == ProfileRightSideType.Dynamic)
            {
                // If the path targets a property inside the event, evaluate on the event path value instead of the right path value
                if (RightPath.Target is EventPredicateWrapperDataModel)
                    return Operator.InternalEvaluate(GetEventPathValue(LeftPath, target), GetEventPathValue(RightPath, target));
                return Operator.InternalEvaluate(GetEventPathValue(LeftPath, target), RightPath.GetValue());
            }

            return false;
        }

        #endregion
    }
}
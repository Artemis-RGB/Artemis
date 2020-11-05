using System;
using Artemis.Storage.Entities.Profile;
using Artemis.Storage.Entities.Profile.Conditions;

namespace Artemis.Core
{
    /// <summary>
    ///     A predicate like evaluated inside a <see cref="DataModelConditionList" />
    /// </summary>
    public class DataModelConditionListPredicate : DataModelConditionPredicate
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="DataModelConditionListPredicate" /> class
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="predicateType"></param>
        public DataModelConditionListPredicate(DataModelConditionPart parent, ProfileRightSideType predicateType)
            : base(parent, predicateType, new DataModelConditionListPredicateEntity())
        {
            DataModelConditionList = null!;
            ApplyParentList();
            Initialize();
        }

        internal DataModelConditionListPredicate(DataModelConditionPart parent, DataModelConditionListPredicateEntity entity)
            : base(parent, entity)
        {
            DataModelConditionList = null!;
            ApplyParentList();
            Initialize();
        }

        /// <summary>
        ///     Gets the data model condition list this predicate belongs to
        /// </summary>
        public DataModelConditionList DataModelConditionList { get; private set; }

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

        private object? GetListPathValue(DataModelPath path, object target)
        {
            if (!(path.Target is ListPredicateWrapperDataModel wrapper))
                throw new ArtemisCoreException("Data model condition list predicate has a path with an invalid target");

            wrapper.UntypedValue = target;
            return path.GetValue();
        }

        #region Initialization

        protected override void InitializeLeftPath()
        {
            if (Entity.LeftPath != null)
                LeftPath = DataModelConditionList.ListType != null
                    ? new DataModelPath(ListPredicateWrapperDataModel.Create(DataModelConditionList.ListType), Entity.LeftPath)
                    : null;
        }

        protected override void InitializeRightPath()
        {
            if (PredicateType == ProfileRightSideType.Dynamic && Entity.RightPath != null)
            {
                // Right side dynamic inside the list
                if (Entity.RightPath.WrapperType == PathWrapperType.List)
                {
                    RightPath = DataModelConditionList.ListType != null
                        ? new DataModelPath(ListPredicateWrapperDataModel.Create(DataModelConditionList.ListType), Entity.RightPath)
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
            Type? leftSideType = DataModelConditionList.IsPrimitiveList
                ? DataModelConditionList.ListType
                : LeftPath?.GetPropertyType();
            if (preferredType == null)
                return null;

            if (leftSideType != null && preferredType.IsAssignableFrom(leftSideType))
                preferredType = leftSideType;

            return preferredType;
        }

        #endregion

        #region Evaluation

        /// <summary>
        ///     Not supported for list predicates, always returns <c>false</c>
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

        #endregion
    }
}
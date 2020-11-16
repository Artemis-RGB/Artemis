using System;
using Artemis.Storage.Entities.Profile.Conditions;

namespace Artemis.Core
{
    /// <summary>
    ///     A predicate in a data model condition using either two data model values or one data model value and a
    ///     static value
    /// </summary>
    public class DataModelConditionGeneralPredicate : DataModelConditionPredicate
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="DataModelConditionGeneralPredicate" /> class
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="predicateType"></param>
        public DataModelConditionGeneralPredicate(DataModelConditionPart parent, ProfileRightSideType predicateType)
            : base(parent, predicateType, new DataModelConditionGeneralPredicateEntity())
        {
            Initialize();
        }

        internal DataModelConditionGeneralPredicate(DataModelConditionPart parent, DataModelConditionGeneralPredicateEntity entity)
            : base(parent, entity)
        {
            Initialize();
        }

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

        #region Initialization

        /// <inheritdoc />
        protected override void InitializeLeftPath()
        {
            if (Entity.LeftPath != null)
                LeftPath = new DataModelPath(null, Entity.LeftPath);
        }

        /// <inheritdoc />
        protected override void InitializeRightPath()
        {
            if (PredicateType == ProfileRightSideType.Dynamic && Entity.RightPath != null)
                RightPath = new DataModelPath(null, Entity.RightPath);
        }

        #endregion
    }
}
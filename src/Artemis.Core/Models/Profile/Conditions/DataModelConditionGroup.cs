using System;
using System.Linq;
using Artemis.Storage.Entities.Profile.Abstract;
using Artemis.Storage.Entities.Profile.Conditions;

namespace Artemis.Core
{
    /// <summary>
    ///     A group containing zero to many <see cref="DataModelConditionPart" />s which it evaluates using a boolean specific
    ///     operator
    /// </summary>
    public class DataModelConditionGroup : DataModelConditionPart
    {
        private bool _disposed;

        /// <summary>
        ///     Creates a new instance of the <see cref="DataModelConditionGroup" /> class
        /// </summary>
        /// <param name="parent"></param>
        public DataModelConditionGroup(DataModelConditionPart parent)
        {
            Parent = parent;
            Entity = new DataModelConditionGroupEntity();
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="DataModelConditionGroup" /> class
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="entity"></param>
        public DataModelConditionGroup(DataModelConditionPart parent, DataModelConditionGroupEntity entity)
        {
            Parent = parent;
            Entity = entity;
            BooleanOperator = (BooleanOperator) Entity.BooleanOperator;

            foreach (var childEntity in Entity.Children)
            {
                if (childEntity is DataModelConditionGroupEntity groupEntity)
                    AddChild(new DataModelConditionGroup(this, groupEntity));
                else if (childEntity is DataModelConditionListEntity listEntity)
                    AddChild(new DataModelConditionList(this, listEntity));
                else if (childEntity is DataModelConditionPredicateEntity predicateEntity)
                    AddChild(new DataModelConditionPredicate(this, predicateEntity));
                else if (childEntity is DataModelConditionListPredicateEntity listPredicateEntity)
                    AddChild(new DataModelConditionListPredicate(this, listPredicateEntity));
            }
        }

        /// <summary>
        ///     Gets or sets the boolean operator of this group
        /// </summary>
        public BooleanOperator BooleanOperator { get; set; }

        internal DataModelConditionGroupEntity Entity { get; set; }

        /// <inheritdoc />
        public override bool Evaluate()
        {
            if (_disposed)
                throw new ObjectDisposedException("DataModelConditionGroup");

            // Empty groups are always true
            if (Children.Count == 0)
                return true;
            // Groups with only one child ignore the boolean operator
            if (Children.Count == 1)
                return Children[0].Evaluate();

            switch (BooleanOperator)
            {
                case BooleanOperator.And:
                    return Children.All(c => c.Evaluate());
                case BooleanOperator.Or:
                    return Children.Any(c => c.Evaluate());
                case BooleanOperator.AndNot:
                    return Children.All(c => !c.Evaluate());
                case BooleanOperator.OrNot:
                    return Children.Any(c => !c.Evaluate());
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #region IDisposable

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            _disposed = true;
            foreach (var child in Children)
                child.Dispose();

            base.Dispose(disposing);
        }

        #endregion

        /// <inheritdoc />
        internal override bool EvaluateObject(object target)
        {
            if (_disposed)
                throw new ObjectDisposedException("DataModelConditionGroup");

            // Empty groups are always true
            if (Children.Count == 0)
                return true;
            // Groups with only one child ignore the boolean operator
            if (Children.Count == 1)
                return Children[0].EvaluateObject(target);

            return BooleanOperator switch
            {
                BooleanOperator.And => Children.All(c => c.EvaluateObject(target)),
                BooleanOperator.Or => Children.Any(c => c.EvaluateObject(target)),
                BooleanOperator.AndNot => Children.All(c => !c.EvaluateObject(target)),
                BooleanOperator.OrNot => Children.Any(c => !c.EvaluateObject(target)),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        internal override void Save()
        {
            Entity.BooleanOperator = (int) BooleanOperator;

            Entity.Children.Clear();
            Entity.Children.AddRange(Children.Select(c => c.GetEntity()));
            foreach (var child in Children)
                child.Save();
        }

        internal override DataModelConditionPartEntity GetEntity()
        {
            return Entity;
        }
    }

    /// <summary>
    ///     Represents a boolean operator
    /// </summary>
    public enum BooleanOperator
    {
        /// <summary>
        ///     All the conditions in the group should evaluate to true
        /// </summary>
        And,

        /// <summary>
        ///     Any of the conditions in the group should evaluate to true
        /// </summary>
        Or,

        /// <summary>
        ///     All the conditions in the group should evaluate to false
        /// </summary>
        AndNot,

        /// <summary>
        ///     Any of the conditions in the group should evaluate to false
        /// </summary>
        OrNot
    }
}
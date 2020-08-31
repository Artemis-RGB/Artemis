using System;
using System.Linq;
using Artemis.Core.Services;
using Artemis.Storage.Entities.Profile.Abstract;
using Artemis.Storage.Entities.Profile.Conditions;

namespace Artemis.Core
{
    /// <summary>
    ///     A group containing zero to many <see cref="DisplayConditionPart" />s which it evaluates using a boolean specific
    ///     operator
    /// </summary>
    public class DisplayConditionGroup : DisplayConditionPart
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="DisplayConditionGroup" /> class
        /// </summary>
        /// <param name="parent"></param>
        public DisplayConditionGroup(DisplayConditionPart parent)
        {
            Parent = parent;
            Entity = new DisplayConditionGroupEntity();
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="DisplayConditionGroup" /> class
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="entity"></param>
        public DisplayConditionGroup(DisplayConditionPart parent, DisplayConditionGroupEntity entity)
        {
            Parent = parent;
            Entity = entity;
            BooleanOperator = (BooleanOperator) Entity.BooleanOperator;

            foreach (var childEntity in Entity.Children)
            {
                if (childEntity is DisplayConditionGroupEntity groupEntity)
                    AddChild(new DisplayConditionGroup(this, groupEntity));
                else if (childEntity is DisplayConditionListEntity listEntity)
                    AddChild(new DisplayConditionList(this, listEntity));
                else if (childEntity is DisplayConditionPredicateEntity predicateEntity)
                    AddChild(new DisplayConditionPredicate(this, predicateEntity));
                else if (childEntity is DisplayConditionListPredicateEntity listPredicateEntity)
                    AddChild(new DisplayConditionListPredicate(this, listPredicateEntity));
            }
        }

        /// <summary>
        ///     Gets or sets the boolean operator of this group
        /// </summary>
        public BooleanOperator BooleanOperator { get; set; }

        internal DisplayConditionGroupEntity Entity { get; set; }

        /// <inheritdoc />
        public override bool Evaluate()
        {
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

        /// <inheritdoc />
        public override bool EvaluateObject(object target)
        {
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

        internal override void ApplyToEntity()
        {
            Entity.BooleanOperator = (int) BooleanOperator;

            Entity.Children.Clear();
            Entity.Children.AddRange(Children.Select(c => c.GetEntity()));
            foreach (var child in Children)
                child.ApplyToEntity();
        }

        internal override void Initialize(IDataModelService dataModelService)
        {
            foreach (var child in Children)
                child.Initialize(dataModelService);
        }

        internal override DisplayConditionPartEntity GetEntity()
        {
            return Entity;
        }
    }

    public enum BooleanOperator
    {
        And,
        Or,
        AndNot,
        OrNot
    }
}
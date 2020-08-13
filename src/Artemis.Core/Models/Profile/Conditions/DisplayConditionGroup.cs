using System;
using System.Linq;
using Artemis.Core.Models.Profile.Conditions.Abstract;
using Artemis.Core.Services.Interfaces;
using Artemis.Storage.Entities.Profile;
using Artemis.Storage.Entities.Profile.Abstract;

namespace Artemis.Core.Models.Profile.Conditions
{
    public class DisplayConditionGroup : DisplayConditionPart
    {
        public DisplayConditionGroup(DisplayConditionPart parent)
        {
            Parent = parent;
            Entity = new DisplayConditionGroupEntity();
        }

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

        public BooleanOperator BooleanOperator { get; set; }
        public DisplayConditionGroupEntity Entity { get; set; }

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
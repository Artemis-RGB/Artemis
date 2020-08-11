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
            DisplayConditionGroupEntity = new DisplayConditionGroupEntity();
        }

        public DisplayConditionGroup(DisplayConditionPart parent, DisplayConditionGroupEntity entity)
        {
            Parent = parent;
            DisplayConditionGroupEntity = entity;
            BooleanOperator = (BooleanOperator) DisplayConditionGroupEntity.BooleanOperator;

            foreach (var childEntity in DisplayConditionGroupEntity.Children)
            {
                if (childEntity is DisplayConditionGroupEntity groupEntity)
                    AddChild(new DisplayConditionGroup(this, groupEntity));
                else if (childEntity is DisplayConditionPredicateEntity predicateEntity)
                    AddChild(new DisplayConditionPredicate(this, predicateEntity));
                else if (childEntity is DisplayConditionListPredicateEntity listPredicateEntity)
                    AddChild(new DisplayConditionListPredicate(this, listPredicateEntity));
            }
        }

        public BooleanOperator BooleanOperator { get; set; }
        public DisplayConditionGroupEntity DisplayConditionGroupEntity { get; set; }

        public override bool Evaluate()
        {
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

        internal override void ApplyToEntity()
        {
            DisplayConditionGroupEntity.BooleanOperator = (int) BooleanOperator;

            DisplayConditionGroupEntity.Children.Clear();
            DisplayConditionGroupEntity.Children.AddRange(Children.Select(c => c.GetEntity()));
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
            return DisplayConditionGroupEntity;
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
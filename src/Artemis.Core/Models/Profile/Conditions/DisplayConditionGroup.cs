using System;
using Artemis.Core.Models.Profile.Conditions.Abstract;
using Artemis.Storage.Entities.Profile;

namespace Artemis.Core.Models.Profile.Conditions
{
    public class DisplayConditionGroup : DisplayConditionPart
    {
        public DisplayConditionGroup(DisplayConditionPart parent)
        {
            Parent = parent;
            EntityId = Guid.NewGuid();
            DisplayConditionGroupEntity = new DisplayConditionGroupEntity();
        }

        public DisplayConditionGroup(DisplayConditionPart parent, DisplayConditionGroupEntity entity)
        {
            Parent = parent;
            DisplayConditionGroupEntity = entity;
            EntityId = DisplayConditionGroupEntity.Id;
            BooleanOperator = (BooleanOperator) DisplayConditionGroupEntity.BooleanOperator;

            foreach (var childEntity in DisplayConditionGroupEntity.Children)
            {
                if (childEntity is DisplayConditionGroupEntity groupEntity)
                    AddChild(new DisplayConditionGroup(this, groupEntity));
                if (childEntity is DisplayConditionPredicateEntity predicateEntity)
                    AddChild(new DisplayConditionPredicate(this, predicateEntity));
            }
        }

        public BooleanOperator BooleanOperator { get; set; }
        public DisplayConditionGroupEntity DisplayConditionGroupEntity { get; set; }

        public override void ApplyToEntity()
        {
            DisplayConditionGroupEntity.Id = EntityId;
            DisplayConditionGroupEntity.ParentId = Parent?.EntityId ?? new Guid();
            DisplayConditionGroupEntity.BooleanOperator = (int) BooleanOperator;

            foreach (var child in Children)
                child.ApplyToEntity();
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
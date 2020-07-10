using System.Collections.Generic;
using Artemis.Core.Services.Interfaces;
using Artemis.Storage.Entities.Profile.Abstract;

namespace Artemis.Core.Models.Profile.Conditions.Abstract
{
    public abstract class DisplayConditionPart
    {
        private readonly List<DisplayConditionPart> _children;

        protected DisplayConditionPart()
        {
            _children = new List<DisplayConditionPart>();
        }

        public DisplayConditionPart Parent { get; set; }
        public IReadOnlyList<DisplayConditionPart> Children => _children.AsReadOnly();

        public void AddChild(DisplayConditionPart displayConditionPart)
        {
            if (!_children.Contains(displayConditionPart))
            {
                displayConditionPart.Parent = this;
                _children.Add(displayConditionPart);
            }
        }

        public void RemoveChild(DisplayConditionPart displayConditionPart)
        {
            if (_children.Contains(displayConditionPart))
            {
                displayConditionPart.Parent = null;
                _children.Remove(displayConditionPart);
            }
        }

        public abstract DisplayConditionPartEntity GetEntity();

        internal abstract void ApplyToEntity();
        internal abstract void Initialize(IDataModelService dataModelService);
    }
}
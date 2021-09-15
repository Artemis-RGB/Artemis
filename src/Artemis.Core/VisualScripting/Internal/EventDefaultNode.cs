using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Artemis.Core.Modules;
using Humanizer;

namespace Artemis.Core.Internal
{
    internal class EventDefaultNode : Node
    {
        private readonly Dictionary<PropertyInfo, OutputPin> _propertyPins;
        private IDataModelEvent? _dataModelEvent;

        public EventDefaultNode() : base("Event Arguments", "Contains the event arguments that triggered the evaluation")
        {
            _propertyPins = new Dictionary<PropertyInfo, OutputPin>();
        }

        public override bool IsDefaultNode => true;

        public void UpdateDataModelEvent(IDataModelEvent dataModelEvent)
        {
            if (_dataModelEvent == dataModelEvent)
                return;

            foreach (var (_, outputPin) in _propertyPins)
                RemovePin(outputPin);
            _propertyPins.Clear();

            _dataModelEvent = dataModelEvent;
            foreach (PropertyInfo propertyInfo in dataModelEvent.ArgumentsType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CustomAttributes.All(a => a.AttributeType != typeof(DataModelIgnoreAttribute))))
                _propertyPins.Add(propertyInfo, CreateOutputPin(propertyInfo.PropertyType, propertyInfo.Name.Humanize()));
        }

        public override void Evaluate()
        {
            if (_dataModelEvent?.LastEventArgumentsUntyped == null)
                return;

            foreach (var (propertyInfo, outputPin) in _propertyPins)
            {
                if (outputPin.ConnectedTo.Any())
                    outputPin.Value = propertyInfo.GetValue(_dataModelEvent.LastEventArgumentsUntyped) ?? outputPin.Type.GetDefault()!;
            }
        }
    }
}
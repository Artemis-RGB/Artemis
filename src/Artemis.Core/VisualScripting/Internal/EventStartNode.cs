using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Humanizer;

namespace Artemis.Core.Internal
{
    internal class EventStartNode : Node
    {
        private IDataModelEvent? _dataModelEvent;
        private readonly Dictionary<PropertyInfo, OutputPin> _propertyPins;

        public EventStartNode() : base("Event Arguments", "Contains the event arguments that triggered the evaluation")
        {
            _propertyPins = new Dictionary<PropertyInfo, OutputPin>();
        }

        public void UpdateDataModelEvent(IDataModelEvent dataModelEvent)
        {
            if (_dataModelEvent == dataModelEvent)
                return;

            foreach (var (_, outputPin) in _propertyPins)
                RemovePin(outputPin);
            _propertyPins.Clear();

            _dataModelEvent = dataModelEvent;
            foreach (PropertyInfo propertyInfo in dataModelEvent.ArgumentsType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                _propertyPins.Add(propertyInfo, CreateOutputPin(propertyInfo.PropertyType, propertyInfo.Name.Humanize()));
        }

        #region Overrides of Node

        /// <inheritdoc />
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

        #endregion
    }
}
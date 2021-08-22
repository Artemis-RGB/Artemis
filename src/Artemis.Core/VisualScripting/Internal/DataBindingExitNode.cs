using System;
using System.Collections.Generic;
using System.Linq;

namespace Artemis.Core.Internal
{
    internal class DataBindingExitNode<TLayerProperty> : Node, IExitNode
    {
        private readonly Dictionary<IDataBindingProperty, InputPin> _propertyPins = new();
        private readonly Dictionary<IDataBindingProperty, object> _propertyValues = new();

        public DataBinding<TLayerProperty> DataBinding { get; }

        public DataBindingExitNode(DataBinding<TLayerProperty> dataBinding) : base(dataBinding.LayerProperty.PropertyDescription.Name ?? "", "")
        {
            DataBinding = dataBinding;
            DataBinding.DataBindingPropertiesCleared += DataBindingOnDataBindingPropertiesCleared;
            DataBinding.DataBindingPropertyRegistered += DataBindingOnDataBindingPropertyRegistered;

            CreateInputPins();
        }

        public override bool IsExitNode => true;

        public override void Evaluate()
        {
            foreach (var (property, inputPin) in _propertyPins)
            {
                if (inputPin.ConnectedTo.Any())
                    _propertyValues[property] = inputPin.Value;
                else 
                    _propertyValues.Remove(property);
            }
        }

        public void ApplyToDataBinding()
        {
            foreach (var (property, pendingValue) in _propertyValues)
                property.SetValue(pendingValue);
        }

        private void ClearInputPins()
        {
            while (Pins.Any())
                RemovePin((Pin) Pins.First());

            _propertyPins.Clear();
            _propertyValues.Clear();
        }

        private void CreateInputPins()
        {
            ClearInputPins();

            foreach (IDataBindingProperty property in DataBinding.Properties)
                _propertyPins.Add(property, CreateInputPin(property.ValueType, property.DisplayName));
        }

        #region Event handlers

        private void DataBindingOnDataBindingPropertyRegistered(object? sender, DataBindingEventArgs e)
        {
            CreateInputPins();
        }

        private void DataBindingOnDataBindingPropertiesCleared(object? sender, DataBindingEventArgs e)
        {
            ClearInputPins();
        }
        
        #endregion
    }
}
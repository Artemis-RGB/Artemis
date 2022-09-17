﻿using System.Collections.Generic;
using System.Linq;

namespace Artemis.Core.Internal;

internal class DataBindingExitNode<TLayerProperty> : Node, IExitNode
{
    private readonly Dictionary<IDataBindingProperty, InputPin> _propertyPins = new();
    private readonly Dictionary<IDataBindingProperty, object> _propertyValues = new();

    public DataBindingExitNode(DataBinding<TLayerProperty> dataBinding) : base(dataBinding.LayerProperty.PropertyDescription.Name ?? "", "")
    {
        DataBinding = dataBinding;
        DataBinding.DataBindingPropertiesCleared += DataBindingOnDataBindingPropertiesCleared;
        DataBinding.DataBindingPropertyRegistered += DataBindingOnDataBindingPropertyRegistered;
        Id = IExitNode.NodeId;

        CreateInputPins();
    }

    public DataBinding<TLayerProperty> DataBinding { get; }

    public void ApplyToDataBinding()
    {
        foreach ((IDataBindingProperty? property, object? pendingValue) in _propertyValues)
        {
            if (pendingValue != null)
                property.SetValue(pendingValue);
        }
    }

    public override void Evaluate()
    {
        foreach ((IDataBindingProperty? property, InputPin? inputPin) in _propertyPins)
        {
            if (inputPin.ConnectedTo.Any())
                _propertyValues[property] = inputPin.Value!;
            else
                _propertyValues.Remove(property);
        }
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
        {
            _propertyPins.Add(property, Numeric.IsTypeCompatible(property.ValueType)
                ? CreateInputPin(typeof(Numeric), property.DisplayName)
                : CreateInputPin(property.ValueType, property.DisplayName)
            );
        }
    }

    private void DataBindingOnDataBindingPropertyRegistered(object? sender, DataBindingEventArgs e)
    {
        CreateInputPins();
    }

    private void DataBindingOnDataBindingPropertiesCleared(object? sender, DataBindingEventArgs e)
    {
        ClearInputPins();
    }

    public override bool IsExitNode => true;
}
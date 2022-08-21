using System;
using System.Collections.ObjectModel;
using Artemis.Core.Modules;

namespace Artemis.Core;

/// <summary>
///     Represents a data binding that binds a certain <see cref="LayerProperty{T}" /> to a value inside a
///     <see cref="DataModel" />
/// </summary>
public interface IDataBinding : IStorageModel, IDisposable
{
    /// <summary>
    ///     Gets the layer property the data binding is applied to
    /// </summary>
    ILayerProperty BaseLayerProperty { get; }

    /// <summary>
    ///     Gets the script used to populate the data binding
    /// </summary>
    INodeScript Script { get; }

    /// <summary>
    ///     Gets a list of sub-properties this data binding applies to
    /// </summary>
    ReadOnlyCollection<IDataBindingProperty> Properties { get; }

    /// <summary>
    ///     Gets a boolean indicating whether the data binding is enabled or not
    /// </summary>
    bool IsEnabled { get; set; }

    /// <summary>
    ///     Applies the pending value of the data binding to the property
    /// </summary>
    void Apply();

    /// <summary>
    ///     If the data binding is enabled, loads the node script for that data binding
    /// </summary>
    void LoadNodeScript();

    /// <summary>
    ///     Occurs when a data binding property has been added
    /// </summary>
    public event EventHandler<DataBindingEventArgs>? DataBindingPropertyRegistered;

    /// <summary>
    ///     Occurs when all data binding properties have been removed
    /// </summary>
    public event EventHandler<DataBindingEventArgs>? DataBindingPropertiesCleared;

    /// <summary>
    ///     Occurs when a data binding has been enabled
    /// </summary>
    public event EventHandler<DataBindingEventArgs>? DataBindingEnabled;

    /// <summary>
    ///     Occurs when a data binding has been disabled
    /// </summary>
    public event EventHandler<DataBindingEventArgs>? DataBindingDisabled;
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core.Events;

namespace Artemis.Core;

/// <inheritdoc cref="IPin" />
public abstract class Pin : CorePropertyChanged, IPin
{
    #region Constructors

    /// <summary>
    ///     Creates a new instance of the <see cref="Pin" /> class on the provided node with the provided name
    /// </summary>
    /// <param name="node">The node the pin belongs to</param>
    /// <param name="name">The name of the pin</param>
    protected Pin(INode node, string name = "")
    {
        Node = node;
        _name = name;
    }

    #endregion

    /// <inheritdoc />
    public event EventHandler<SingleValueEventArgs<IPin>>? PinConnected;

    /// <inheritdoc />
    public event EventHandler<SingleValueEventArgs<IPin>>? PinDisconnected;

    #region Properties & Fields

    /// <inheritdoc />
    public INode Node { get; }

    /// <inheritdoc />
    public string Name
    {
        get => _name;
        set => SetAndNotify(ref _name, value);
    }

    private bool _isEvaluated;

    /// <inheritdoc />
    public bool IsEvaluated
    {
        get => _isEvaluated;
        set => SetAndNotify(ref _isEvaluated, value);
    }

    private bool _isNumeric;

    /// <inheritdoc />
    public bool IsNumeric
    {
        get => _isNumeric;
        protected set => SetAndNotify(ref _isNumeric, value);
    }

    private readonly List<IPin> _connectedTo = new();
    private string _name;

    /// <inheritdoc />
    public IReadOnlyList<IPin> ConnectedTo => new ReadOnlyCollection<IPin>(_connectedTo);

    /// <inheritdoc />
    public abstract PinDirection Direction { get; }

    /// <inheritdoc />
    public abstract Type Type { get; }

    /// <inheritdoc />
    public abstract object? PinValue { get; }

    #endregion

    #region Methods

    /// <inheritdoc />
    public void Reset()
    {
        IsEvaluated = false;
    }

    /// <inheritdoc />
    public void ConnectTo(IPin pin)
    {
        _connectedTo.Add(pin);
        if (!pin.ConnectedTo.Contains(this))
            pin.ConnectTo(this);

        OnPropertyChanged(nameof(ConnectedTo));
        PinConnected?.Invoke(this, new SingleValueEventArgs<IPin>(pin));
    }

    /// <inheritdoc />
    public void DisconnectFrom(IPin pin)
    {
        _connectedTo.Remove(pin);
        if (pin.ConnectedTo.Contains(this))
            pin.DisconnectFrom(this);

        OnPropertyChanged(nameof(ConnectedTo));
        PinDisconnected?.Invoke(this, new SingleValueEventArgs<IPin>(pin));
    }

    /// <inheritdoc />
    public void DisconnectAll()
    {
        if (!_connectedTo.Any())
            return;

        List<IPin> connectedPins = new(_connectedTo);
        _connectedTo.Clear();

        foreach (IPin pin in connectedPins)
        {
            PinDisconnected?.Invoke(this, new SingleValueEventArgs<IPin>(pin));
            if (pin.ConnectedTo.Contains(this))
                pin.DisconnectFrom(this);
        }

        OnPropertyChanged(nameof(ConnectedTo));
    }

    /// <inheritdoc />
    public bool IsTypeCompatible(Type type)
    {
        return Type == type
               || (Type == typeof(Enum) && type.IsEnum)
               || (Type.IsEnum && type == typeof(Enum))
               || (Direction == PinDirection.Input && Type == typeof(object))
               || (Direction == PinDirection.Output && type == typeof(object));
    }

    #endregion
}
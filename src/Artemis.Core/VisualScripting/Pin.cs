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
    public bool IsTypeCompatible(Type type, bool forgivingEnumMatching = true)
    {
        return Type == type
               || (Direction == PinDirection.Input && Type == typeof(Enum) && type.IsEnum && forgivingEnumMatching)
               || (Direction == PinDirection.Output && type == typeof(Enum) && Type.IsEnum && forgivingEnumMatching)
               || (Direction == PinDirection.Input && Type == typeof(object))
               || (Direction == PinDirection.Output && type == typeof(object));
    }

    /// <summary>
    ///     Changes the type of this pin, disconnecting any pins that are incompatible with the new type.
    /// </summary>
    /// <param name="type">The new type of the pin.</param>
    /// <param name="currentType">The backing field of the current type of the pin.</param>
    protected void ChangeType(Type type, ref Type currentType)
    {
        // Enums are a special case that disconnect and, if still compatible, reconnect
        if (type.IsEnum && currentType.IsEnum)
        {
            List<IPin> connections = new(ConnectedTo);
            DisconnectAll();

            // Change the type
            SetAndNotify(ref currentType, type, nameof(Type));
            IsNumeric = type == typeof(Numeric);

            foreach (IPin pin in connections.Where(p => p.IsTypeCompatible(type)))
                ConnectTo(pin);
        }
        // Disconnect pins incompatible with the new type
        else
        {
            List<IPin> toDisconnect = ConnectedTo.Where(p => !p.IsTypeCompatible(type, false)).ToList();
            foreach (IPin pin in toDisconnect)
                DisconnectFrom(pin);

            // Change the type
            SetAndNotify(ref currentType, type, nameof(Type));
            IsNumeric = type == typeof(Numeric);
        }
    }
    
    #endregion
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core.Events;
using DryIoc;

namespace Artemis.Core;

/// <summary>
///     Represents a kind of node inside a <see cref="NodeScript" />
/// </summary>
public abstract class Node : BreakableModel, INode
{
    /// <inheritdoc />
    public event EventHandler? Resetting;

    /// <inheritdoc />
    public event EventHandler<SingleValueEventArgs<IPin>>? PinAdded;

    /// <inheritdoc />
    public event EventHandler<SingleValueEventArgs<IPin>>? PinRemoved;

    /// <inheritdoc />
    public event EventHandler<SingleValueEventArgs<IPinCollection>>? PinCollectionAdded;

    /// <inheritdoc />
    public event EventHandler<SingleValueEventArgs<IPinCollection>>? PinCollectionRemoved;

    #region Properties & Fields

    private readonly List<OutputPin> _outputPinBucket = new();
    private readonly List<InputPin> _inputPinBucket = new();

    private Guid _id;

    /// <inheritdoc />
    public Guid Id
    {
        get => _id;
        set => SetAndNotify(ref _id, value);
    }

    /// <inheritdoc />
    public NodeData NodeData { get; set; }

    private string _name;

    /// <inheritdoc />
    public string Name
    {
        get => _name;
        set => SetAndNotify(ref _name, value);
    }

    private string _description;

    /// <inheritdoc />
    public string Description
    {
        get => _description;
        set => SetAndNotify(ref _description, value);
    }

    private double _x;

    /// <inheritdoc />
    public double X
    {
        get => _x;
        set => SetAndNotify(ref _x, value);
    }

    private double _y;

    /// <inheritdoc />
    public double Y
    {
        get => _y;
        set => SetAndNotify(ref _y, value);
    }

    /// <inheritdoc />
    public string HelpUrl
    {
        get => _helpUrl;
        set => SetAndNotify(ref _helpUrl, value);
    }

    /// <inheritdoc />
    public virtual bool IsExitNode => false;

    /// <inheritdoc />
    public virtual bool IsDefaultNode => false;

    private readonly List<IPin> _pins = new();

    /// <inheritdoc />
    public IReadOnlyCollection<IPin> Pins => new ReadOnlyCollection<IPin>(_pins);

    private readonly List<IPinCollection> _pinCollections = new();
    private string _helpUrl;

    /// <inheritdoc />
    public IReadOnlyCollection<IPinCollection> PinCollections => new ReadOnlyCollection<IPinCollection>(_pinCollections);

    /// <inheritdoc />
    public override string BrokenDisplayName => Name;

    #endregion

    #region Construtors

    /// <summary>
    ///     Creates a new instance of the <see cref="Node" /> class with an empty name and description
    /// </summary>
    protected Node()
    {
        _name = string.Empty;
        _description = string.Empty;
        _helpUrl = string.Empty;
        _id = Guid.NewGuid();
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="Node" /> class with the provided name and description
    /// </summary>
    protected Node(string name, string description)
    {
        _name = name;
        _description = description;
        _helpUrl = string.Empty;
        _id = Guid.NewGuid();
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Creates a new input pin and adds it to the <see cref="Pins" /> collection
    /// </summary>
    /// <param name="name">The name of the pin</param>
    /// <typeparam name="T">The type of value the pin will hold</typeparam>
    /// <returns>The newly created pin</returns>
    public InputPin<T> CreateInputPin<T>(string name = "")
    {
        InputPin<T> pin = new(this, name);
        _pins.Add(pin);
        PinAdded?.Invoke(this, new SingleValueEventArgs<IPin>(pin));
        return pin;
    }

    /// <summary>
    ///     Creates a new input pin and adds it to the <see cref="Pins" /> collection
    /// </summary>
    /// <param name="type">The type of value the pin will hold</param>
    /// <param name="name">The name of the pin</param>
    /// <returns>The newly created pin</returns>
    public InputPin CreateInputPin(Type type, string name = "")
    {
        InputPin pin = new(this, type, name);
        _pins.Add(pin);
        PinAdded?.Invoke(this, new SingleValueEventArgs<IPin>(pin));
        return pin;
    }

    /// <summary>
    ///     Creates a new output pin and adds it to the <see cref="Pins" /> collection
    /// </summary>
    /// <param name="name">The name of the pin</param>
    /// <typeparam name="T">The type of value the pin will hold</typeparam>
    /// <returns>The newly created pin</returns>
    public OutputPin<T> CreateOutputPin<T>(string name = "")
    {
        OutputPin<T> pin = new(this, name);
        _pins.Add(pin);
        PinAdded?.Invoke(this, new SingleValueEventArgs<IPin>(pin));
        return pin;
    }

    /// <summary>
    ///     Creates a new output pin and adds it to the <see cref="Pins" /> collection
    /// </summary>
    /// <param name="type">The type of value the pin will hold</param>
    /// <param name="name">The name of the pin</param>
    /// <returns>The newly created pin</returns>
    public OutputPin CreateOutputPin(Type type, string name = "")
    {
        OutputPin pin = new(this, type, name);
        _pins.Add(pin);
        PinAdded?.Invoke(this, new SingleValueEventArgs<IPin>(pin));
        return pin;
    }

    /// <summary>
    ///     Creates or adds an output pin to the node using a bucket.
    ///     The bucket might grow a bit over time as the user edits the node but pins won't get lost, enabling undo/redo in the
    ///     editor.
    /// </summary>
    public OutputPin CreateOrAddOutputPin(Type valueType, string displayName)
    {
        // Grab the first pin from the bucket that isn't on the node yet
        OutputPin? pin = _outputPinBucket.FirstOrDefault(p => !Pins.Contains(p));

        if (Numeric.IsTypeCompatible(valueType))
            valueType = typeof(Numeric);

        // If there is none, create a new one and add it to the bucket
        if (pin == null)
        {
            pin = CreateOutputPin(valueType, displayName);
            _outputPinBucket.Add(pin);
        }
        // If there was a pin in the bucket, update it's type and display name and reuse it
        else
        {
            pin.ChangeType(valueType);
            pin.Name = displayName;
            AddPin(pin);
        }

        return pin;
    }

    /// <summary>
    ///     Creates or adds an input pin to the node using a bucket.
    ///     The bucket might grow a bit over time as the user edits the node but pins won't get lost, enabling undo/redo in the
    ///     editor.
    /// </summary>
    public InputPin CreateOrAddInputPin(Type valueType, string displayName)
    {
        // Grab the first pin from the bucket that isn't on the node yet
        InputPin? pin = _inputPinBucket.FirstOrDefault(p => !Pins.Contains(p));

        if (Numeric.IsTypeCompatible(valueType))
            valueType = typeof(Numeric);

        // If there is none, create a new one and add it to the bucket
        if (pin == null)
        {
            pin = CreateInputPin(valueType, displayName);
            _inputPinBucket.Add(pin);
        }
        // If there was a pin in the bucket, update it's type and display name and reuse it
        else
        {
            pin.ChangeType(valueType);
            pin.Name = displayName;
            AddPin(pin);
        }

        return pin;
    }

    /// <summary>
    ///     Removes the provided <paramref name="pin" /> from the node and it's <see cref="Pins" /> collection
    /// </summary>
    /// <param name="pin">The pin to remove</param>
    /// <returns><see langword="true" /> if the pin was removed; otherwise <see langword="false" />.</returns>
    public bool RemovePin(Pin pin)
    {
        bool isRemoved = _pins.Remove(pin);
        if (isRemoved)
        {
            pin.DisconnectAll();
            PinRemoved?.Invoke(this, new SingleValueEventArgs<IPin>(pin));
        }

        return isRemoved;
    }

    /// <summary>
    ///     Adds an existing <paramref name="pin" /> to the <see cref="Pins" /> collection.
    /// </summary>
    /// <param name="pin">The pin to add</param>
    public void AddPin(Pin pin)
    {
        if (pin.Node != this)
            throw new ArtemisCoreException("Can't add a pin to a node that belongs to a different node than the one it's being added to.");
        if (_pins.Contains(pin))
            return;

        _pins.Add(pin);
        PinAdded?.Invoke(this, new SingleValueEventArgs<IPin>(pin));
    }

    /// <summary>
    ///     Creates a new input pin collection and adds it to the <see cref="PinCollections" /> collection
    /// </summary>
    /// <typeparam name="T">The type of value the pins of this collection will hold</typeparam>
    /// <param name="name">The name of the pin collection</param>
    /// <param name="initialCount">The amount of pins to initially add to the collection</param>
    /// <returns>The resulting input pin collection</returns>
    public InputPinCollection<T> CreateInputPinCollection<T>(string name = "", int initialCount = 1)
    {
        InputPinCollection<T> pin = new(this, name, initialCount);
        _pinCollections.Add(pin);
        PinCollectionAdded?.Invoke(this, new SingleValueEventArgs<IPinCollection>(pin));
        return pin;
    }

    /// <summary>
    ///     Creates a new input pin collection and adds it to the <see cref="PinCollections" /> collection
    /// </summary>
    /// <param name="type">The type of value the pins of this collection will hold</param>
    /// <param name="name">The name of the pin collection</param>
    /// <param name="initialCount">The amount of pins to initially add to the collection</param>
    /// <returns>The resulting input pin collection</returns>
    public InputPinCollection CreateInputPinCollection(Type type, string name = "", int initialCount = 1)
    {
        InputPinCollection pin = new(this, type, name, initialCount);
        _pinCollections.Add(pin);
        PinCollectionAdded?.Invoke(this, new SingleValueEventArgs<IPinCollection>(pin));
        return pin;
    }

    /// <summary>
    ///     Creates a new output pin collection and adds it to the <see cref="PinCollections" /> collection
    /// </summary>
    /// <typeparam name="T">The type of value the pins of this collection will hold</typeparam>
    /// <param name="name">The name of the pin collection</param>
    /// <param name="initialCount">The amount of pins to initially add to the collection</param>
    /// <returns>The resulting output pin collection</returns>
    public OutputPinCollection<T> CreateOutputPinCollection<T>(string name = "", int initialCount = 1)
    {
        OutputPinCollection<T> pin = new(this, name, initialCount);
        _pinCollections.Add(pin);
        PinCollectionAdded?.Invoke(this, new SingleValueEventArgs<IPinCollection>(pin));
        return pin;
    }

    /// <summary>
    ///     Removes the provided <paramref name="pinCollection" /> from the node and it's <see cref="PinCollections" />
    ///     collection
    /// </summary>
    /// <param name="pinCollection">The pin collection to remove</param>
    /// <returns><see langword="true" /> if the pin collection was removed; otherwise <see langword="false" />.</returns>
    public bool RemovePinCollection(PinCollection pinCollection)
    {
        bool isRemoved = _pinCollections.Remove(pinCollection);
        if (isRemoved)
        {
            foreach (IPin pin in pinCollection)
                pin.DisconnectAll();
            PinCollectionRemoved?.Invoke(this, new SingleValueEventArgs<IPinCollection>(pinCollection));
        }

        return isRemoved;
    }

    /// <summary>
    ///     Called when the node was loaded from storage or newly created, at this point pin connections aren't reestablished
    ///     yet.
    /// </summary>
    /// <param name="script">The script the node is contained in</param>
    public virtual void Initialize(INodeScript script)
    {
    }

    /// <summary>
    ///     Evaluates the value of the output pins of this node
    /// </summary>
    public abstract void Evaluate();

    /// <inheritdoc />
    public virtual void Reset()
    {
        foreach (IPin pin in _pins)
            pin.Reset();
        foreach (IPinCollection pinCollection in _pinCollections)
            pinCollection.Reset();

        Resetting?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public void TryInitialize(INodeScript script)
    {
        TryOrBreak(() => Initialize(script), "Failed to initialize");
    }

    /// <inheritdoc />
    public void TryEvaluate()
    {
        TryOrBreak(Evaluate, "Failed to evaluate");
    }

    /// <summary>
    ///     Serializes the <see cref="Storage" /> object into a string
    /// </summary>
    /// <returns>The serialized object</returns>
    public virtual string SerializeStorage()
    {
        return string.Empty;
    }

    /// <summary>
    ///     Deserializes the <see cref="Storage" /> object and sets it
    /// </summary>
    /// <param name="serialized">The serialized object</param>
    public virtual void DeserializeStorage(string serialized)
    {
    }

    #endregion
}
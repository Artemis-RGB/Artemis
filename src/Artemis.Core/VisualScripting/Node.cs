using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ninject;
using Ninject.Parameters;

namespace Artemis.Core;

/// <summary>
///     Represents a kind of node inside a <see cref="NodeScript" />
/// </summary>
public abstract class Node : CorePropertyChanged, INode
{
    /// <inheritdoc />
    public event EventHandler? Resetting;

    #region Properties & Fields
    
    private Guid _id;

    /// <inheritdoc />
    public Guid Id
    {
        get => _id;
        set => SetAndNotify(ref _id , value);
    }

    private string _name;

    /// <inheritdoc />
    public string Name
    {
        get => _name;
        protected set => SetAndNotify(ref _name, value);
    }

    private string _description;

    /// <inheritdoc />
    public string Description
    {
        get => _description;
        protected set => SetAndNotify(ref _description, value);
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
    public virtual bool IsExitNode => false;

    /// <inheritdoc />
    public virtual bool IsDefaultNode => false;

    private readonly List<IPin> _pins = new();

    /// <inheritdoc />
    public IReadOnlyCollection<IPin> Pins => new ReadOnlyCollection<IPin>(_pins);

    private readonly List<IPinCollection> _pinCollections = new();
    
    /// <inheritdoc />
    public IReadOnlyCollection<IPinCollection> PinCollections => new ReadOnlyCollection<IPinCollection>(_pinCollections);

    #endregion

    #region Construtors

    /// <summary>
    ///     Creates a new instance of the <see cref="Node" /> class with an empty name and description
    /// </summary>
    protected Node()
    {
        _name = string.Empty;
        _description = string.Empty;
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="Node" /> class with the provided name and description
    /// </summary>
    protected Node(string name, string description)
    {
        _name = name;
        _description = description;
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Creates a new input pin and adds it to the <see cref="Pins" /> collection
    /// </summary>
    /// <param name="name">The name of the pin</param>
    /// <typeparam name="T">The type of value the pin will hold</typeparam>
    /// <returns>The newly created pin</returns>
    protected InputPin<T> CreateInputPin<T>(string name = "")
    {
        InputPin<T> pin = new(this, name);
        _pins.Add(pin);
        OnPropertyChanged(nameof(Pins));
        return pin;
    }

    /// <summary>
    ///     Creates a new input pin and adds it to the <see cref="Pins" /> collection
    /// </summary>
    /// <param name="type">The type of value the pin will hold</param>
    /// <param name="name">The name of the pin</param>
    /// <returns>The newly created pin</returns>
    protected InputPin CreateInputPin(Type type, string name = "")
    {
        InputPin pin = new(this, type, name);
        _pins.Add(pin);
        OnPropertyChanged(nameof(Pins));
        return pin;
    }

    /// <summary>
    ///     Creates a new output pin and adds it to the <see cref="Pins" /> collection
    /// </summary>
    /// <param name="name">The name of the pin</param>
    /// <typeparam name="T">The type of value the pin will hold</typeparam>
    /// <returns>The newly created pin</returns>
    protected OutputPin<T> CreateOutputPin<T>(string name = "")
    {
        OutputPin<T> pin = new(this, name);
        _pins.Add(pin);
        OnPropertyChanged(nameof(Pins));
        return pin;
    }

    /// <summary>
    ///     Creates a new output pin and adds it to the <see cref="Pins" /> collection
    /// </summary>
    /// <param name="type">The type of value the pin will hold</param>
    /// <param name="name">The name of the pin</param>
    /// <returns>The newly created pin</returns>
    protected OutputPin CreateOutputPin(Type type, string name = "")
    {
        OutputPin pin = new(this, type, name);
        _pins.Add(pin);
        OnPropertyChanged(nameof(Pins));
        return pin;
    }

    /// <summary>
    ///     Removes the provided <paramref name="pin" /> from the node and it's <see cref="Pins" /> collection
    /// </summary>
    /// <param name="pin">The pin to remove</param>
    /// <returns><see langword="true" /> if the pin was removed; otherwise <see langword="false" />.</returns>
    protected bool RemovePin(Pin pin)
    {
        bool isRemoved = _pins.Remove(pin);
        if (isRemoved)
        {
            pin.DisconnectAll();
            OnPropertyChanged(nameof(Pins));
        }

        return isRemoved;
    }

    /// <summary>
    ///     Adds an existing <paramref name="pin"/> to the <see cref="Pins" /> collection.
    /// </summary>
    /// <param name="pin">The pin to add</param>
    protected void AddPin(Pin pin)
    {
        if (pin.Node != this)
            throw new ArtemisCoreException("Can't add a pin to a node that belongs to a different node than the one it's being added to.");
        if (_pins.Contains(pin))
            return;

        _pins.Add(pin);
        OnPropertyChanged(nameof(Pins));
    }

    /// <summary>
    ///     Creates a new input pin collection and adds it to the <see cref="PinCollections" /> collection
    /// </summary>
    /// <typeparam name="T">The type of value the pins of this collection will hold</typeparam>
    /// <param name="name">The name of the pin collection</param>
    /// <param name="initialCount">The amount of pins to initially add to the collection</param>
    /// <returns>The resulting input pin collection</returns>
    protected InputPinCollection<T> CreateInputPinCollection<T>(string name = "", int initialCount = 1)
    {
        InputPinCollection<T> pin = new(this, name, initialCount);
        _pinCollections.Add(pin);
        OnPropertyChanged(nameof(PinCollections));
        return pin;
    }

    /// <summary>
    ///     Creates a new input pin collection and adds it to the <see cref="PinCollections" /> collection
    /// </summary>
    /// <param name="type">The type of value the pins of this collection will hold</param>
    /// <param name="name">The name of the pin collection</param>
    /// <param name="initialCount">The amount of pins to initially add to the collection</param>
    /// <returns>The resulting input pin collection</returns>
    protected InputPinCollection CreateInputPinCollection(Type type, string name = "", int initialCount = 1)
    {
        InputPinCollection pin = new(this, type, name, initialCount);
        _pinCollections.Add(pin);
        OnPropertyChanged(nameof(PinCollections));
        return pin;
    }

    /// <summary>
    ///     Creates a new output pin collection and adds it to the <see cref="PinCollections" /> collection
    /// </summary>
    /// <typeparam name="T">The type of value the pins of this collection will hold</typeparam>
    /// <param name="name">The name of the pin collection</param>
    /// <param name="initialCount">The amount of pins to initially add to the collection</param>
    /// <returns>The resulting output pin collection</returns>
    protected OutputPinCollection<T> CreateOutputPinCollection<T>(string name = "", int initialCount = 1)
    {
        OutputPinCollection<T> pin = new(this, name, initialCount);
        _pinCollections.Add(pin);
        OnPropertyChanged(nameof(PinCollections));
        return pin;
    }

    /// <summary>
    ///     Removes the provided <paramref name="pinCollection" /> from the node and it's <see cref="PinCollections" />
    ///     collection
    /// </summary>
    /// <param name="pinCollection">The pin collection to remove</param>
    /// <returns><see langword="true" /> if the pin collection was removed; otherwise <see langword="false" />.</returns>
    protected bool RemovePinCollection(PinCollection pinCollection)
    {
        bool isRemoved = _pinCollections.Remove(pinCollection);
        if (isRemoved)
        {
            foreach (IPin pin in pinCollection)
                pin.DisconnectAll();
            OnPropertyChanged(nameof(PinCollections));
        }

        return isRemoved;
    }

    /// <inheritdoc />
    public virtual void Initialize(INodeScript script)
    {
    }

    /// <inheritdoc />
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

    /// <summary>
    ///     Called whenever the node must show it's custom view model, if <see langword="null" />, no custom view model is used
    /// </summary>
    /// <param name="nodeScript"></param>
    /// <returns>The custom view model, if <see langword="null" />, no custom view model is used</returns>
    public virtual ICustomNodeViewModel? GetCustomViewModel(NodeScript nodeScript)
    {
        return null;
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

/// <summary>
///     Represents a kind of node inside a <see cref="NodeScript" /> containing storage value of type
///     <typeparamref name="TStorage" />.
/// </summary>
/// <typeparam name="TStorage">The type of value the node stores</typeparam>
public abstract class Node<TStorage> : Node
{
    private TStorage? _storage;

    /// <inheritdoc />
    protected Node()
    {
    }

    /// <inheritdoc />
    protected Node(string name, string description) : base(name, description)
    {
    }

    /// <summary>
    ///     Gets or sets the storage object of this node, this is saved across sessions
    /// </summary>
    public TStorage? Storage
    {
        get => _storage;
        set
        {
            if (SetAndNotify(ref _storage, value))
                StorageModified?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    ///     Occurs whenever the storage of this node was modified.
    /// </summary>
    public event EventHandler? StorageModified;

    /// <inheritdoc />
    public override string SerializeStorage()
    {
        return CoreJson.SerializeObject(Storage, true);
    }

    /// <inheritdoc />
    public override void DeserializeStorage(string serialized)
    {
        Storage = CoreJson.DeserializeObject<TStorage>(serialized) ?? default(TStorage);
    }
}

/// <summary>
///     Represents a kind of node inside a <see cref="NodeScript" /> containing storage value of type
///     <typeparamref name="TStorage" /> and a view model of type <typeparamref name="TViewModel" />.
/// </summary>
/// <typeparam name="TStorage">The type of value the node stores</typeparam>
/// <typeparam name="TViewModel">The type of view model the node uses</typeparam>
public abstract class Node<TStorage, TViewModel> : Node<TStorage> where TViewModel : ICustomNodeViewModel
{
    /// <inheritdoc />
    protected Node()
    {
    }

    /// <inheritdoc />
    protected Node(string name, string description) : base(name, description)
    {
    }

    [Inject]
    internal IKernel Kernel { get; set; } = null!;

    /// <summary>
    ///     Called when a view model is required
    /// </summary>
    /// <param name="nodeScript"></param>
    public virtual TViewModel GetViewModel(NodeScript nodeScript)
    {
        return Kernel.Get<TViewModel>(new ConstructorArgument("node", this), new ConstructorArgument("script", nodeScript));
    }

    /// <param name="nodeScript"></param>
    /// <inheritdoc />
    public override ICustomNodeViewModel? GetCustomViewModel(NodeScript nodeScript)
    {
        return GetViewModel(nodeScript);
    }
}
using System;
using Artemis.Core;

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
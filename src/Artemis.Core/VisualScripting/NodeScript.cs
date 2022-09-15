using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core.Events;
using Artemis.Core.Internal;
using Artemis.Core.Properties;
using Artemis.Storage.Entities.Profile.Nodes;

namespace Artemis.Core;

/// <summary>
///     Represents a node script
/// </summary>
public abstract class NodeScript : CorePropertyChanged, INodeScript
{
    private void NodeTypeStoreOnNodeTypeAdded(object? sender, NodeTypeStoreEvent e)
    {
        if (Entity.Nodes.Any(n => e.TypeRegistration.MatchesEntity(n)))
            Load();
    }

    private void NodeTypeStoreOnNodeTypeRemoved(object? sender, NodeTypeStoreEvent e)
    {
        List<INode> nodes = Nodes.Where(n => n.GetType() == e.TypeRegistration.NodeData.Type).ToList();
        foreach (INode node in nodes)
            RemoveNode(node);
    }

    /// <inheritdoc />
    public event EventHandler<SingleValueEventArgs<INode>>? NodeAdded;

    /// <inheritdoc />
    public event EventHandler<SingleValueEventArgs<INode>>? NodeRemoved;

    #region Properties & Fields

    /// <summary>
    ///     Gets the entity used to store this script.
    /// </summary>
    public NodeScriptEntity Entity { get; private set; }

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public string Description { get; }

    private readonly List<INode> _nodes = new();

    /// <inheritdoc />
    public IEnumerable<INode> Nodes => new ReadOnlyCollection<INode>(_nodes);

    /// <summary>
    ///     Gets or sets the exit node of the script
    /// </summary>
    protected INode ExitNode { get; set; }

    /// <summary>
    ///     Gets a boolean indicating whether the exit node is connected to any other nodes
    /// </summary>
    public abstract bool ExitNodeConnected { get; }

    /// <inheritdoc />
    public abstract Type ResultType { get; }

    /// <inheritdoc />
    public object? Context { get; set; }

    #endregion

    #region Constructors

    /// <summary>
    ///     Creates a new instance of the <see cref="NodeScript" /> class with a name, description and optional context
    /// </summary>
    /// <param name="name">The name of the node script</param>
    /// <param name="description">The description of the node script</param>
    /// <param name="context">
    ///     The context of the node script, usually a <see cref="Profile" /> or
    ///     <see cref="ProfileConfiguration" />
    /// </param>
    protected NodeScript(string name, string description, object? context = null)
    {
        Name = name;
        Description = description;
        Context = context;
        Entity = new NodeScriptEntity();
        ExitNode = null!;

        NodeTypeStore.NodeTypeAdded += NodeTypeStoreOnNodeTypeAdded;
        NodeTypeStore.NodeTypeRemoved += NodeTypeStoreOnNodeTypeRemoved;
    }

    internal NodeScript(string name, string description, NodeScriptEntity entity, object? context = null)
    {
        Name = name;
        Description = description;
        Entity = entity;
        Context = context;
        ExitNode = null!;

        NodeTypeStore.NodeTypeAdded += NodeTypeStoreOnNodeTypeAdded;
        NodeTypeStore.NodeTypeRemoved += NodeTypeStoreOnNodeTypeRemoved;
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public void Run()
    {
        lock (_nodes)
        {
            foreach (INode node in _nodes)
                node.Reset();
        }

        ExitNode.TryEvaluate();
    }

    /// <inheritdoc />
    public void AddNode(INode node)
    {
        lock (_nodes)
        {
            _nodes.Add(node);
        }

        NodeAdded?.Invoke(this, new SingleValueEventArgs<INode>(node));
    }

    /// <inheritdoc />
    public void RemoveNode(INode node)
    {
        lock (_nodes)
        {
            _nodes.Remove(node);
        }

        NodeRemoved?.Invoke(this, new SingleValueEventArgs<INode>(node));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        NodeTypeStore.NodeTypeAdded -= NodeTypeStoreOnNodeTypeAdded;
        NodeTypeStore.NodeTypeRemoved -= NodeTypeStoreOnNodeTypeRemoved;

        lock (_nodes)
        {
            foreach (INode node in _nodes)
            {
                if (node is IDisposable disposable)
                    disposable.Dispose();
            }
        }
    }

    #endregion

    #region Implementation of IStorageModel

    /// <inheritdoc />
    public void Load()
    {
        lock (_nodes)
        {
            // Remove nodes no longer on the entity
            List<INode> removeNodes = _nodes.Where(n => Entity.Nodes.All(e => e.Id != n.Id)).ToList();
            foreach (INode removeNode in removeNodes)
            {
                RemoveNode(removeNode);
                if (removeNode is IDisposable disposable)
                    disposable.Dispose();
            }
        }

        // Create missing nodes nodes
        foreach (NodeEntity nodeEntity in Entity.Nodes)
        {
            INode? node = Nodes.FirstOrDefault(n => n.Id == nodeEntity.Id);
            // If the node already exists, apply the entity to it
            if (node != null)
            {
                LoadExistingNode(node, nodeEntity);
            }
            else
            {
                INode? loaded = LoadNode(nodeEntity);
                if (loaded != null)
                    AddNode(loaded);
            }
        }

        LoadConnections();
    }

    internal void LoadFromEntity(NodeScriptEntity entity)
    {
        Entity = entity;
        Load();
    }

    private void LoadExistingNode(INode node, NodeEntity nodeEntity)
    {
        node.Id = nodeEntity.Id;
        node.X = nodeEntity.X;
        node.Y = nodeEntity.Y;

        // Restore pin collections
        foreach (NodePinCollectionEntity entityNodePinCollection in nodeEntity.PinCollections)
        {
            IPinCollection? collection = node.PinCollections.ElementAtOrDefault(entityNodePinCollection.Id);
            if (collection == null)
                continue;

            while (collection.Count() > entityNodePinCollection.Amount)
                collection.Remove(collection.Last());
            while (collection.Count() < entityNodePinCollection.Amount)
                collection.Add(collection.CreatePin());
        }
    }

    private INode? LoadNode(NodeEntity nodeEntity)
    {
        NodeTypeRegistration? nodeTypeRegistration = NodeTypeStore.Get(nodeEntity);
        if (nodeTypeRegistration == null)
            return null;

        // Create the node
        INode node = nodeTypeRegistration.NodeData.CreateNode(this, nodeEntity);
        LoadExistingNode(node, nodeEntity);
        return node;
    }

    /// <inheritdoc />
    public void LoadConnections()
    {
        List<INode> nodes = Nodes.ToList();
        foreach (NodeConnectionEntity nodeConnectionEntity in Entity.Connections.OrderBy(p => p.SourcePinCollectionId))
        {
            INode? source = nodes.FirstOrDefault(n => n.Id == nodeConnectionEntity.SourceNode);
            if (source == null)
                continue;
            INode? target = nodes.FirstOrDefault(n => n.Id == nodeConnectionEntity.TargetNode);
            if (target == null)
                continue;

            IPin? sourcePin = nodeConnectionEntity.SourcePinCollectionId == -1
                ? source.Pins.ElementAtOrDefault(nodeConnectionEntity.SourcePinId)
                : source.PinCollections.ElementAtOrDefault(nodeConnectionEntity.SourcePinCollectionId)?.ElementAtOrDefault(nodeConnectionEntity.SourcePinId);
            IPin? targetPin = nodeConnectionEntity.TargetPinCollectionId == -1
                ? target.Pins.ElementAtOrDefault(nodeConnectionEntity.TargetPinId)
                : target.PinCollections.ElementAtOrDefault(nodeConnectionEntity.TargetPinCollectionId)?.ElementAtOrDefault(nodeConnectionEntity.TargetPinId);

            // Ensure both nodes have the required pins
            if (sourcePin == null || targetPin == null)
                continue;
            // Ensure the connection is valid
            if (sourcePin.Direction == targetPin.Direction)
                continue;

            // Clear existing connections on input pins, we don't want none of that now
            if (targetPin.Direction == PinDirection.Input)
                while (targetPin.ConnectedTo.Any())
                    targetPin.DisconnectFrom(targetPin.ConnectedTo[0]);

            if (sourcePin.Direction == PinDirection.Input)
                while (sourcePin.ConnectedTo.Any())
                    sourcePin.DisconnectFrom(sourcePin.ConnectedTo[0]);

            // Only connect the nodes if they aren't already connected (LoadConnections may be called twice or more)
            // Type checking is done later when all connections are in place
            if (!targetPin.ConnectedTo.Contains(sourcePin))
                targetPin.ConnectTo(sourcePin);
            if (!sourcePin.ConnectedTo.Contains(targetPin))
                sourcePin.ConnectTo(targetPin);
        }

        // With all connections restored, ensure types match (connecting pins may affect types so the check is done afterwards)
        foreach (INode node in nodes)
        {
            foreach (IPin nodePin in node.Pins.Concat(node.PinCollections.SelectMany(p => p)))
            {
                List<IPin> toDisconnect = nodePin.ConnectedTo.Where(c => !c.IsTypeCompatible(nodePin.Type)).ToList();
                foreach (IPin pin in toDisconnect)
                    pin.DisconnectFrom(nodePin);
            }
        }
    }

    /// <inheritdoc />
    public void Save()
    {
        Entity.Name = Name;
        Entity.Description = Description;
        Entity.Nodes.Clear();

        foreach (INode node in Nodes)
        {
            NodeEntity nodeEntity = new()
            {
                Id = node.Id,
                PluginId = NodeTypeStore.GetPlugin(node)?.Guid ?? Constants.CorePlugin.Guid,
                Type = node.GetType().Name,
                X = node.X,
                Y = node.Y,
                Name = node.Name,
                Description = node.Description,
                IsExitNode = node.IsExitNode
            };

            if (node is Node nodeImplementation)
                nodeEntity.Storage = nodeImplementation.SerializeStorage();

            int collectionId = 0;
            foreach (IPinCollection nodePinCollection in node.PinCollections)
            {
                nodeEntity.PinCollections.Add(new NodePinCollectionEntity
                {
                    Id = collectionId,
                    Direction = (int) nodePinCollection.Direction,
                    Amount = nodePinCollection.Count()
                });
                collectionId++;
            }

            Entity.Nodes.Add(nodeEntity);
        }

        // Store connections
        Entity.Connections.Clear();
        foreach (INode node in Nodes)
        {
            SavePins(node, -1, node.Pins);

            int pinCollectionId = 0;
            foreach (IPinCollection pinCollection in node.PinCollections)
            {
                SavePins(node, pinCollectionId, pinCollection);
                pinCollectionId++;
            }
        }
    }

    private void SavePins(INode node, int collectionId, IEnumerable<IPin> pins)
    {
        int sourcePinId = 0;
        foreach (IPin sourcePin in pins)
        {
            if (sourcePin.Direction == PinDirection.Output)
            {
                sourcePinId++;
                continue;
            }

            foreach (IPin targetPin in sourcePin.ConnectedTo)
            {
                int targetPinCollectionId = -1;
                int targetPinId;

                IPinCollection? targetCollection = targetPin.Node.PinCollections.FirstOrDefault(c => c.Contains(targetPin));
                if (targetCollection != null)
                {
                    targetPinCollectionId = targetPin.Node.PinCollections.IndexOf(targetCollection);
                    targetPinId = targetCollection.ToList().IndexOf(targetPin);
                }
                else
                {
                    targetPinId = targetPin.Node.Pins.IndexOf(targetPin);
                }

                Entity.Connections.Add(new NodeConnectionEntity
                {
                    SourceType = sourcePin.Type.Name,
                    SourceNode = node.Id,
                    SourcePinCollectionId = collectionId,
                    SourcePinId = sourcePinId,
                    TargetType = targetPin.Type.Name,
                    TargetNode = targetPin.Node.Id,
                    TargetPinCollectionId = targetPinCollectionId,
                    TargetPinId = targetPinId
                });
            }

            sourcePinId++;
        }
    }

    #endregion
}

/// <summary>
///     Represents a node script with a result value of type <typeparamref name="T" />
/// </summary>
/// <typeparam name="T">The type of result value</typeparam>
public class NodeScript<T> : NodeScript, INodeScript<T>
{
    #region Properties & Fields

    /// <inheritdoc />
    public T? Result => ((ExitNode<T>) ExitNode).Value;

    /// <inheritdoc />
    public override bool ExitNodeConnected => ((ExitNode<T>) ExitNode).Input.ConnectedTo.Any();

    /// <inheritdoc />
    public override Type ResultType => typeof(T);

    #endregion

    #region Constructors

    /// <inheritdoc />
    public NodeScript(string name, string description, NodeScriptEntity entity, object? context = null)
        : base(name, description, entity, context)
    {
        ExitNode = new ExitNode<T>(name, description);
        AddNode(ExitNode);

        Load();
    }

    /// <inheritdoc />
    public NodeScript(string name, string description, object? context = null)
        : base(name, description, context)
    {
        ExitNode = new ExitNode<T>(name, description);
        AddNode(ExitNode);

        Save();
    }

    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
    private NodeScript(string name, string description, INode exitNode)
        : base(name, description)
    {
        ExitNode = exitNode;
        AddNode(ExitNode);
    }

    #endregion
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core.Events;
using Artemis.Core.Internal;
using Artemis.Core.Properties;
using Artemis.Storage.Entities.Profile.Nodes;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a node script
    /// </summary>
    public abstract class NodeScript : CorePropertyChanged, INodeScript, IStorageModel
    {
        private void NodeTypeStoreOnNodeTypeChanged(object? sender, NodeTypeStoreEvent e)
        {
            Load();
        }

        /// <inheritdoc />
        public event EventHandler<SingleValueEventArgs<INode>>? NodeAdded;

        /// <inheritdoc />
        public event EventHandler<SingleValueEventArgs<INode>>? NodeRemoved;

        #region Properties & Fields

        internal NodeScriptEntity Entity { get; }

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

        public NodeScript(string name, string description, object? context = null)
        {
            Name = name;
            Description = description;
            Context = context;
            Entity = new NodeScriptEntity();

            NodeTypeStore.NodeTypeAdded += NodeTypeStoreOnNodeTypeChanged;
            NodeTypeStore.NodeTypeRemoved += NodeTypeStoreOnNodeTypeChanged;
        }

        internal NodeScript(string name, string description, NodeScriptEntity entity, object? context = null)
        {
            Name = name;
            Description = description;
            Entity = entity;
            Context = context;

            NodeTypeStore.NodeTypeAdded += NodeTypeStoreOnNodeTypeChanged;
            NodeTypeStore.NodeTypeRemoved += NodeTypeStoreOnNodeTypeChanged;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public void Run()
        {
            foreach (INode node in Nodes)
                node.Reset();

            ExitNode.Evaluate();
        }

        /// <inheritdoc />
        public void AddNode(INode node)
        {
            _nodes.Add(node);

            NodeAdded?.Invoke(this, new SingleValueEventArgs<INode>(node));
        }

        /// <inheritdoc />
        public void RemoveNode(INode node)
        {
            _nodes.Remove(node);

            if (node is IDisposable disposable)
                disposable.Dispose();

            NodeRemoved?.Invoke(this, new SingleValueEventArgs<INode>(node));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            NodeTypeStore.NodeTypeAdded -= NodeTypeStoreOnNodeTypeChanged;
            NodeTypeStore.NodeTypeRemoved -= NodeTypeStoreOnNodeTypeChanged;

            foreach (INode node in _nodes)
            {
                if (node is IDisposable disposable)
                    disposable.Dispose();
            }
        }

        #endregion

        #region Implementation of IStorageModel

        /// <inheritdoc />
        public void Load()
        {
            List<INode> removeNodes = _nodes.Where(n => !n.IsExitNode).ToList();
            foreach (INode removeNode in removeNodes)
                RemoveNode(removeNode);

            // Create nodes
            foreach (NodeEntity entityNode in Entity.Nodes)
            {
                INode? node = LoadNode(entityNode, entityNode.IsExitNode ? ExitNode : null);
                if (node == null)
                    continue;

                if (!entityNode.IsExitNode)
                    AddNode(node);
            }

            LoadConnections();
        }

        private INode? LoadNode(NodeEntity nodeEntity, INode? node)
        {
            if (node == null)
            {
                NodeTypeRegistration? nodeTypeRegistration = NodeTypeStore.Get(nodeEntity.PluginId, nodeEntity.Type);
                if (nodeTypeRegistration == null)
                    return null;

                // Create the node
                node = nodeTypeRegistration.NodeData.CreateNode(this, nodeEntity);
            }
            else
            {
                node.X = nodeEntity.X;
                node.Y = nodeEntity.Y;
            }

            // Restore pin collections
            foreach (NodePinCollectionEntity entityNodePinCollection in nodeEntity.PinCollections)
            {
                IPinCollection? collection = node.PinCollections.ElementAtOrDefault(entityNodePinCollection.Id);
                if (collection == null)
                    continue;

                while (collection.Count() < entityNodePinCollection.Amount)
                    collection.AddPin();
            }

            return node;
        }

        /// <summary>
        ///     Loads missing connections between the nodes of this node script from the <see cref="Entity" />
        /// </summary>
        public void LoadConnections()
        {
            List<INode> nodes = Nodes.ToList();
            foreach (NodeConnectionEntity nodeConnectionEntity in Entity.Connections.OrderBy(p => p.SourcePinCollectionId))
            {
                INode? source = nodes.ElementAtOrDefault(nodeConnectionEntity.SourceNode);
                if (source == null)
                    continue;
                INode? target = nodes.ElementAtOrDefault(nodeConnectionEntity.TargetNode);
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
                if (!targetPin.ConnectedTo.Contains(sourcePin))
                    targetPin.ConnectTo(sourcePin);
                if (!sourcePin.ConnectedTo.Contains(targetPin))
                    sourcePin.ConnectTo(targetPin);
            }
        }

        /// <inheritdoc />
        public void Save()
        {
            Entity.Name = Name;
            Entity.Description = Description;

            Entity.Nodes.Clear();

            // No need to save the exit node if that's all there is
            if (Nodes.Count() == 1)
                return;

            int id = 0;
            foreach (INode node in Nodes)
            {
                NodeEntity nodeEntity = new()
                {
                    Id = id,
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
                id++;
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
            List<INode> nodes = Nodes.ToList();
            foreach (IPin sourcePin in pins.Where(p => p.Direction == PinDirection.Input))
            {
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
                        SourceNode = nodes.IndexOf(node),
                        SourcePinCollectionId = collectionId,
                        SourcePinId = sourcePinId,
                        TargetType = targetPin.Type.Name,
                        TargetNode = nodes.IndexOf(targetPin.Node),
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
    ///     Represents a node script with a result value of type <paramref name="T" />
    /// </summary>
    /// <typeparam name="T">The type of result value</typeparam>
    public class NodeScript<T> : NodeScript, INodeScript<T>
    {
        #region Properties & Fields

        /// <inheritdoc />
        public T Result => ((ExitNode<T>) ExitNode).Value;

        /// <inheritdoc />
        public override bool ExitNodeConnected => ((ExitNode<T>) ExitNode).Input.ConnectedTo.Any();

        /// <inheritdoc />
        public override Type ResultType => typeof(T);

        #endregion

        #region Constructors

        internal NodeScript(string name, string description, NodeScriptEntity entity, object? context = null)
            : base(name, description, entity, context)
        {
            ExitNode = new ExitNode<T>(name, description);
            AddNode(ExitNode);

            Load();
        }

        public NodeScript(string name, string description, object? context = null)
            : base(name, description, context)
        {
            ExitNode = new ExitNode<T>(name, description);
            AddNode(ExitNode);
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
}
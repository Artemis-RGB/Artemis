using System;
using System.Collections.Generic;
using System.Reflection;
using Artemis.Storage.Entities.Profile.Nodes;
using Newtonsoft.Json;
using Ninject;
using Ninject.Parameters;
using SkiaSharp;

namespace Artemis.Core.Services
{
    internal class NodeService : INodeService
    {
        #region Constants

        private static readonly Type TYPE_NODE = typeof(INode);

        #endregion

        private readonly IKernel _kernel;

        #region Constructors

        public NodeService(IKernel kernel)
        {
            _kernel = kernel;
        }

        #endregion

        #region Properties & Fields

        public IEnumerable<NodeData> AvailableNodes => NodeTypeStore.GetAll();

        #endregion

        #region Methods

        /// <inheritdoc />
        public TypeColorRegistration? GetTypeColor(Type type)
        {
            return NodeTypeStore.GetColor(type);
        }

        public NodeTypeRegistration RegisterNodeType(Plugin plugin, Type nodeType)
        {
            if (plugin == null) throw new ArgumentNullException(nameof(plugin));
            if (nodeType == null) throw new ArgumentNullException(nameof(nodeType));

            if (!TYPE_NODE.IsAssignableFrom(nodeType)) throw new ArgumentException("Node has to be a base type of the Node-Type.", nameof(nodeType));

            NodeAttribute? nodeAttribute = nodeType.GetCustomAttribute<NodeAttribute>();
            string name = nodeAttribute?.Name ?? nodeType.Name;
            string description = nodeAttribute?.Description ?? string.Empty;
            string category = nodeAttribute?.Category ?? string.Empty;

            NodeData nodeData = new(plugin, nodeType, name, description, category, (s, e) => CreateNode(s, e, nodeType));
            return NodeTypeStore.Add(nodeData);
        }

        public TypeColorRegistration RegisterTypeColor(Plugin plugin, Type type, SKColor color)
        {
            if (plugin == null) throw new ArgumentNullException(nameof(plugin));
            if (type == null) throw new ArgumentNullException(nameof(type));

            return NodeTypeStore.AddColor(type, color, plugin);
        }

        private INode CreateNode(INodeScript script, NodeEntity? entity, Type nodeType)
        {
            INode node = _kernel.Get(nodeType) as INode ?? throw new InvalidOperationException($"Node {nodeType} is not an INode");

            if (entity != null)
            {
                node.X = entity.X;
                node.Y = entity.Y;
                try
                {
                    node.Storage = CoreJson.DeserializeObject(entity.Storage, true);
                }
                catch
                {
                    // ignored
                }
            }

            if (node is CustomViewModelNode customViewModelNode)
                customViewModelNode.BaseCustomViewModel = _kernel.Get(customViewModelNode.CustomViewModelType, new ConstructorArgument("node", node));

            node.Initialize(script);
            return node;
        }

        #endregion
    }

    /// <summary>
    ///     A service that provides access to the node system
    /// </summary>
    public interface INodeService : IArtemisService
    {
        /// <summary>
        ///     Gets all available nodes
        /// </summary>
        IEnumerable<NodeData> AvailableNodes { get; }

        /// <summary>
        /// Gets the best matching registration for the provided type
        /// </summary>
        TypeColorRegistration? GetTypeColor(Type type);

        /// <summary>
        ///     Registers a node of the provided <paramref name="nodeType" />
        /// </summary>
        /// <param name="plugin">The plugin the node belongs to</param>
        /// <param name="nodeType">The type of node to initialize</param>
        NodeTypeRegistration RegisterNodeType(Plugin plugin, Type nodeType);

        /// <summary>
        ///     Registers a type with a provided color for use in the node editor
        /// </summary>
        /// <param name="plugin">The plugin making the registration</param>
        /// <param name="type">The type to associate the color with</param>
        /// <param name="color">The color to display</param>
        TypeColorRegistration RegisterTypeColor(Plugin plugin, Type type, SKColor color);
    }
}
using System;
using System.Collections.Generic;
using System.Reflection;
using Ninject;

namespace Artemis.Core.Services
{
    internal class NodeService : INodeService
    {
        private readonly IKernel _kernel;

        #region Constants

        private static readonly Type TYPE_NODE = typeof(INode);

        #endregion

        #region Properties & Fields

        public IEnumerable<NodeData> AvailableNodes => NodeTypeStore.GetAll();

        #endregion

        #region Constructors

        public NodeService(IKernel kernel)
        {
            _kernel = kernel;
        }

        #endregion

        #region Methods

        public NodeTypeRegistration RegisterNodeType(Plugin plugin, Type nodeType)
        {
            if (plugin == null) throw new ArgumentNullException(nameof(plugin));
            if (nodeType == null) throw new ArgumentNullException(nameof(nodeType));

            if (!TYPE_NODE.IsAssignableFrom(nodeType)) throw new ArgumentException("Node has to be a base type of the Node-Type.", nameof(nodeType));

            NodeAttribute? nodeAttribute = nodeType.GetCustomAttribute<NodeAttribute>();
            string name = nodeAttribute?.Name ?? nodeType.Name;
            string description = nodeAttribute?.Description ?? string.Empty;
            string category = nodeAttribute?.Category ?? string.Empty;

            NodeData nodeData = new(plugin, nodeType, name, description, category, () => CreateNode(nodeType));
            return NodeTypeStore.Add(nodeData);
        }

        private INode CreateNode(Type nodeType)
        {
            INode node = _kernel.Get(nodeType) as INode ?? throw new InvalidOperationException($"Node {nodeType} is not an INode");
            if (node is CustomViewModelNode customViewModelNode)
                customViewModelNode.BaseCustomViewModel = _kernel.Get(customViewModelNode.CustomViewModelType);
            return node;
        }

        #endregion
    }

    /// <summary>
    /// A service that provides access to the node system
    /// </summary>
    public interface INodeService : IArtemisService
    {
        /// <summary>
        /// Gets all available nodes
        /// </summary>
        IEnumerable<NodeData> AvailableNodes { get; }

        /// <summary>
        /// Initializes a node of the provided <paramref name="nodeType"/>
        /// </summary>
        /// <param name="plugin">The plugin the node belongs to</param>
        /// <param name="nodeType">The type of node to initialize</param>
        NodeTypeRegistration RegisterNodeType(Plugin plugin, Type nodeType);
    }
}
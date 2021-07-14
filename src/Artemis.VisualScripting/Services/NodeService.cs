using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Artemis.Core.VisualScripting;
using Artemis.VisualScripting.Attributes;
using Artemis.VisualScripting.Model;

namespace Artemis.VisualScripting.Services
{
    //TODO DarthAffe 09.07.2021: Make this Static?
    public class NodeService
    {
        #region Constants

        private static readonly Type TYPE_NODE = typeof(INode);

        #endregion

        #region Properties & Fields

        private Dictionary<Type, NodeData> _nodeData { get; } = new();
        public IReadOnlyDictionary<Type, NodeData> NodeData => new ReadOnlyDictionary<Type, NodeData>(_nodeData);
        public IEnumerable<NodeData> AvailableNodes => _nodeData.Values;

        #endregion

        #region Methods

        public void InitializeNodes()
        {
            foreach (Type nodeType in typeof(NodeService).Assembly.GetTypes().Where(t => typeof(INode).IsAssignableFrom(t) && t.IsPublic && !t.IsAbstract && !t.IsInterface))
                InitializeNode(nodeType);
        }

        public void InitializeNode<T>()
            where T : INode
            => InitializeNode(typeof(T));

        public void InitializeNode(Type nodeType)
        {
            if (!TYPE_NODE.IsAssignableFrom(nodeType)) throw new ArgumentException("Node has to be a base type of the Node-Type.", nameof(nodeType));
            if (_nodeData.ContainsKey(nodeType)) return;

            UIAttribute uiAttribute = nodeType.GetCustomAttribute<UIAttribute>();
            string name = uiAttribute?.Name ?? nodeType.Name;
            string description = uiAttribute?.Description ?? string.Empty;
            string category = uiAttribute?.Category ?? string.Empty;

            NodeData nodeData = new(nodeType, name, description, category, () => CreateNode(nodeType));
            _nodeData.Add(nodeType, nodeData);
        }

        private INode CreateNode(Type nodeType) => Activator.CreateInstance(nodeType) as INode;

        #endregion
    }
}

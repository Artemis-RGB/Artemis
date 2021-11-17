using System;
using Artemis.Storage.Entities.Profile.Nodes;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents node data describing a certain <see cref="INode" />
    /// </summary>
    public class NodeData
    {
        #region Constructors

        internal NodeData(Plugin plugin, Type type, string name, string description, string category, Type? inputType, Type? outputType, Func<INodeScript, NodeEntity?, INode> create)
        {
            Plugin = plugin;
            Type = type;
            Name = name;
            Description = description;
            Category = category;
            InputType = inputType;
            OutputType = outputType;
            _create = create;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Creates a new instance of the node this data represents
        /// </summary>
        /// <param name="script">The script to create the node for</param>
        /// <param name="entity">An optional storage entity to apply to the node</param>
        /// <returns>The returning node of type <see cref="Type" /></returns>
        public INode CreateNode(INodeScript script, NodeEntity? entity)
        {
            return _create(script, entity);
        }

        #endregion

        #region Properties & Fields

        /// <summary>
        ///     Gets the plugin that provided this node data
        /// </summary>
        public Plugin Plugin { get; }

        /// <summary>
        ///     Gets the type of <see cref="INode" /> this data represents
        /// </summary>
        public Type Type { get; }

        /// <summary>
        ///     Gets the name of the node this data represents
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets the description of the node this data represents
        /// </summary>
        public string Description { get; }

        /// <summary>
        ///     Gets the category of the node this data represents
        /// </summary>
        public string Category { get; }

        /// <summary>
        ///     Gets the primary input type of the node this data represents
        /// </summary>
        public Type? InputType { get; }

        /// <summary>
        ///     Gets the primary output of the node this data represents
        /// </summary>
        public Type? OutputType { get; }

        private readonly Func<INodeScript, NodeEntity?, INode> _create;

        #endregion
    }
}
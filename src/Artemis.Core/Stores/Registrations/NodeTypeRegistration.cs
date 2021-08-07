using System;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a registration for a type of <see cref="INode"/>
    /// </summary>
    public class NodeTypeRegistration
    {
        internal NodeTypeRegistration(NodeData nodeData, Plugin plugin)
        {
            NodeData = nodeData;
            Plugin = plugin;

            Plugin.Disabled += OnDisabled;
        }

        public NodeData NodeData { get; }

        /// <summary>
        ///     Gets the plugin the node is associated with
        /// </summary>
        public Plugin Plugin { get; }
        
        /// <summary>
        ///     Gets a boolean indicating whether the registration is in the internal Core store
        /// </summary>
        public bool IsInStore { get; internal set; }

        private void OnDisabled(object? sender, EventArgs e)
        {
            Plugin.Disabled -= OnDisabled;
            if (IsInStore)
                NodeTypeStore.Remove(this);
        }
    }
}
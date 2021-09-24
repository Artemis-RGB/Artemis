using System;
using System.Collections.Generic;
using System.ComponentModel;
using Artemis.Core.Events;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a node script
    /// </summary>
    public interface INodeScript : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        ///     Gets the name of the node script.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Gets the description of the node script.
        /// </summary>
        string Description { get; }

        /// <summary>
        ///     Gets an enumerable of all the nodes on this script.
        /// </summary>
        IEnumerable<INode> Nodes { get; }

        /// <summary>
        ///     Gets the return type of the node script.
        /// </summary>
        Type ResultType { get; }

        /// <summary>
        ///     Gets or sets the context of the node script, usually a <see cref="Profile" /> or
        ///     <see cref="ProfileConfiguration" />.
        /// </summary>
        object? Context { get; set; }

        /// <summary>
        ///     Occurs whenever a node was added to the script
        /// </summary>
        event EventHandler<SingleValueEventArgs<INode>>? NodeAdded;

        /// <summary>
        ///     Occurs whenever a node was removed from the script
        /// </summary>
        event EventHandler<SingleValueEventArgs<INode>>? NodeRemoved;

        /// <summary>
        ///     Runs the script, evaluating nodes where needed
        /// </summary>
        void Run();

        /// <summary>
        ///     Adds a node to the script
        /// </summary>
        /// <param name="node">The node to add</param>
        void AddNode(INode node);

        /// <summary>
        ///     Removes a node from the script
        /// </summary>
        /// <param name="node">The node to remove</param>
        void RemoveNode(INode node);
    }

    /// <summary>
    ///     Represents a node script with a result value of type <paramref name="T" />
    /// </summary>
    /// <typeparam name="T">The type of result value</typeparam>
    public interface INodeScript<out T> : INodeScript
    {
        /// <summary>
        ///     Gets the result of the script
        /// </summary>
        T Result { get; }
    }
}
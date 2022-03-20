using System;
using System.Collections.Generic;
using Artemis.Core.Events;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a collection of <see cref="IPin" />s on a <see cref="INode" />
    /// </summary>
    public interface IPinCollection : IEnumerable<IPin>
    {
        /// <summary>
        ///     Gets the node the pin collection belongs to
        /// </summary>
        INode Node { get; }

        /// <summary>
        ///     Gets the name of the pin collection
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Gets the direction of the pin collection and all its pins
        /// </summary>
        PinDirection Direction { get; }

        /// <summary>
        ///     Gets the type of values the pin collection and all its pins holds
        /// </summary>
        Type Type { get; }

        /// <summary>
        ///     Occurs when a pin was added to the collection
        /// </summary>
        event EventHandler<SingleValueEventArgs<IPin>> PinAdded;

        /// <summary>
        ///     Occurs when a pin was removed from the collection
        /// </summary>
        event EventHandler<SingleValueEventArgs<IPin>> PinRemoved;

        /// <summary>
        /// Creates a new pin compatible with this collection
        /// </summary>
        /// <returns>The newly created pin</returns>
        IPin CreatePin();

        /// <summary>
        ///     Adds the provided <paramref name="pin" /> to the collection
        /// </summary>
        void Add(IPin pin);

        /// <summary>
        ///     Removes the provided <paramref name="pin" /> from the collection
        /// </summary>
        /// <param name="pin">The pin to remove</param>
        bool Remove(IPin pin);

        /// <summary>
        ///     Resets the pin collection, causing its pins to re-evaluate the next time its value is requested
        /// </summary>
        void Reset();
    }
}
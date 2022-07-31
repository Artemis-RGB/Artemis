using System;
using System.Collections.Generic;
using Artemis.Core.Events;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a pin containing a value on a <see cref="INode" />
    /// </summary>
    public interface IPin
    {
        /// <summary>
        ///     Gets the node the pin belongs to
        /// </summary>
        INode Node { get; }

        /// <summary>
        ///     Gets or sets the name of the pin
        /// </summary>
        string Name { get; set; }

        /// <summary>
        ///     Gets the direction of the pin
        /// </summary>
        PinDirection Direction { get; }

        /// <summary>
        ///     Gets the type of value the pin holds
        /// </summary>
        Type Type { get; }

        /// <summary>
        ///     Gets a boolean indicating whether the type of this pin is numeric.
        /// </summary>
        bool IsNumeric { get;  }

        /// <summary>
        ///     Gets the value the pin holds
        /// </summary>
        object? PinValue { get; }

        /// <summary>
        ///     Gets a read only list of pins this pin is connected to
        /// </summary>
        IReadOnlyList<IPin> ConnectedTo { get; }

        /// <summary>
        ///     Gets or sets a boolean indicating whether this pin is evaluated or not
        /// </summary>
        bool IsEvaluated { get; set; }

        /// <summary>
        ///     Occurs when the pin connects to another pin
        /// </summary>
        event EventHandler<SingleValueEventArgs<IPin>> PinConnected;

        /// <summary>
        ///     Occurs when the pin disconnects from another pin
        /// </summary>
        event EventHandler<SingleValueEventArgs<IPin>> PinDisconnected;

        /// <summary>
        ///     Resets the pin, causing it to re-evaluate the next time its value is requested
        /// </summary>
        void Reset();

        /// <summary>
        ///     Connects the pin to the provided <paramref name="pin"></paramref>
        /// </summary>
        /// <param name="pin">The pin to connect this pin to</param>
        void ConnectTo(IPin pin);

        /// <summary>
        ///     Disconnects the pin to the provided <paramref name="pin"></paramref>
        /// </summary>
        /// <param name="pin">The pin to disconnect this pin to</param>
        void DisconnectFrom(IPin pin);

        /// <summary>
        ///     Disconnects all pins this pin is connected to
        /// </summary>
        void DisconnectAll();

        /// <summary>
        ///     Determines whether this pin is compatible with the given type
        /// </summary>
        /// <param name="type">The type to check for compatibility</param>
        /// <returns><see langword="true"/> if the type is compatible, otherwise <see langword="false"/>.</returns>
        public bool IsTypeCompatible(Type type);
    }
}
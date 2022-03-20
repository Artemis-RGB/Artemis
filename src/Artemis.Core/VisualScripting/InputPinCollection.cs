using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a collection of input pins containing values of type <typeparamref name="T" />
    /// </summary>
    /// <typeparam name="T">The type of value the pins in this collection hold</typeparam>
    public sealed class InputPinCollection<T> : PinCollection
    {
        #region Constructors

        internal InputPinCollection(INode node, string name, int initialCount)
            : base(node, name)
        {
            // Can't do this in the base constructor because the type won't be set yet
            for (int i = 0; i < initialCount; i++)
                Add(CreatePin());
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override IPin CreatePin()
        {
            return new InputPin<T>(Node, string.Empty);
        }

        #endregion

        #region Properties & Fields

        /// <inheritdoc />
        public override PinDirection Direction => PinDirection.Input;

        /// <inheritdoc />
        public override Type Type => typeof(T);

        /// <summary>
        ///     Gets an enumerable of the pins in this collection
        /// </summary>
        public new IEnumerable<InputPin<T>> Pins => base.Pins.Cast<InputPin<T>>();

        /// <summary>
        ///     Gets an enumerable of the values of the pins in this collection
        /// </summary>
        public IEnumerable<T> Values => Pins.Where(p => p.Value != null).Select(p => p.Value!);

        #endregion
    }

    /// <summary>
    ///     Represents a collection of input pins
    /// </summary>
    public sealed class InputPinCollection : PinCollection
    {
        #region Constructors

        internal InputPinCollection(INode node, Type type, string name, int initialCount)
            : base(node, name)
        {
            Type = type;

            // Can't do this in the base constructor because the type won't be set yet
            for (int i = 0; i < initialCount; i++)
                Add(CreatePin());
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override IPin CreatePin()
        {
            return new InputPin(Node, Type, string.Empty);
        }

        #endregion

        #region Properties & Fields

        /// <inheritdoc />
        public override PinDirection Direction => PinDirection.Input;

        /// <inheritdoc />
        public override Type Type { get; }

        /// <summary>
        ///     Gets an enumerable of the pins in this collection
        /// </summary>
        public new IEnumerable<InputPin> Pins => base.Pins.Cast<InputPin>();

        /// <summary>
        ///     Gets an enumerable of the values of the pins in this collection
        /// </summary>
        public IEnumerable Values => Pins.Where(p => p.Value != null).Select(p => p.Value);

        #endregion
    }
}
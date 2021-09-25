using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Artemis.Core.Events;

namespace Artemis.Core
{
    /// <inheritdoc cref="IPinCollection"/>
    public abstract class PinCollection : IPinCollection
    {
        #region Constructors

        /// <summary>
        ///     Creates a new instance of the <see cref="PinCollection" /> class
        /// </summary>
        /// <param name="node">The node the pin collection belongs to</param>
        /// <param name="name">The name of the pin collection</param>
        /// <param name="initialCount">The amount of pins to initially add to the collection</param>
        /// <returns>The resulting output pin collection</returns>
        protected PinCollection(INode node, string name, int initialCount)
        {
            Node = node ?? throw new ArgumentNullException(nameof(node));
            Name = name;

            for (int i = 0; i < initialCount; i++)
                AddPin();
        }

        #endregion

        /// <inheritdoc />
        public event EventHandler<SingleValueEventArgs<IPin>>? PinAdded;

        /// <inheritdoc />
        public event EventHandler<SingleValueEventArgs<IPin>>? PinRemoved;

        #region Properties & Fields

        /// <inheritdoc />
        public INode Node { get; }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public abstract PinDirection Direction { get; }

        /// <inheritdoc />
        public abstract Type Type { get; }

        private readonly ObservableCollection<IPin> _pins = new();

        /// <summary>
        ///     Gets a read only observable collection of the pins
        /// </summary>
        public ReadOnlyObservableCollection<IPin> Pins => new(_pins);

        #endregion

        #region Methods

        /// <inheritdoc />
        public IPin AddPin()
        {
            IPin pin = CreatePin();
            _pins.Add(pin);

            PinAdded?.Invoke(this, new SingleValueEventArgs<IPin>(pin));

            return pin;
        }

        /// <inheritdoc />
        public bool Remove(IPin pin)
        {
            bool removed = _pins.Remove(pin);

            if (removed)
                PinRemoved?.Invoke(this, new SingleValueEventArgs<IPin>(pin));

            return removed;
        }

        /// <inheritdoc />
        public void Reset()
        {
            foreach (IPin pin in _pins) 
                pin.Reset();
        }

        /// <summary>
        ///     Creates a new pin to be used in this collection
        /// </summary>
        /// <returns>The resulting pin</returns>
        protected abstract IPin CreatePin();

        /// <inheritdoc />
        public IEnumerator<IPin> GetEnumerator()
        {
            return Pins.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
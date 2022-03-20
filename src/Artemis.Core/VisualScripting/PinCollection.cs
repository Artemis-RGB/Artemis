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
        /// <returns>The resulting output pin collection</returns>
        protected PinCollection(INode node, string name)
        {
            Node = node ?? throw new ArgumentNullException(nameof(node));
            Name = name;
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
        public void Add(IPin pin)
        {
            if (pin.Direction != Direction)
                throw new ArtemisCoreException($"Can't add a {pin.Direction} pin to an {Direction} pin collection.");
            if (pin.Type != Type)
                throw new ArtemisCoreException($"Can't add a {pin.Type} pin to an {Type} pin collection.");

            if (_pins.Contains(pin))
                return;

            _pins.Add(pin);
            PinAdded?.Invoke(this, new SingleValueEventArgs<IPin>(pin));
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

        /// <inheritdoc />
        public abstract IPin CreatePin();

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
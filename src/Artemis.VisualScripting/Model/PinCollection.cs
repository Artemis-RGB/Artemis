using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Artemis.Core.VisualScripting;

namespace Artemis.VisualScripting.Model
{
    public abstract class PinCollection : IPinCollection
    {
        #region Properties & Fields

        public INode Node { get; }
        public string Name { get; }

        public abstract PinDirection Direction { get; }
        public abstract Type Type { get; }

        private readonly ObservableCollection<IPin> _pins = new();
        public ReadOnlyObservableCollection<IPin> Pins => new(_pins);

        #endregion

        #region Constructors

        protected PinCollection(INode node, string name, int initialCount)
        {
            this.Node = node;
            this.Name = name;

            for (int i = 0; i < initialCount; i++)
                AddPin();
        }

        #endregion

        #region Methods

        public IPin AddPin()
        {
            IPin pin = CreatePin();
            _pins.Add(pin);
            return pin;
        }

        public bool Remove(IPin pin) => _pins.Remove(pin);

        protected abstract IPin CreatePin();

        public IEnumerator<IPin> GetEnumerator() => Pins.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}

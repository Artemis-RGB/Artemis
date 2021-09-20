using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Artemis.Core
{
    public sealed class InputPinCollection<T> : PinCollection
    {
        #region Properties & Fields

        public override PinDirection Direction => PinDirection.Input;
        public override Type Type => typeof(T);

        public new IEnumerable<InputPin<T>> Pins => base.Pins.Cast<InputPin<T>>();

        public IEnumerable<T> Values => Pins.Select(p => p.Value);

        #endregion

        #region Constructors

        internal InputPinCollection(INode node, string name, int initialCount)
            : base(node, name, initialCount)
        { }

        #endregion

        #region Methods

        protected override IPin CreatePin() => new InputPin<T>(Node, string.Empty);

        #endregion
    }

    public sealed class InputPinCollection : PinCollection
    {
        #region Properties & Fields

        public override PinDirection Direction => PinDirection.Input;
        public override Type Type { get; }

        public new IEnumerable<InputPin> Pins => base.Pins.Cast<InputPin>();

        public IEnumerable Values => Pins.Select(p => p.Value);

        #endregion

        #region Constructors

        internal InputPinCollection(INode node, Type type, string name, int initialCount)
            : base(node, name, 0)
        {
            this.Type = type;

            // Can't do this in the base constructor because the type won't be set yet
            for (int i = 0; i < initialCount; i++)
                AddPin();
        }

        #endregion

        #region Methods

        protected override IPin CreatePin() => new InputPin(Node, Type, string.Empty);

        #endregion
    }
}

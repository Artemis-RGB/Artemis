using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.VisualScripting;

namespace Artemis.VisualScripting.Model
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
}
